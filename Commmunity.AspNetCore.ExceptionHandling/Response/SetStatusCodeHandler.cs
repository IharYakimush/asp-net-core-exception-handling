using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class SetStatusCodeHandler<TException> : RawResponseExceptionHandler<TException>
    where TException : Exception
    {
        public SetStatusCodeHandler(IOptions<SetStatusCodeOptions<TException>> options,
            ILoggerFactory loggerFactory) : base(options.Value, loggerFactory)
        {
        }
    }
}