using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Mvc;
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
                options.For<DuplicateNameException>().Retry().NextPolicy();

                options.For<DuplicateWaitObjectException>().Retry();
                
                options.For<ArgumentOutOfRangeException>().Log().Rethrow();
                
                options.For<InvalidCastException>()
                    .Response(e => 400)
                    .Headers((h, e) => h["X-qwe"] = e.Message)
                    .WithBody((req,sw, exception) => sw.WriteAsync(exception.ToString()))
                    .NextPolicy();

                options.For<Exception>()
                    .Log(lo => { lo.Formatter = (o, e) => "qwe"; })
                    //.Response(e => 500, ResponseAlreadyStartedBehaviour.GoToNextHandler).ClearCacheHeaders().WithBodyJson((r, e) => new { msg = e.Message, path = r.Path })
                    .Response(e => 500, ResponseAlreadyStartedBehaviour.GoToNextHandler).ClearCacheHeaders().WithObjectResult((r, e) => new { msg = e.Message, path = r.Path })
                    .Handled();
            });
            
            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseBuffering().UseDeveloperExceptionPage().UseExceptionHandlingPolicies();
            app.UseMvc();
        }
    }
}
