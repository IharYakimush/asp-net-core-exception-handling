using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling
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
        public static IServiceCollection AddExceptionHandlingPolicies(this IServiceCollection services, Action<ExceptionHandlingPolicyOptions> options = null)
        {
            services.TryAddSingleton<ExceptionHandlingPolicyMiddleware>();
            if (options != null)
            {
                services.Configure(options);
            }

            services.TryAddSingleton<ReThrowExceptionHandler>();
            services.TryAddSingleton<LogExceptionHandler>();

            return services;
        }

        public static IApplicationBuilder UseExceptionHandlingPolicies(this IApplicationBuilder app, ExceptionHandlingPolicyOptions options)
        {
            return app.UseMiddleware<ExceptionHandlingPolicyMiddleware>(Options.Create(options));
        }
    }
}