using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public class ExceptionHandlingPolicyMiddleware : IMiddleware
    {
        private readonly IOptions<ExceptionHandlingPolicyOptions> options;

        public ExceptionHandlingPolicyMiddleware(IOptions<ExceptionHandlingPolicyOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }       

        private static async Task<bool> EnumerateExceptionMapping(
            HttpContext context, 
            ExceptionHandlingPolicyOptions policyOptions,
            Exception exception)
        {
            Type exceptionType = exception.GetType();

            ILogger logger = context.RequestServices.GetService<ILogger<ExceptionHandlingPolicyMiddleware>>() ??
                             NullLoggerFactory.Instance.CreateLogger(Const.Category);

            bool? throwRequired = null;

            foreach (Type type in policyOptions.GetExceptionsInternal())
            {
                if (type.IsAssignableFrom(exceptionType))
                {                    
                    throwRequired = await EnumerateHandlers(context, exception, policyOptions, logger);

                    break;
                }
            }

            if (!throwRequired.HasValue)
            {
                logger.LogWarning(Events.PolicyNotFound,
                    "Handlers mapping for exception type {exceptionType} not exists. Exception will be re-thrown. RequestId: {RequestId}",
                    exceptionType, context.TraceIdentifier);
            }

            return throwRequired ?? true;
        }

        private static async Task<bool> EnumerateHandlers(
            HttpContext context, 
            Exception exception, 
            ExceptionHandlingPolicyOptions policyOptions, 
            ILogger logger)
        {
            bool? throwRequired = null;
            Type exceptionType = exception.GetType();

            IEnumerable<Type> handlers = policyOptions.GetHandlersInternal(exceptionType);

            foreach (Type handlerType in handlers)
            {                
                try
                {
                    IExceptionHandler handler =
                        context.RequestServices.GetService(handlerType) as IExceptionHandler;

                    if (handler == null)
                    {
                        throwRequired = true;
                        logger.LogError(Events.HandlerNotCreated,
                            "Handler type {handlerType} can't be created because it not registered in IServiceProvider. RequestId: {RequestId}",
                            handlerType, context.TraceIdentifier);
                    }
                    else
                    {
                        throwRequired = await handler.Handle(context, exception);
                    }                    
                }
                catch (Exception e)
                {
                    logger.LogError(Events.HandlerError, e,
                        "Unhandled exception executing handler of type {handlerType} on exception of type {exceptionType}. RequestId: {RequestId}",
                        handlerType, exceptionType, context.TraceIdentifier);
                    throwRequired = true;
                }

                if (throwRequired.Value)
                {
                    break;
                }
            }

            if (!throwRequired.HasValue)
            {
                logger.LogWarning(Events.HandlersNotFound,
                    "Handlers collection for exception type {exceptionType} is empty. Exception will be re-thrown. RequestId: {RequestId}",
                    exceptionType, context.TraceIdentifier);
            }

            return throwRequired ?? true;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                ExceptionHandlingPolicyOptions policyOptions = this.options.Value;

                bool throwRequired = await EnumerateExceptionMapping(context, policyOptions, exception);

                if (throwRequired)
                {
                    throw;
                }
            }
        }
    }
}