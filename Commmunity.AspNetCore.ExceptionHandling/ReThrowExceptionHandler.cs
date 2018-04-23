using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public class ReThrowExceptionHandler : IExceptionHandler
    {
        public Task<bool> Handle(HttpContext httpContext, Exception exception, RequestDelegate next)
        {
            return Task.FromResult(true);
        }
    }
}