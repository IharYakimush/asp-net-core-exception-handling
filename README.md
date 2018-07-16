# AspNetCore-ExceptionHandling
Middleware to configure exception handling policies. Configure chain of handlers per exception type. OOTB handlers: log, retry, set responce headers and body

### Sample
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    services.AddExceptionHandlingPolicies(options =>
    {
        options.For<InitializationException>().Rethrow();
                
        options.For<SomeTransientException>().Retry().NextChain();
                
        options.For<SomeBadRequestException>()
            .Response(e => 400)
                .Headers((h, e) => h["X-MyCustomHeader"] = e.Message)
                .WithBody((req,sw, exception) => sw.WriteAsync(exception.ToString()))
            .NextChain();

        // Ensure that all exception types are handled by adding handler for generic exception at the end.
        options.For<Exception>()
            .Log(lo =>
                {
                    lo.EventIdFactory = (c, e) => new EventId(123, "UnhandlerException");
                    lo.Category = (context, exception) => "MyCategory";
                })
            .Response(e => 500, ResponseAlreadyStartedBehaviour.GoToNextHandler)
                .ClearCacheHeaders()
                .WithObjectResult((r, e) => new { msg = e.Message, path = r.Path })
            .Handled();
    });
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseExceptionHandlingPolicies();
    app.UseMvc();
}
```
