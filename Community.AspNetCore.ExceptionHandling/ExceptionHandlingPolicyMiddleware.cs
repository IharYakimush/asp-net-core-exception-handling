using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling
{
    public class ExceptionHandlingPolicyMiddleware : IMiddleware
    {
        public const int MaxRetryIterations = 10;

        private readonly IOptions<ExceptionHandlingPolicyOptions> options;

        public ExceptionHandlingPolicyMiddleware(IOptions<ExceptionHandlingPolicyOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }       

        private static async Task<HandlerResult> EnumerateExceptionMapping(
            HttpContext context, 
            ExceptionHandlingPolicyOptions policyOptions,
            Exception exception,
            ILogger logger)
        {
            Type exceptionType = exception.GetType();            

            bool mappingExists = false;
            HandlerResult handleResult = HandlerResult.ReThrow;

            foreach (Type type in policyOptions.GetExceptionsInternal())
            {
                if (type.IsAssignableFrom(exceptionType))
                {
                    mappingExists = true;
                    handleResult = await EnumerateHandlers(context, type, exception, policyOptions, logger);
                                        
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

                return HandlerResult.ReThrow;
            }

            return handleResult;
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
            ILogger logger = context.RequestServices.GetService<ILogger<ExceptionHandlingPolicyMiddleware>>() ??
                             NullLoggerFactory.Instance.CreateLogger(Const.Category);

            await InvokeWithRetryAsync(context, next, logger, 0);
        }   
        
        private async Task InvokeWithRetryAsync(HttpContext context, RequestDelegate next, ILogger logger, int iteration)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                ExceptionHandlingPolicyOptions policyOptions = this.options.Value;

                var result = await EnumerateExceptionMapping(context, policyOptions, exception, logger);

                if (result == HandlerResult.ReThrow)
                {
                    throw;
                }

                if (result == HandlerResult.Retry)
                {
                    // We can't do anything if the response has already started, just abort.
                    if (context.Response.HasStarted)
                    {
                        logger.LogWarning(Events.RetryForStartedResponce,
                        exception,
                            "Retry requested when responce already started. Exception will be re-thrown. RequestId: {RequestId}",
                            context.TraceIdentifier);

                        throw;
                    }

                    if (iteration > MaxRetryIterations)
                    {
                        logger.LogCritical(Events.RetryIterationExceedLimit,
                        exception,
                            "Retry iterations count exceed limit of {limit}. Possible issues with retry policy configuration. Exception will be re-thrown. RequestId: {RequestId}",
                            MaxRetryIterations,
                            context.TraceIdentifier);

                        throw;
                    }

                    logger.LogWarning(Events.Retry,
                        exception,
                            "Retry requested. Iteration {iteration} RequestId: {RequestId}",
                            iteration,
                            context.TraceIdentifier);

                    context.Response.Headers.Clear();

                    await InvokeWithRetryAsync(context, next, logger, iteration + 1);
                }

                if (result != HandlerResult.Handled)
                {
                    logger.LogWarning(Events.UnhandledResult,
                        exception,
                            "After execution of all handlers exception was not marked as handled and  will be re thrown. RequestId: {RequestId}",
                            context.TraceIdentifier);

                    throw;
                }
            }
        }
    }
}