namespace BrandUp.Website.IntegrationTests
{
    public class HealthCheckTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public HealthCheckTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");
        }

        [Fact]
        public async Task CustomHost()
        {
            factory.ClientOptions.BaseAddress = new Uri("https://yandex.ru/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/healthz");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", responseHtml);
        }

        [Fact]
        public async Task Http()
        {
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/");
            factory.ClientOptions.AllowAutoRedirect = false;

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/healthz");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", responseHtml);
        }
    }
}