using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-page")]
    [HtmlTargetElement(Attributes = "nav-link")]
    public class NavLinkTagHelper : TagHelper
    {
        const string LinkClass = "applink";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
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