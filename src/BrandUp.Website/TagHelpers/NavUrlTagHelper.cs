using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-page")]
    [HtmlTargetElement(Attributes = "nav-url")]
    [HtmlTargetElement(Attributes = "data-nav-url")]
    public class NavUrlTagHelper : TagHelper
    {
        const string LinkClass = "applink";

        [HtmlAttributeName("nav-url")]
        public string NavUrl { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (NavUrl != null)
            {
                if (string.Equals(output.TagName, "a", StringComparison.InvariantCultureIgnoreCase))
                    output.Attributes.SetAttribute("href", NavUrl);
                else
                    output.Attributes.SetAttribute("data-nav-url", NavUrl);
            }

            output.Attributes.AddCssClass(LinkClass);
        }
    }
}