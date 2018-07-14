using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commmunity.AspNetCore.ExceptionHandling.Retry
{
    public class RetryHandlerOptions<TException> : IOptions<RetryHandlerOptions<TException>>
    where TException : Exception
    {
        public RetryHandlerOptions<TException> Value => this;

        public RequestStartedBehaviour RequestStartedBehaviour { get; set; } = RequestStartedBehaviour.ReThrow;

        public int MaxRetryCount { get; set; } = 1;
    }
}
