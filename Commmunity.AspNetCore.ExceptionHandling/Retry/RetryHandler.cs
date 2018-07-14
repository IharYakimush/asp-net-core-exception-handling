using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Retry
{
    class RetryHandler<TException> : IExceptionHandler
    where TException : Exception
    {
        private const string CurrentRetryGuardKey = "71DAAFEC7B56";

        private readonly RetryHandlerOptions<TException> options;

        public RetryHandler(IOptions<RetryHandlerOptions<TException>> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options.Value;
        }

        public Task<HandlerResult> Handle(HttpContext httpContext, Exception exception)
        {
            if (httpContext.Response.HasStarted)
            {
                // Retry is not possible, so let's next handler decide
                return Task.FromResult(HandlerResult.NextHandler);
            }

            if (!httpContext.Items.ContainsKey(CurrentRetryGuardKey))
            {
                httpContext.Items[CurrentRetryGuardKey] = 0;
            }

            if ((int)httpContext.Items[CurrentRetryGuardKey] < this.options.MaxRetryCount)
            {
                httpContext.Items[CurrentRetryGuardKey] = (int)httpContext.Items[CurrentRetryGuardKey] + 1;
                return Task.FromResult(HandlerResult.Retry);
            }
            else
            {
                // Retry is not possible, so let's next handler decide
                return Task.FromResult(HandlerResult.NextHandler);
            }                   
        }
    }
}
