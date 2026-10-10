using System.Net;
using System.Text.RegularExpressions;
using BrandUp.Website.Pages;

namespace BrandUp.Website.IntegrationTests
{
    public class CanonicalLinkTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public CanonicalLinkTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Theory]
        [InlineData("/contacts?_gl=1*r0v4yt*_ga*NzMz", "https://localhost/contacts")]
        [InlineData("/contacts?utm_source=yandex&page=2&yclid=1", "https://localhost/contacts?page=2")]
        [InlineData("/contacts?page=2", "https://localhost/contacts?page=2")]
        public async Task CanonicalAndOpenGraph_WithoutTrackingParameters(string path, string expected)
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);
            var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new Uri(expected), Attribute(html, "page-link-canonical", "href"));
            Assert.Equal(new Uri(expected), Attribute(html, "og-url", "content"));
        }

        [Fact]
        public async Task Navigation_WithoutNoCacheAndTrackingParameters()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add(PageConstants.HttpHeaderPageNav, await NavData.GetStateAsync(client, "/"));
            using var response = await client.GetAsync("/contacts?_=1728000000000&utm_source=yandex&page=2", TestContext.Current.CancellationToken);
            var navData = NavData.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new Uri("https://localhost/contacts?page=2"), new Uri(navData.GetProperty("canonicalLink").GetString()!));
            Assert.Equal(new Uri("https://localhost/contacts?page=2"), new Uri(navData.GetProperty("openGraph").GetProperty("url").GetString()!));
        }

        /// <summary>
        /// Адрес из атрибута тега с указанным id. Разметка примера минифицирована: кавычки у атрибутов
        /// необязательны, а у части ссылок вырезана схема (<c>//localhost/…</c>).
        /// </summary>
        static Uri Attribute(string html, string id, string name)
        {
            var tag = Regex.Match(html, $@"<[a-z]+ [^>]*\bid=""?{Regex.Escape(id)}\b[^>]*>").Value;
            Assert.NotEmpty(tag);

            var value = Regex.Match(tag, $@"\b{name}=(""(?<v>[^""]*)""|(?<v>[^\s>]+))").Groups["v"].Value;

            return new Uri(new Uri("https://localhost/"), WebUtility.HtmlDecode(value));
        }
    }
}
