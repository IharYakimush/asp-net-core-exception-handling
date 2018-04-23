using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public interface IExceptionHandler
    {
        Task<bool> Handle(HttpContext httpContext, Exception exception, RequestDelegate next);
    }
}