using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling.Response
{
    public class RawResponseExceptionHandler<TException> : HandlerStrongType<TException>
        where TException : Exception
    {
        public const string StatusCodeSetKey = "5D1CFED34A39";

        public const string BodySetKey = "6D1CFED34A39";

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
                if (this._options.ResponseAlreadyStartedBehaviour == ResponseAlreadyStartedBehaviour.ReThrow)
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

        public static Task SetStatusCode(HttpContext context, TException exception, Func<TException, int> statusCodeFactory)
        {
            if(statusCodeFactory == null)
            {
                if (!context.Items.ContainsKey(StatusCodeSetKey))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = statusCodeFactory.Invoke(exception);
                context.Items[StatusCodeSetKey] = true;
            }

            return Task.CompletedTask;
        }

        public static Task SetBody(HttpContext context, TException exception, Func<HttpRequest, Stream, TException, Task> settings) 
        {
            if (!context.Items.ContainsKey(BodySetKey))
            {
                context.Items[BodySetKey] = true;

                Stream stream = context.Response.Body;

                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.SetLength(0);
                }

                if (stream.CanWrite)
                {
                    return settings(context.Request, stream, exception);                    
                }
                else
                {
                    throw new InvalidOperationException("Unable to write to response stream");
                }
            }

            return Task.CompletedTask;
        }
    }
}