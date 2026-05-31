using Microsoft.AspNetCore.Http;

namespace BrandUp.Website.Identity
{
    public class HttpAccessProviderTest
    {
        [Fact]
        public async Task IsAuthenticated_NoHttpContext_ReturnsFalse()
        {
            var provider = new HttpAccessProvider(new HttpContextAccessor { HttpContext = null });

            var result = await provider.IsAuthenticatedAsync(TestContext.Current.CancellationToken);

            Assert.False(result);
        }

        [Fact]
        public async Task GetUserId_NoHttpContext_ReturnsNull()
        {
            var provider = new HttpAccessProvider(new HttpContextAccessor { HttpContext = null });

            var result = await provider.GetUserIdAsync(TestContext.Current.CancellationToken);

            Assert.Null(result);
        }
    }
}
