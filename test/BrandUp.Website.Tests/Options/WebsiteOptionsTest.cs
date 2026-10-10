using Microsoft.Extensions.Configuration;

namespace BrandUp.Website
{
    public class WebsiteOptionsTest
    {
        [Fact]
        public void Validate_Valid_DoesNotThrow()
        {
            var options = new WebsiteOptions { Host = "localhost", CookiesPrefix = "ex" };

            options.Validate();
        }

        [Fact]
        public void Validate_MissingHost_Throws()
        {
            var options = new WebsiteOptions { Host = "", CookiesPrefix = "ex" };

            var ex = Assert.Throws<InvalidOperationException>(options.Validate);
            Assert.Contains(nameof(WebsiteOptions.Host), ex.Message);
        }

        [Fact]
        public void Validate_MissingCookiesPrefix_Throws()
        {
            var options = new WebsiteOptions { Host = "localhost", CookiesPrefix = null };

            var ex = Assert.Throws<InvalidOperationException>(options.Validate);
            Assert.Contains(nameof(WebsiteOptions.CookiesPrefix), ex.Message);
        }

        [Fact]
        public void Validate_MissingProtectionPurpose_Throws()
        {
            var options = new WebsiteOptions { Host = "localhost", CookiesPrefix = "ex", ProtectionPurpose = "" };

            var ex = Assert.Throws<InvalidOperationException>(options.Validate);
            Assert.Contains(nameof(WebsiteOptions.ProtectionPurpose), ex.Message);
        }

        [Fact]
        public void MapConfiguration_TrackingQueryParameters_AppendsToDefaults()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Website:Host"] = "localhost",
                    ["Website:CookiesPrefix"] = "ex",
                    ["Website:TrackingQueryParameters:0"] = "from"
                })
                .Build();
            var defaults = new WebsiteOptions().TrackingQueryParameters;
            var options = new WebsiteOptions();

            options.MapConfiguration(configuration);

            Assert.Equal([.. defaults, "from"], options.TrackingQueryParameters);
        }
    }
}
