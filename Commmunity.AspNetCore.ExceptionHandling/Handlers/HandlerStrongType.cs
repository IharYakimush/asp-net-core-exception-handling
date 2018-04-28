using System;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public abstract class HandlerStrongType<TException> : HandlerWithLogger, IExceptionHandler
        where TException : Exception
    {
        private static readonly EventId ExceptionTypeNotMatchGenericType = new EventId(136, "ExceptionTypeNotMatchGenericType");

        protected HandlerStrongType(HandlerWithLoggerOptions options, ILoggerFactory loggerFactory) : base(options,
            loggerFactory)
        {
        }

        public async Task<bool> Handle(HttpContext httpContext, Exception exception)
        {            
            if (exception is TException e)
            {
                return await this.HandleStrongType(httpContext, e);
            }
            else
            {
                this.Logger.LogError(ExceptionTypeNotMatchGenericType,
                    "Excpetion type {exceptionType} not match current generic type {genericType}. Exception will be re-thrown.",
                    exception.GetType(), typeof(TException));

                return true;
            }
        }

        protected abstract Task<bool> HandleStrongType(HttpContext httpContext, TException exception);
    }
}