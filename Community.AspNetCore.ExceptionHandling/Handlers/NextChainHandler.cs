using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling.Handlers
{
    public class NextChainHandler : IExceptionHandler
    {
        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            return Task.FromResult(HandlerResult.NextChain);
        }
    }
}