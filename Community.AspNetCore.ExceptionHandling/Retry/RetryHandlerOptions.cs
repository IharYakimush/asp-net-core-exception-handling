using System;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling.Retry
{
    public class RetryHandlerOptions<TException> : IOptions<RetryHandlerOptions<TException>>
    where TException : Exception
    {
        public RetryHandlerOptions<TException> Value => this;
        
        /// <summary>
        /// Max retry count
        /// </summary>
        public int MaxRetryCount { get; set; } = 1;
    }
}
