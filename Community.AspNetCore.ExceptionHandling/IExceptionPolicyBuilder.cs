using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.ExceptionHandling
{
    public interface IExceptionPolicyBuilder
    {
        IServiceCollection Services { get; }

        ExceptionHandlingPolicyOptions Options { get; }
    }
}