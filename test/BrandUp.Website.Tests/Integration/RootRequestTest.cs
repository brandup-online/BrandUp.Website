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
        [InlineData("https://alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("https://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
        [InlineData("http://www.alias.ru/", "/", HttpStatusCode.MovedPermanently, "https://localhost/")]
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

        [Theory]
        [InlineData("http://localhost/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("http://www.localhost/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("https://www.localhost/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("https://alias.ru/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("http://alias.ru/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("https://www.alias.ru/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("http://www.alias.ru/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("http://msk.localhost/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        [InlineData("https://msk.localhost/", "/catalog/elki/", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/")]
        public async Task Redirect_path(string baseAddress, string path, HttpStatusCode statusCode, string redirectUrl)
        {
            factory.ClientOptions.BaseAddress = new Uri(baseAddress);
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync(path);

            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }

        [Theory]
        [InlineData("http://localhost/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("http://www.localhost/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("https://www.localhost/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("https://alias.ru/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("http://alias.ru/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("https://www.alias.ru/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("http://www.alias.ru/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("http://msk.localhost/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        [InlineData("https://msk.localhost/", "/catalog/elki/?page=10", HttpStatusCode.MovedPermanently, "https://localhost/catalog/elki/?page=10")]
        public async Task Redirect_path_and_query(string baseAddress, string path, HttpStatusCode statusCode, string redirectUrl)
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
            builder
                .UseEnvironment("Test")
                .ConfigureTestServices(services =>
                {
                    services.Configure<WebsiteOptions>((options) =>
                    {
                        options.Aliases = new string[] { "alias.ru" };
                    });
                });
        }
    }
}