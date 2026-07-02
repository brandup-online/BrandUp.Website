using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website.IntegrationTests
{
    public class ReferrerPolicyHeaderTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public ReferrerPolicyHeaderTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Fact]
        public async Task Default_SendsHeader()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        }

        [Fact]
        public async Task Redirect_SendsHeader()
        {
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        }
    }

    public class ReferrerPolicyDisabledTest : IClassFixture<ReferrerPolicyDisabledTest.NoReferrerPolicyWebApplicationFactory>
    {
        private readonly NoReferrerPolicyWebApplicationFactory factory;

        public ReferrerPolicyDisabledTest(NoReferrerPolicyWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Fact]
        public async Task None_NoHeader()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains("Referrer-Policy"));
        }

        public class NoReferrerPolicyWebApplicationFactory : WebApplicationFactory<ExampleWebSite.Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder
                    .ConfigureTestServices(services =>
                    {
                        services.Configure<WebsiteOptions>((options) =>
                        {
                            options.Host = "localhost";
                            options.ReferrerPolicy = ReferrerPolicy.None;
                        });

                        services.AddSingleton<IAntiforgery, Fakes.FakeAntiforgery>();
                    });

                builder.UseEnvironment("Development");
            }
        }
    }
}
