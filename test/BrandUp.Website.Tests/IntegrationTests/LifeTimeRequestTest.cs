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
        public async Task Request_Full_TwitterCard()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            // Домашняя страница задаёт OpenGraph — значит рядом должен появиться Twitter Card.
            // HTML минифицируется (кавычки атрибутов могут срезаться), поэтому проверяем токены.
            Assert.Contains("og:title", responseHtml);
            Assert.Contains("twitter:card", responseHtml);
            Assert.Contains("summary_large_image", responseHtml);
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
        public async Task Request_Content_SameLayout()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/examples", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(PageConstants.HttpHeaderPageReload));
        }

        [Fact]
        public async Task Request_Content_ChangedLayout()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/examples/altlayout", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

        [Fact]
        public async Task Request_Content_ChangedLayout_Back()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/examples/altlayout");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/contacts", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

        [Fact]
        public async Task Request_Content_ChangedArea()
        {
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var response = await client.GetAsync("/admin", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", response.Headers.GetValues(PageConstants.HttpHeaderPageReload).First());
        }

        [Fact]
        public async Task Request_Content_ChainedNavigation()
        {
            // Ответ навигации несёт состояние уже новой страницы, поэтому следующий переход
            // с тем же layout не должен приводить к перезагрузке.
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            using var firstResponse = await client.GetAsync("/examples", TestContext.Current.CancellationToken);

            Assert.False(firstResponse.Headers.Contains(PageConstants.HttpHeaderPageReload));

            var firstHtml = await firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            client.DefaultRequestHeaders.Remove(PageConstants.HttpHeaderPageNav);
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, ExtractNavState(firstHtml));
            using var secondResponse = await client.GetAsync("/contacts", TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.False(secondResponse.Headers.Contains(PageConstants.HttpHeaderPageReload));
        }

        [Fact]
        public async Task Request_Submit()
        {
            // Отправка формы идёт в том же режиме, что и навигация, но layout страницы не меняется.
            using var client = factory.CreateClient();
            var navState = await GetNavStateAsync(client, "/examples/pagesubmit");

            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, navState);
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageSubmit, "true");

            var form = new FormUrlEncodedContent([new KeyValuePair<string, string>("Key", "test")]);
            using var response = await client.PostAsync("/examples/pagesubmit?handler=Save", form, TestContext.Current.CancellationToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(PageConstants.HttpHeaderPageReload));
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

            return ExtractNavState(html);
        }

        static string ExtractNavState(string html)
        {
            var markerIndex = html.IndexOf("nav-data", StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, "nav-data script not found in response.");

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