using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "nav-ignore")]
    public class NavIgnoreTagHelper : TagHelper
    {
        const string AttributeName = "data-nav-ignore";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.ContainsName(AttributeName))
                output.Attributes.SetAttribute(AttributeName, string.Empty);
        }
    }
}