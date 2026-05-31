using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website.IntegrationTests
{
    public class NoVisitorStoreTest : IClassFixture<NoVisitorStoreWebApplicationFactory>
    {
        private readonly NoVisitorStoreWebApplicationFactory factory;

        public NoVisitorStoreTest(NoVisitorStoreWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Theory]
        [InlineData("http://localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://www.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://www.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        // www-strip removes only the "www." prefix; alias canonicalization is a separate hop
        [InlineData("https://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://alias.ru/")]
        [InlineData("http://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://alias.ru/")]
        // www-strip preserves nested subdomains instead of collapsing to the base host
        [InlineData("https://www.msk.localhost/", "/", HttpStatusCode.MovedPermanently, "https://msk.localhost/")]
        public async Task Redirect_root(string baseAddress, string path, HttpStatusCode statusCode, string redirectUrl)
        {
            factory.ClientOptions.BaseAddress = new Uri(baseAddress);
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(statusCode, response.StatusCode);
            if (redirectUrl != null)
                Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }
    }

    public class NoVisitorStoreWebApplicationFactory : WebApplicationFactory<ExampleWebSite.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("Development")
                .ConfigureTestServices(services =>
                {
                    services.Configure<WebsiteOptions>((options) =>
                    {
                        options.Host = "localhost";
                        options.Aliases = ["alias.ru"];
                        options.RedirectToHttps = true;
                    });
                });
        }
    }
}