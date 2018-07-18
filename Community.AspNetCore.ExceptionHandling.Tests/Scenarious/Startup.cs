using System;
using System.Data;
using System.Text;
using Community.AspNetCore.ExceptionHandling.Mvc;
using Community.AspNetCore.ExceptionHandling.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.ExceptionHandling.Tests.Scenarious
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
                options.For<RethrowException>().Rethrow();

                options.For<NextEmptyException>().Log();

                options.For<HandledWithoutResponseException>().Handled();

                options.For<EmptyChainException>();

                options.For<NotBaseException>().Log().NextPolicy();

                options.For<CustomResponseException>().Response(e => 400).Headers((h, e) => h.Add("X-Custom", "val"))
                    .WithBody(async (r, w, e) =>
                    {
                        //await w.BaseStream.WriteAsync(Encoding.UTF8.GetBytes("customResponse"));
                        byte[] array = Encoding.UTF8.GetBytes("customResponse");
                        await w.WriteAsync(array, 0, array.Length);
                        //await w.FlushAsync();
                    })
                    .Handled();

                options.For<CustomObjectResponseException>().Response(e => 404)
                    .WithObjectResult(new {message="qwe"})
                    .Handled();

                options.For<CustomJsonResponseException>().Response(e => 403)
                    .WithBodyJson((r, e) => new { message = "qwe" })
                    .Handled();

                options.For<CommonResponseException>().Log().NextPolicy();

                options.For<BaseException>()
                    .Log(lo => { lo.Formatter = (o, e) => "qwe"; })                    
                    .Response(null, ResponseAlreadyStartedBehaviour.GoToNextHandler)
                        .ClearCacheHeaders()
                        .Headers((h, e) => h.Add("X-Common", "val"))
                        .WithBody(async(r, w, e) =>
                    {
                        byte[] array = Encoding.UTF8.GetBytes(e.GetType().Name + "_commonResponse");
                        await w.WriteAsync(array, 0, array.Length);
                    })
                    .Handled();
            });

            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseResponseBuffering();
            app.UseDeveloperExceptionPage();
            app.UseExceptionHandlingPolicies().Use(async (context, func) =>
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/rethrow")))
                {
                    throw new RethrowException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/nextempty")))
                {
                    throw new NextEmptyException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/nextpolicyempty")))
                {
                    throw new NotBaseException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/emptychain")))
                {
                    throw new EmptyChainException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/handled")))
                {
                    throw new HandledWithoutResponseException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/custom")))
                {
                    throw new CustomResponseException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/common")))
                {
                    throw new CommonResponseException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/object")))
                {
                    throw new CustomObjectResponseException();
                }

                if (context.Request.Path.StartsWithSegments(new PathString("/json")))
                {
                    throw new CustomJsonResponseException();
                }

                context.Response.StatusCode = 200;
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("ok"));
            });            
        }
    }
}