using System.Text.Json;

namespace BrandUp.Website.IntegrationTests
{
    /// <summary>
    /// Reads the navigation model that a page renders into the <c>nav-data</c> script.
    /// </summary>
    static class NavData
    {
        public static async Task<string> GetStateAsync(HttpClient client, string url)
        {
            using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);
            var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            return GetState(html);
        }

        public static string GetState(string html) => Parse(html).GetProperty("state").GetString()!;

        public static JsonElement Parse(string html)
        {
            var markerIndex = html.IndexOf("nav-data", StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, "nav-data script not found in response.");

            var jsonStart = html.IndexOf('{', markerIndex);
            var jsonEnd = html.IndexOf("</script>", jsonStart, StringComparison.Ordinal);

            using var doc = JsonDocument.Parse(html[jsonStart..jsonEnd]);
            return doc.RootElement.Clone();
        }
    }
}
