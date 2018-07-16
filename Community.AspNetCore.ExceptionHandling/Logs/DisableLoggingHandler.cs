using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace Commmunity.AspNetCore.ExceptionHandling.Logs
{
    class DisableLoggingHandler : IExceptionHandler
    {
        public const string DisableLoggingFlagKey = "427EDE68BE9A";
                
        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            exception.Data[DisableLoggingFlagKey] = string.Empty;
            
            return Task.FromResult(HandlerResult.NextHandler);
        }
    }
}
