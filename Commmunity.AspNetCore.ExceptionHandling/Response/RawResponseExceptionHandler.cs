using System;
using System.Threading;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Commmunity.AspNetCore.ExceptionHandling.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class RawResponseExceptionHandler<TException> : HandlerStrongType<TException>
        where TException : Exception
    {
        private readonly RawResponseHandlerOptions<TException> _options;

        private static readonly EventId ResponseHasStarted = new EventId(127, "ResponseAlreadyStarted");

        public RawResponseExceptionHandler(IOptions<RawResponseHandlerOptions<TException>> options, ILoggerFactory loggerFactory)
            : base(options.Value, loggerFactory)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task<HandlerResult> HandleStrongType(HttpContext httpContext, TException exception)
        {
            if (httpContext.Response.HasStarted)
            {
                if (this._options.RequestStartedBehaviour == RequestStartedBehaviour.ReThrow)
                {
                    this.Logger.LogError(ResponseHasStarted,
                        "Unable to execute {handletType} handler, because respnse already started. Exception will be re-thrown.",
                        this.GetType());

                    return HandlerResult.ReThrow;
                }
                else
                {
                    return HandlerResult.NextHandler;
                }
                
            }
            
            await HandleResponseAsync(httpContext, exception);

            return HandlerResult.NextHandler;
        }

        protected virtual Task HandleResponseAsync(HttpContext httpContext, TException exception)
        {
            Task result = Task.CompletedTask;

            foreach (Func<HttpContext, TException, Task> func in this._options.SetResponse)
            {
                result = result.ContinueWith(task => func(httpContext, exception));                
            }

            return result;                      
        }
    }
}