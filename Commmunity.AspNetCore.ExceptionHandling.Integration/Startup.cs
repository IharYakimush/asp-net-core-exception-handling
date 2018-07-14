using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
                options.For<DuplicateNameException>().Retry().NextChain();
                options.For<DuplicateWaitObjectException>().Retry();
                
                options.For<ArgumentOutOfRangeException>().Log().Rethrow();
                
                options.For<InvalidCastException>()
                    .Response(e => 400)
                    .WithHeaders((h, e) => h["X-qwe"] = e.Message)
                    .WithBody((stream, exception) =>
                    {
                        using (StreamWriter sw = new StreamWriter(stream))
                        {
                            return sw.WriteAsync(exception.ToString());
                        }                                                
                    })
                    .NextChain();
                options.For<Exception>()
                    .Log(lo => { lo.Formatter = (o, e) => "qwe"; })
                    .Response(e => 500, RequestStartedBehaviour.SkipHandler).WithBody(
                    async (stream, exception) =>
                    {
                        using (StreamWriter sw = new StreamWriter(stream))
                        {                            
                            await sw.WriteAsync("unhandled exception");
                        }
                    })
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
