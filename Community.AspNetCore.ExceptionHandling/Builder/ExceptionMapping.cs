using System;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.ExceptionHandling.Builder
{
    public class ExceptionMapping<TException> : IResponseHandlers<TException>
        where TException : Exception
    {
        public IExceptionPolicyBuilder Builder { get; }

        internal ExceptionMapping(IExceptionPolicyBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public IServiceCollection Services => Builder.Services;

        public ExceptionHandlingPolicyOptions Options => Builder.Options;
    }
}