using System;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling.Logs
{
    public class LogExceptionHandler<TException> : HandlerStrongType<TException>
    where TException : Exception
    {
        private readonly IOptions<LogHandlerOptions<TException>> _settings;

        private static readonly EventId DefaultEvent = new EventId(500, "UnhandledException");

        private static readonly EventId LogActionErrorEvent = new EventId(501, "LogActionError");

        public LogHandlerOptions<TException> Settings => this._settings.Value;

        public LogExceptionHandler(IOptions<LogHandlerOptions<TException>> settings, ILoggerFactory loggerFactory):base(settings.Value, loggerFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override Task<HandlerResult> HandleStrongType(HttpContext httpContext, TException exception)
        {            
            if (exception.Data.Contains(DisableLoggingHandler.DisableLoggingFlagKey))
            {
                return Task.FromResult(HandlerResult.NextHandler);
            }

            if (httpContext.RequestServices.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory)
            {
                ILogger logger =
                    loggerFactory.CreateLogger(this.Settings.Category?.Invoke(httpContext, exception) ??
                                               Const.Category);

                Action<ILogger, HttpContext, TException> logAction = this.Settings.LogAction ?? LogDefault;

                try
                {
                    logAction(logger, httpContext, exception);
                }
                catch (Exception logException)
                {
                    try
                    {
                        logger.LogWarning(DefaultEvent, logException, "Unhandled error occured in log action.");
                    }
                    catch
                    {
                        // don't fail in case of errors with this log
                    }

                    if (this.Settings.RethrowLogActionExceptions)
                    {
                        throw;
                    }
                }
            }

            return Task.FromResult(HandlerResult.NextHandler);
        }

        private static void LogDefault(ILogger logger, HttpContext context, TException exception)
        {
            logger.LogError(DefaultEvent, exception, "Unhandled error occured. RequestId: {requestId}.", context.TraceIdentifier);
        }
    }
}