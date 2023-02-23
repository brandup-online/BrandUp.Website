namespace BrandUp.Website.IntegrationTests
{
    public class LifeTimeRequestTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public LifeTimeRequestTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Fact]
        public async Task Request_Full()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("</html>", responseHtml);
        }

        [Fact]
        public async Task Request_Full_Redirect()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/delivery");

            Assert.Equal(System.Net.HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.Equal("/contacts", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Request_Navigation_Get()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/?_nav");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        //[Fact]
        //public async Task Request_Navigation_Post()
        //{
        //    factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
        //    factory.ClientOptions.AllowAutoRedirect = false;

        //    using var client = factory.CreateClient();
        //    var response = await client.PostAsync("/contacts?_nav", new StringContent("test", System.Text.Encoding.UTF8, "text/plain"));

        //    //Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        //    var json = await response.Content.ReadAsStringAsync();

        //    var navModel = JsonConvert.DeserializeObject<Pages.Models.NavigationClientModel>(json, new JsonSerializerSettings { });

        //    Assert.False(navModel.IsAuthenticated);
        //    Assert.NotNull(navModel.ValidationToken);
        //    Assert.NotNull(navModel.State);
        //    Assert.NotNull(navModel.Page);
        //    Assert.Equal("https://localhost/contacts", navModel.Url);
        //    Assert.Equal("/contacts", navModel.Path);
        //    Assert.NotNull(navModel.Page.Title);
        //}

        [Fact]
        public async Task Request_Content()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("?_content&_=235235235");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("</html>", responseHtml);
        }

        [Fact]
        public async Task Request_Content_Redirect()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/delivery?_content&_=235235235");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/contacts", response.Headers.GetValues("Page-Location").First());

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseHtml);
        }
    }
}