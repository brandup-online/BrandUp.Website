using BrandUp.Website.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    public class TagHelpersTest
    {
        static TagHelperContext Context()
            => new([], new Dictionary<object, object>(), "test");

        static TagHelperOutput Output(string tagName)
            => new(tagName, [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        [Fact]
        public void NavUrl_Anchor_SetsHrefAndApplinkClass()
        {
            var output = Output("a");
            new NavUrlTagHelper { NavUrl = "/catalog" }.Process(Context(), output);

            Assert.Equal("/catalog", output.Attributes["href"].Value.ToString());
            Assert.Contains("applink", output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void NavUrl_NonAnchor_SetsDataNavUrl()
        {
            var output = Output("div");
            new NavUrlTagHelper { NavUrl = "/catalog" }.Process(Context(), output);

            Assert.Equal("/catalog", output.Attributes["data-nav-url"].Value.ToString());
            Assert.False(output.Attributes.ContainsName("href"));
            Assert.Contains("applink", output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void NavUrl_AlwaysAddsApplinkClass()
        {
            var output = Output("a");
            new NavUrlTagHelper().Process(Context(), output);

            Assert.Contains("applink", output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void Form_AddsAppformClass()
        {
            var output = Output("form");
            new FormTagHelper().Process(Context(), output);

            Assert.Contains("appform", output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void NavReplace_SetsReplaceAttribute()
        {
            var output = Output("a");
            new NavReplaceTagHelper { NavReplace = true }.Process(Context(), output);

            Assert.True(output.Attributes.ContainsName("data-nav-replace"));
        }

        [Fact]
        public void NavReplace_False_DoesNotSetReplaceAttribute()
        {
            var output = Output("a");
            new NavReplaceTagHelper { NavReplace = false }.Process(Context(), output);

            Assert.False(output.Attributes.ContainsName("data-nav-replace"));
        }

        [Fact]
        public void NavReplace_SetsScopeAttribute()
        {
            var output = Output("a");
            new NavReplaceTagHelper { NavScope = "main" }.Process(Context(), output);

            Assert.Equal("main", output.Attributes["data-nav-scope"].Value.ToString());
        }
    }
}
