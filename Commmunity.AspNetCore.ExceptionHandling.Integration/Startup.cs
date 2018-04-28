using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Integration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddExceptionHandlingPolicies(options =>
            {
                options.For<ArgumentOutOfRangeException>().AddLog().AddRethrow();
                options.For<InvalidCastException>().AddLog().AddStatusCode(e => 400).AddHeaders((h, e) => h["X-qwe"] = e.Message);
            });

            services.AddLogging(b => b.AddConsole());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage().UseExceptionHandlingPolicies();
            app.UseMvc();
        }
    }
}
