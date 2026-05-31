namespace BrandUp.Website.Pages
{
    public class PageOpenGraphTest
    {
        [Fact]
        public void Items_IncludeKnownAndCustomProperties()
        {
            var og = new PageOpenGraph("website", new Uri("https://example.com/og.jpg"), "Title", new Uri("https://example.com/"))
            {
                SiteName = "Example"
            };
            og.Set("custom_prop", "value");

            var items = og.Items.ToDictionary(it => it.Key, it => it.Value);

            Assert.Equal("website", items["type"]);
            Assert.Equal("Title", items["title"]);
            Assert.Equal("Example", items["site_name"]);
            Assert.Equal("value", items["custom_prop"]);
        }

        [Fact]
        public void Set_CustomName_IsNormalized()
        {
            var og = new PageOpenGraph("website", new Uri("https://example.com/og.jpg"), "Title", new Uri("https://example.com/"));
            og.Set("  Custom_Prop  ", "value");

            Assert.True(og.Contains("custom_prop"));
            Assert.Equal("value", og.Get<string>("custom_prop"));
        }
    }
}
