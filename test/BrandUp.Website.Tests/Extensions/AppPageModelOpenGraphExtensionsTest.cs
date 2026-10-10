using BrandUp.Website.Pages;

namespace BrandUp.Website.Extensions
{
    public class AppPageModelOpenGraphExtensionsTest
    {
        [Fact]
        public void SetOpenGraphWebsite_Url_IsCanonicalLink()
        {
            var page = new CanonicalPage();

            var og = page.SetOpenGraphWebsite(new Uri("https://example.com/og.jpg"));

            Assert.Equal(page.CanonicalLink, og.Url);
            Assert.Same(og, page.OpenGraph);
        }

        [Fact]
        public void SetOpenGraphArticle_Url_IsCanonicalLink()
        {
            var page = new CanonicalPage();

            var og = page.SetOpenGraphArticle(new Uri("https://example.com/og.jpg"));

            Assert.Equal(page.CanonicalLink, og.Url);
            Assert.Same(og, page.OpenGraph);
        }

        class CanonicalPage : AppPageModel
        {
            public override string Title => "Title";
            public override Uri CanonicalLink { get; } = new("https://example.com/canonical");
        }
    }
}
