# AspNetCore-ExceptionHandling
Middleware to configure exception handling policies. Configure chain of handlers per exception type. OOTB handlers: log, retry, set responce headers and body

### Code Sample
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    services.AddExceptionHandlingPolicies(options =>
    {
        options.For<InitializationException>().Rethrow();
                
        options.For<SomeTransientException>().Retry(ro => ro.MaxRetryCount = 2).NextChain();
                
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

### Policy exception handler transitions
When exception catched in middleware it try to apply handlers from first registered policy siutable for given exception. Policy contains a chain of handlers. Each handler perform some action and apply transition. To prevent re throw of exception handlers chain MUST ends with "Handled" transition.
Following handlers currently supported:

| Handler  | Action | Transition |
| ---------| ------------- | ------------- |
| Rethrow  | Apply ReThrow transition  | ReThrow |
| NextPolicy  | Try to execute next policy siutable for given exception  | NextPolicy |
| Handled  | Mark exception as handled to prevent it from bein re thrown  | Handled |
| Retry  | Execute aspnetcore pipeline again if retry count not exceed limit  | Retry (if retry limit not exceeded) or NextHandler |
| Log  | Log exception  | NextHandler |
| DisableFurtherLog  | Prevent exception from being logged again in current middleware (for current request only)  | NextHandler |
| Response  | Modify response (set status code, headers and body) depending on further response builder configuration | NextHandler |

Sample of transitions:
![alt text](/Transitions.png)

### Nuget
| Package  | Target | Comments |
| ---------| ------------- | ------------- |
| https://www.nuget.org/packages/Commmunity.AspNetCore.ExceptionHandling | netstandard2.0;netcoreapp2.1 | Main functionality |
| https://www.nuget.org/packages/Commmunity.AspNetCore.ExceptionHandling.Mvc | netcoreapp2.1 | Alllow to use MVC IActionResult (including ObjectResult) in 'Response' handler |
| https://www.nuget.org/packages/Commmunity.AspNetCore.ExceptionHandling.NewtonsoftJson | netstandard2.0; | Allow to set Json serialized object as a response body in 'Response' handler. Use it only if 'Commmunity.AspNetCore.ExceptionHandling.Mvc' usage not possible |
