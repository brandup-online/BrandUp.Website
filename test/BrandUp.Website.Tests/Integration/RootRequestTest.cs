using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Website.Tests.Integration
{
    public class RootRequestTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public RootRequestTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Fact]
        public async Task Default()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("http://localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://www.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://www.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://test.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://test.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://www.test.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://www.test.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://msk.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://msk.localhost/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        public async Task Redirect_root(string baseAddress, string path, HttpStatusCode statusCode, string redirectUrl)
        {
            factory.ClientOptions.BaseAddress = new Uri(baseAddress);
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync(path);

            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<ExampleWebSite.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.Configure<WebsiteOptions>((options) =>
                {
                    options.Aliases = new string[] { "test.ru" };
                });
            });
        }
    }
}