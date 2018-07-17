using System;
using Microsoft.Extensions.Logging;

namespace Community.AspNetCore.ExceptionHandling.Handlers
{
    public class HandlerWithLogger
    {
        private readonly HandlerWithLoggerOptions _options;
        private readonly ILoggerFactory _loggerFactory;

        public HandlerWithLogger(HandlerWithLoggerOptions options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected ILogger Logger => this._loggerFactory.CreateLogger(_options.LoggerCategory);
    }
}