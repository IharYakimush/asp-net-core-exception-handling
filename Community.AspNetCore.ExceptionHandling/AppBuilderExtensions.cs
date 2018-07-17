using System;
using Community.AspNetCore.ExceptionHandling.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.ExceptionHandling
{
    //TODO: add warning for policy override
    //TODO: add response handler
    //TODO: add retry handler
    //TODO: policy builder
    //TODO: add api exception and handler
    //TODO: add terminate policies pipeline handler ???
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingPolicies(this IApplicationBuilder app)
        {            
            return app.UseMiddleware<ExceptionHandlingPolicyMiddleware>();
        }
        public static IServiceCollection AddExceptionHandlingPolicies(this IServiceCollection services, Action<IExceptionPolicyBuilder> builder)
        {
            PolicyBuilder policyBuilder = new PolicyBuilder(services);
            builder?.Invoke(policyBuilder);
            services.TryAddSingleton<IOptions<ExceptionHandlingPolicyOptions>>(policyBuilder.Options);
            services.TryAddSingleton<ExceptionHandlingPolicyMiddleware>();

            return policyBuilder;
        }
    }
}