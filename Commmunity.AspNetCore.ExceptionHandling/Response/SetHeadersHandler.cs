using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class SetHeadersHandler<TException> : RawResponseExceptionHandler<TException>
        where TException : Exception
    {
        public SetHeadersHandler(IOptions<SetHeadersOptions<TException>> options,
            ILoggerFactory loggerFactory) : base(options.Value, loggerFactory)
        {
        }
    }
}