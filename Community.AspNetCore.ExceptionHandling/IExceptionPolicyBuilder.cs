using Microsoft.Extensions.DependencyInjection;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public interface IExceptionPolicyBuilder
    {
        IServiceCollection Services { get; }

        ExceptionHandlingPolicyOptions Options { get; }
    }
}