using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Xunit;

namespace Community.AspNetCore.ExceptionHandling.Tests.Handlers
{
    public class NextChainHandlerTests
    {
        [Fact]
        public async Task AlwaysNextChainResult()
        {
            NextChainHandler handler = new NextChainHandler();
            HandlerResult result = await handler.Handle(null, null);

            Assert.Equal(HandlerResult.NextChain, result);
        }
    }
}