using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Xunit;

namespace Community.AspNetCore.ExceptionHandling.Tests.Handlers
{
    public class MarkHandledHandlerTests
    {
        [Fact]
        public async Task AlwaysHandledResult()
        {
            MarkHandledHandler handler = new MarkHandledHandler();
            HandlerResult result = await handler.Handle(null, null);

            Assert.Equal(HandlerResult.Handled, result);
        }
    }
}