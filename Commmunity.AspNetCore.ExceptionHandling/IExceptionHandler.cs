using System;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public interface IExceptionHandler
    {
        Task<HandlerResult> Handle(HttpContext httpContext, Exception exception);
    }
}