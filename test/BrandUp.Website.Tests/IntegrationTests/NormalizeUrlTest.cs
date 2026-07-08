using System.Net;

namespace BrandUp.Website.IntegrationTests
{
    public class NormalizeUrlTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory factory;

        public NormalizeUrlTest(CustomWebApplicationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory.Server.BaseAddress = new Uri("https://localhost/");

            this.factory.ClientOptions.BaseAddress = new Uri("https://localhost/");
            this.factory.ClientOptions.AllowAutoRedirect = false;
        }

        [Theory]
        // завершающий "/" убирается
        [InlineData("/contacts/", "https://localhost/contacts")]
        // верхний регистр приводится к нижнему
        [InlineData("/Contacts", "https://localhost/contacts")]
        [InlineData("/CONTACTS", "https://localhost/contacts")]
        // одновременно и слэш, и регистр
        [InlineData("/Contacts/", "https://localhost/contacts")]
        // query-строка сохраняется как есть
        [InlineData("/Contacts/?page=10", "https://localhost/contacts?page=10")]
        public async Task Normalize_Redirects(string path, string redirectUrl)
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal(new Uri(redirectUrl), response.Headers.Location);
        }

        [Theory]
        // уже нормализованный путь — редиректа нет
        [InlineData("/contacts")]
        // корень не трогаем (длина пути == 1)
        [InlineData("/")]
        public async Task Normalize_NoRedirect(string path)
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Excluded_Path_IsNotNormalized()
        {
            using var client = factory.CreateClient();
            // /dist/ исключён: путь с верхним регистром НЕ должен получить 301 на нижний регистр
            // (иначе манифест static web assets не найдёт бандл webpack).
            using var response = await client.GetAsync("/dist/App.js", TestContext.Current.CancellationToken);

            Assert.NotEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }

        [Fact]
        public async Task Excluded_Path_ServesFile()
        {
            using var client = factory.CreateClient();
            // Точный регистр бандла отдаётся статикой без редиректов.
            using var response = await client.GetAsync("/dist/app.js", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
