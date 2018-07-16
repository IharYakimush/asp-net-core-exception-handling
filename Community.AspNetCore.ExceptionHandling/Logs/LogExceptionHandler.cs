﻿using System;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Logs
{
    public class LogExceptionHandler<TException> : IExceptionHandler
    where TException : Exception
    {
        private readonly IOptions<LogHandlerOptions<TException>> _settings;

        private static readonly EventId DefaultEvent = new EventId(500, "UnhandledException");

        public LogHandlerOptions<TException> Settings => this._settings.Value;

        public LogExceptionHandler(IOptions<LogHandlerOptions<TException>> settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            var logLevel = this.Settings.Level?.Invoke(httpContext, exception) ?? LogLevel.Error;

            if (logLevel == LogLevel.None)
            {
                return Task.FromResult(HandlerResult.NextHandler);
            }

            if (exception.Data.Contains(DisableLoggingHandler.DisableLoggingFlagKey))
            {
                return Task.FromResult(HandlerResult.NextHandler);
            }

            if (httpContext.RequestServices.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory)
            {
                ILogger logger =
                    loggerFactory.CreateLogger(this.Settings.Category?.Invoke(httpContext, exception) ??
                                               Const.Category);

                EventId eventId = this.Settings.EventIdFactory != null
                    ? this.Settings.EventIdFactory(httpContext, exception)
                    : DefaultEvent;                                

                object state = this.Settings.StateFactory?.Invoke(httpContext, exception, this.Settings) ??
                               new FormattedLogValues("Unhandled error occured. RequestId: {requestId}.",
                                   httpContext.TraceIdentifier);

                Func<object, Exception, string> formatter = this.Settings.Formatter ?? ((o, e) => o.ToString());

                logger.Log(logLevel, eventId, state, exception, formatter);                
            }
            
            return Task.FromResult(HandlerResult.NextHandler);
        }
    }
}