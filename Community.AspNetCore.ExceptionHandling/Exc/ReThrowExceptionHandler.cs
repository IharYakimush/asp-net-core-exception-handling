using System;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling.Exc
{
    public class ReThrowExceptionHandler : IExceptionHandler
    {
        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            return Task.FromResult(HandlerResult.ReThrow);
        }
    }
}