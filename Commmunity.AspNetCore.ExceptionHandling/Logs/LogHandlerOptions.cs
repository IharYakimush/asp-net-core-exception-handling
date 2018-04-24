using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Logs
{
    public class LogHandlerOptions<TException> : IOptions<LogHandlerOptions<TException>>
    where TException : Exception
    {
        public LogHandlerOptions<TException> Value => this;

        public Func<HttpContext, Exception, EventId> EventIdFactory { get; set; }

        public Func<object, Exception, string> Formatter { get; set; }

        public Func<HttpContext, Exception, LogHandlerOptions<TException>, object> StateFactory { get; set; }

        public string Category { get; set; }
        public LogLevel Level { get; set; } = LogLevel.Error;
    }
}