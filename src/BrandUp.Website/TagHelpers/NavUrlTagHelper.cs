using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-page")]
    [HtmlTargetElement(Attributes = "nav-url")]
    public class NavUrlTagHelper : TagHelper
    {
        const string LinkClass = "applink";

        [HtmlAttributeName("nav-url")]
        public string NavUrl { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (NavUrl != null)
            {
                if (string.Equals(output.TagName, "a", System.StringComparison.OrdinalIgnoreCase))
                    output.Attributes.SetAttribute("href", NavUrl);
                else
                    output.Attributes.SetAttribute("data-nav-url", NavUrl);
            }

            string cssClass;
            var hasLinkClass = false;
            if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute attribute))
            {
                cssClass = attribute.Value.ToString();
                hasLinkClass = cssClass.Contains("applink");
            }
            else
                cssClass = string.Empty;

            if (!hasLinkClass)
            {
                if (!string.IsNullOrEmpty(cssClass))
                    cssClass += " " + LinkClass;
                else
                    cssClass = LinkClass;
            }

            output.Attributes.SetAttribute("class", cssClass);
        }
    }
}