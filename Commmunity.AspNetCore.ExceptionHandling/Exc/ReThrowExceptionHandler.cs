using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling.Exc
{
    public class ReThrowExceptionHandler : IExceptionHandler
    {
        public Task<bool> Handle(HttpContext httpContext, Exception exception)
        {
            return Task.FromResult(true);
        }
    }
}