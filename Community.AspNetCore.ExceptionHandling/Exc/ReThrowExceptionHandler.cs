using System;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace Community.AspNetCore.ExceptionHandling.Exc
{
    public class ReThrowExceptionHandler : IExceptionHandler
    {
        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            return Task.FromResult(HandlerResult.ReThrow);
        }
    }
}