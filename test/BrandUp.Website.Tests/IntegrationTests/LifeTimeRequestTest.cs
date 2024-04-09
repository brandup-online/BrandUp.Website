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
            using var response = await client.GetAsync("/");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("</html>", responseHtml);
        }

        [Fact]
        public async Task Request_Full_Redirect()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/delivery");

            Assert.Equal(System.Net.HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.Equal("/contacts", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Request_Navigation_Get()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
            using var response = await client.GetAsync("/");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        //[Fact]
        //public async Task Request_Content()
        //{
        //    using var client = factory.CreateClient();
        //    client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
        //    using var response = await client.PostAsync("/", new StringContent(string.Empty));

        //    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        //    Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);

        //    var responseHtml = await response.Content.ReadAsStringAsync();
        //    Assert.DoesNotContain("</html>", responseHtml);
        //}

        //[Fact]
        //public async Task Request_Content_Redirect()
        //{
        //    using var client = factory.CreateClient();
        //    client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
        //    using var response = await client.PostAsync("/delivery", new StringContent(string.Empty));

        //    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        //    Assert.Equal("/contacts", response.Headers.GetValues(PageConstants.HttpHeaderPageLocation).First());

        //    var responseHtml = await response.Content.ReadAsStringAsync();
        //    Assert.Empty(responseHtml);
        //}

        [Fact]
        public async Task Request_Content_BadNavState()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, "true");
            using var response = await client.PostAsync("/", new StringContent("test"));

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

    }
}