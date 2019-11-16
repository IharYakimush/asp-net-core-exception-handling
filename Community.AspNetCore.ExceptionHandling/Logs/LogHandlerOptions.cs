using System;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling.Logs
{
    /// <summary>
    /// The log handler options
    /// </summary>
    /// <typeparam name="TException">
    /// The exception type
    /// </typeparam>
    public class LogHandlerOptions<TException> : HandlerWithLoggerOptions, IOptions<LogHandlerOptions<TException>>
    where TException : Exception
    {
        public LogHandlerOptions<TException> Value => this;

        /// <summary>
        /// Action to log exception. If not set logger.LogError("Unhandled error occured. RequestId: {requestId}.", httpContext.TraceIdentifier); will be used by default.
        /// </summary>
        public Action<ILogger, HttpContext, TException> LogAction { get; set; }

        /// <summary>
        /// Factory for log category. By default "Community.AspNetCore.ExceptionHandling" will be used.
        /// </summary>
        public Func<HttpContext, TException, string> Category { get; set; }

        /// <summary>
        /// Rethrow exception from LogAction.
        /// </summary>
        public bool RethrowLogActionExceptions { get; set; } = false;
    }
}