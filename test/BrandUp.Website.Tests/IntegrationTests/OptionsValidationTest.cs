using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.IntegrationTests
{
    public class OptionsValidationTest
    {
        [Fact]
        public void InvalidOptions_FailFastOnStart()
        {
            using var factory = new WebApplicationFactory<ExampleWebSite.Program>()
                .WithWebHostBuilder(builder => builder
                    .UseEnvironment("Development")
                    .ConfigureTestServices(services =>
                        services.Configure<WebsiteOptions>(options => options.CookiesPrefix = string.Empty)));

            var ex = Record.Exception(() => factory.CreateClient());

            Assert.NotNull(ex);
            var validationException = ex as OptionsValidationException ?? ex.InnerException as OptionsValidationException;
            Assert.NotNull(validationException);
            Assert.Contains(nameof(WebsiteOptions.CookiesPrefix), validationException.Message);
        }
    }
}
