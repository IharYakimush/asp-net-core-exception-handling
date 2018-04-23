using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public class LogExceptionHandler : IExceptionHandler
    {
        private readonly Func<HttpContext, Exception, EventId> _eventIdFactory;
        private readonly Func<HttpContext, Exception, string> _messageFactory;
        private readonly Func<HttpContext, Exception, object[]> _parametersFactory;
        private readonly string _category;

        public LogExceptionHandler(string category = null, Func<HttpContext, Exception, EventId> eventIdFactory = null, Func<HttpContext, Exception, string> messageFactory = null, Func<HttpContext, Exception, object[]> parametersFactory = null)
        {
            _eventIdFactory = eventIdFactory;
            _messageFactory = messageFactory;
            _parametersFactory = parametersFactory;
            _category = category ?? nameof(LogExceptionHandler);
        }
        public Task<bool> Handle(HttpContext httpContext, Exception exception, RequestDelegate next)
        {
            ILoggerFactory loggerFactory =
                httpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

            if (loggerFactory != null)
            {
                ILogger logger = loggerFactory.CreateLogger(this._category);

                logger.LogError(_eventIdFactory?.Invoke(httpContext, exception) ?? new EventId(500, "UnhandledException"),
                    exception,
                    this._messageFactory?.Invoke(httpContext, exception) ?? "Unhandled error occured. RequestId: {requestId}.",
                    this._parametersFactory?.Invoke(httpContext, exception) ?? new object[] { httpContext.TraceIdentifier });
            }
            
            return Task.FromResult(false);
        }
    }
}