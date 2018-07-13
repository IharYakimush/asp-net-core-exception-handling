using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    //public class NewResponseOptions<TException> : HandlerOptions, IOptions<NewResponseOptions<TException>>, IOptions<RawResponseHandlerOptions<TException>>
    //where TException : Exception
    //{
    //    public NewResponseOptions<TException> Value => this;

    //    public Func<TException, int> StatusCodeFactory =
    //        exception => StatusCodes.Status500InternalServerError;

    //    RawResponseHandlerOptions<TException> IOptions<RawResponseHandlerOptions<TException>>.Value =>                        
    //        new RawResponseHandlerOptions<TException>
    //        {
    //            SetResponse =
    //                async (context, exception) =>
    //                {
    //                    context.Response.Headers.Clear();
    //                    if (context.Response.Body.CanSeek)
    //                        context.Response.Body.SetLength(0L);
    //                    context.Response.StatusCode = this.StatusCodeFactory(exception);
    //                }
    //        };
    //}
}