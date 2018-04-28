using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class RawResponseHandlerOptions<TException> : HandlerWithLoggerOptions,
        IOptions<RawResponseHandlerOptions<TException>>
        where TException : Exception
    {
        public Func<HttpContext, TException, Task> SetResponse { get; set; } = null;
        public RawResponseHandlerOptions<TException> Value => this;
    }
}