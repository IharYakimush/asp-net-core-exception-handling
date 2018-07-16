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

        /// <summary>
        /// The behaviour in case of retry can't be executed due to responce already started. Default: re throw.
        /// </summary>
        public ResponseAlreadyStartedBehaviour ResponseAlreadyStartedBehaviour { get; set; } = ResponseAlreadyStartedBehaviour.ReThrow;

        /// <summary>
        /// Max retry count
        /// </summary>
        public int MaxRetryCount { get; set; } = 1;
    }
}
