using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling.Response
{
    public class SetStatusCodeOptions<TException> : IOptions<SetStatusCodeOptions<TException>>, IOptions<RawResponseHandlerOptions<TException>>
    where TException : Exception
    {
        public SetStatusCodeOptions<TException> Value => this;

        public Func<TException, int> StatusCodeFactory =
            exception => StatusCodes.Status500InternalServerError;

        RawResponseHandlerOptions<TException> IOptions<RawResponseHandlerOptions<TException>>.Value =>                        
            new RawResponseHandlerOptions<TException>
            {
                SetResponse =
                    async (context, exception) =>
                    {
                        context.Response.StatusCode = this.StatusCodeFactory(exception);
                    }
            };
    }
}