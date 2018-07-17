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
        /// Factory for <see cref="EventId"/>
        /// </summary>
        public Func<HttpContext, TException, EventId> EventIdFactory { get; set; }

        /// <summary>
        /// The Formatter. By default state.ToString(). 
        /// </summary>
        public Func<object, TException, string> Formatter { get; set; }

        /// <summary>
        /// Foctory for log entry state. By default <see cref="FormattedLogValues"/> with TraceIdentifier.
        /// </summary>
        public Func<HttpContext, TException, LogHandlerOptions<TException>, object> StateFactory { get; set; }

        /// <summary>
        /// Factory for log category. By default "Commmunity.AspNetCore.ExceptionHandling" will be used.
        /// </summary>
        public Func<HttpContext, TException, string> Category { get; set; }

        /// <summary>
        /// Factory for <see cref="LogLevel"/> log level. By default <see cref="LogLevel.Error"/> error will be used. In case of <see cref="LogLevel.None"/> none log entry will be skipped.
        /// </summary>
        public Func<HttpContext, TException, LogLevel> Level { get; set; }
    }
}