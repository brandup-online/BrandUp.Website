using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website.IntegrationTests
{
    public class RedirectSchemeTest : IClassFixture<NoHttpsRedirectWebApplicationFactory>
    {
        private readonly NoHttpsRedirectWebApplicationFactory factory;

        public RedirectSchemeTest(NoHttpsRedirectWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        [Theory]
        // RedirectToHttps=false: www/alias canonicalization must preserve the request scheme, not force https
        [InlineData("http://www.localhost/", "http://localhost/")]
        [InlineData("https://www.localhost/", "https://localhost/")]
        [InlineData("http://www.msk.localhost/", "http://msk.localhost/")]
        [InlineData("http://alias.ru/", "http://localhost/")]
        [InlineData("https://alias.ru/", "https://localhost/")]
        public async Task Canonical_redirect_preserves_scheme(string baseAddress, string redirectUrl)
        {
            factory.ClientOptions.BaseAddress = new Uri(baseAddress);
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }
    }

    public class NoHttpsRedirectWebApplicationFactory : WebApplicationFactory<ExampleWebSite.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("Development")
                .ConfigureTestServices(services =>
                {
                    services.AddWebsite().AddSingleWebsite("single");

                    services.Configure<WebsiteOptions>((options) =>
                    {
                        options.Host = "localhost";
                        options.Aliases = ["alias.ru"];
                        options.RedirectToHttps = false;
                    });
                });
        }
    }
}
