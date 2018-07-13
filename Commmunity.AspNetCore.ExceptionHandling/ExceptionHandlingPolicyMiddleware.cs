using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
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

            bool mappingExists = false;
            HandlerResult handleResult = HandlerResult.ReThrow;

            foreach (Type type in policyOptions.GetExceptionsInternal())
            {
                if (type.IsAssignableFrom(exceptionType))
                {
                    mappingExists = true;
                    handleResult = await EnumerateHandlers(context, type, exception, policyOptions, logger);

                    if (handleResult == HandlerResult.ReThrow)
                    {
                        return true;
                    }

                    if (handleResult != HandlerResult.NextChain)
                    {
                        break;
                    }
                }
            }

            if (!mappingExists)
            {
                logger.LogWarning(Events.PolicyNotFound,
                    "Handlers mapping for exception type {exceptionType} not exists. Exception will be re-thrown. RequestId: {RequestId}",
                    exceptionType, context.TraceIdentifier);

                return false;
            }

            return handleResult == HandlerResult.ReThrow;
        }

        private static async Task<HandlerResult> EnumerateHandlers(
            HttpContext context, 
            Type exceptionType,
            Exception exception, 
            ExceptionHandlingPolicyOptions policyOptions, 
            ILogger logger)
        {
            bool handlerExecuted = false;
            HandlerResult handleResult = HandlerResult.ReThrow;

            IEnumerable<Type> handlers = policyOptions.GetHandlersInternal(exceptionType);

            foreach (Type handlerType in handlers)
            {                
                try
                {
                    IExceptionHandler handler =
                        context.RequestServices.GetService(handlerType) as IExceptionHandler;

                    if (handler == null)
                    {
                        handlerExecuted = false;
                        logger.LogError(Events.HandlerNotCreated,
                            "Handler type {handlerType} can't be created because it not registered in IServiceProvider. RequestId: {RequestId}",
                            handlerType, context.TraceIdentifier);
                    }
                    else
                    {
                        handleResult = await handler.Handle(context, exception);
                        handlerExecuted = true;
                    }                    
                }
                catch (Exception e)
                {
                    logger.LogError(Events.HandlerError, e,
                        "Unhandled exception executing handler of type {handlerType} on exception of type {exceptionType}. RequestId: {RequestId}",
                        handlerType, exceptionType, context.TraceIdentifier);

                    return HandlerResult.ReThrow;
                }

                if (handleResult != HandlerResult.NextHandler)
                {
                    break;
                }
            }

            if (!handlerExecuted)
            {
                logger.LogWarning(Events.HandlersNotFound,
                    "Handlers collection for exception type {exceptionType} is empty. Exception will be re-thrown. RequestId: {RequestId}",
                    exceptionType, context.TraceIdentifier);

                return HandlerResult.ReThrow;
            }

            return handleResult;
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