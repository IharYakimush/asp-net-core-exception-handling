using System;
using Microsoft.Extensions.DependencyInjection;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public class PolicyBuilder : ExceptionHandlingPolicyOptions
    {
        private readonly IServiceCollection _services;

        public PolicyBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}