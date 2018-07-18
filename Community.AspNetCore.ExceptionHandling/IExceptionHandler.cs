using System;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace Community.AspNetCore.ExceptionHandling
{
    public interface IExceptionHandler
    {
        Task<HandlerResult> Handle(HttpContext httpContext, Exception exception);
    }
}