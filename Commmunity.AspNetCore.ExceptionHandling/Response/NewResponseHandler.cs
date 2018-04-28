using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class NewResponseHandler<TException> : RawResponseExceptionHandler<TException>
    where TException : Exception
    {
        public NewResponseHandler(IOptions<NewResponseOptions<TException>> options,
            ILoggerFactory loggerFactory) : base(options.Value, loggerFactory)
        {
        }
    }
}