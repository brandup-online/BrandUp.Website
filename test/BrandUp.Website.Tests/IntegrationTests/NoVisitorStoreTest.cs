using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

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
        [InlineData("https://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        public async Task Redirect_root(string baseAddress, string path, HttpStatusCode statusCode, string redirectUrl)
        {
            factory.ClientOptions.BaseAddress = new Uri(baseAddress);
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync(path);

            Assert.Equal(statusCode, response.StatusCode);
            if (redirectUrl != null)
                Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }
    }

    public class NoVisitorStoreWebApplicationFactory : WebApplicationFactory<ExampleWebSite.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("Test")
                .ConfigureTestServices(services =>
                {
                    services.RemoveAll<IVisitorStore>();

                    services.Configure<WebsiteOptions>((options) =>
                    {
                        options.Aliases = new string[] { "alias.ru" };
                    });
                });
        }
    }
}