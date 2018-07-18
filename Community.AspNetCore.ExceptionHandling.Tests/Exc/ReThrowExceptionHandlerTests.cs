using System.Threading.Tasks;
using Community.AspNetCore.ExceptionHandling.Exc;
using Community.AspNetCore.ExceptionHandling.Handlers;
using Xunit;

namespace Community.AspNetCore.ExceptionHandling.Tests.Exc
{

    public class ReThrowExceptionHandlerTests
    {
        [Fact]
        public async Task AlwaysReThrowResult()
        {
            ReThrowExceptionHandler handler = new ReThrowExceptionHandler();
            HandlerResult result = await handler.Handle(null, null);

            Assert.Equal(HandlerResult.ReThrow, result);
        }
    }
}