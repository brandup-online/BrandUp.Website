using BrandUp.Website.Pages;

namespace BrandUp.Website.IntegrationTests
{
    public class LifeTimeRequestTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public LifeTimeRequestTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");

            this.factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            this.factory.ClientOptions.AllowAutoRedirect = false;
        }

        [Fact]
        public async Task Request_Full()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("html", responseHtml);
        }

        [Fact]
        public async Task Request_Full_Redirect()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/delivery", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.Equal("/contacts", response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Request_Navigation_Get()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
            using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

        [Fact]
        public async Task Request_Content()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/contacts", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
            Assert.False(response.Headers.Contains(PageConstants.HttpHeaderPageReload));

            var responseHtml = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.DoesNotContain("</html>", responseHtml);
            Assert.Contains("page-content", responseHtml);
        }

        [Fact]
        public async Task Request_Content_Redirect()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/delivery", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/contacts", response.Headers.GetValues(PageConstants.HttpHeaderPageLocation).First());
        }

        static async Task<string> GetNavStateAsync(HttpClient client, string url)
        {
            using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);
            var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            var markerIndex = html.IndexOf("nav-data", StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, "nav-data script not found in full page response.");

            var jsonStart = html.IndexOf('{', markerIndex);
            var jsonEnd = html.IndexOf("</script>", jsonStart, StringComparison.Ordinal);
            var json = html[jsonStart..jsonEnd];

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("state").GetString()!;
        }

        [Fact]
        public async Task Request_Content_BadNavState()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
            using var response = await client.PostAsync("/", new StringContent("test"), TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

    }
}