using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    //public class SetHeadersOptions<TException> : IOptions<SetHeadersOptions<TException>>, IOptions<RawResponseHandlerOptions<TException>>
    //    where TException : Exception
    //{
    //    public SetHeadersOptions<TException> Value => this;

    //    public Action<IHeaderDictionary, TException> SetHeadersAction =
    //        (headers, exception) => { };

    //    RawResponseHandlerOptions<TException> IOptions<RawResponseHandlerOptions<TException>>.Value
    //    {
    //        get
    //        {
    //            return new RawResponseHandlerOptions<TException>
    //            {
    //                SetResponse =
    //                    async (context, exception) => { this.SetHeadersAction(context.Response.Headers, exception); }
    //            };
    //        }
    //    }
    //}
}