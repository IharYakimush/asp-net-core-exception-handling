using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling.Response
{
    public class RawResponseHandlerOptions<TException> : HandlerWithLoggerOptions,
        IOptions<RawResponseHandlerOptions<TException>>
        where TException : Exception
    {
        public List<Func<HttpContext, TException, Task>> SetResponse { get; set; } = new List<Func<HttpContext, TException, Task>>();
        public RawResponseHandlerOptions<TException> Value => this;
        public ResponseAlreadyStartedBehaviour ResponseAlreadyStartedBehaviour { get; set; } = ResponseAlreadyStartedBehaviour.ReThrow;
    }
}