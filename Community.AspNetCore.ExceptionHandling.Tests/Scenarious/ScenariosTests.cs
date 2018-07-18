using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Tests.Scenarious;
using Microsoft.AspNetCore;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace Community.AspNetCore.ExceptionHandling.Tests
{
    public class ScenariosTests : WebApplicationFactory<Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder().UseStartup<Startup>();
        }

        private HttpClient Client =>
            this.CreateDefaultClient(new Uri("http://example.com"));

        [Fact]
        public async Task Ok()
        {
            HttpResponseMessage resp = await Client.GetAsync("ok");
            await AssertResponse(resp, HttpStatusCode.OK, "ok");
        }

        [Fact]
        public async Task HandledWithoutResponse()
        {
            HttpResponseMessage resp = await Client.GetAsync("handled");
            var str = await AssertResponse(resp, HttpStatusCode.OK, null);
            Assert.Equal(string.Empty, str);
        }

        [Fact]
        public async Task CustomResponse()
        {
            HttpResponseMessage resp = await Client.GetAsync("custom");
            await AssertResponse(resp, HttpStatusCode.BadRequest, "customResponse",
                new KeyValuePair<string, string>("X-Custom", "val"));
        }

        [Fact]
        public async Task CustomObjectResponse()
        {
            HttpResponseMessage resp = await Client.GetAsync("object");
            await AssertResponse(resp, HttpStatusCode.NotFound, "message");
        }

        [Fact]
        public async Task CustomJsonResponse()
        {
            HttpResponseMessage resp = await Client.GetAsync("json");
            await AssertResponse(resp, HttpStatusCode.Forbidden, "message");
        }

        [Fact]
        public async Task CommonResponse()
        {
            HttpResponseMessage resp = await Client.GetAsync("common");
            await AssertResponse(resp, HttpStatusCode.InternalServerError, "commonResponse",
                new KeyValuePair<string, string>("X-Common", "val"));
        }

        [Fact]
        public async Task ReThrow()
        {
            HttpResponseMessage resp = await Client.GetAsync("rethrow");
            await AssertUnhandledException<RethrowException>(resp);
        }

        [Fact]
        public async Task NextHandlerNotAwailable()
        {
            HttpResponseMessage resp = await Client.GetAsync("nextempty");
            await AssertUnhandledException<NextEmptyException>(resp);
        }

        [Fact]
        public async Task NextPolicyNotAwailable()
        {
            HttpResponseMessage resp = await Client.GetAsync("nextpolicyempty");
            await AssertUnhandledException<NotBaseException>(resp);
        }

        [Fact]
        public async Task EmptyChain()
        {
            HttpResponseMessage resp = await Client.GetAsync("emptychain");
            await AssertUnhandledException<EmptyChainException>(resp);
        }

        private static async Task AssertUnhandledException<TException>(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            string str = await response.Content.ReadAsStringAsync();
            Assert.Contains("An unhandled exception occurred while processing the request", str);
            Assert.Contains(typeof(TException).Name, str);
        }

        private static async Task<string> AssertResponse(HttpResponseMessage response, HttpStatusCode statusCode, string body, params KeyValuePair<string,string>[] headers)
        {
            Assert.Equal(statusCode, response.StatusCode);
            string str = await response.Content.ReadAsStringAsync();
            foreach (var header in headers)
            {
                Assert.True(response.Headers.Contains(header.Key), $"Header {header.Key} not awailable");
                Assert.Equal(header.Value, response.Headers.GetValues(header.Key).First());
            }

            if (body != null)
            {                
                Assert.Contains(body, str);
            }            

            return str;
        }
    }
}
