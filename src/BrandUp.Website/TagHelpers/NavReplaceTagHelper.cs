using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "nav-replace")]
    public class NavReplaceTagHelper : TagHelper
    {
        const string ReplaceAttributeName = "data-nav-replace";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.ContainsName(ReplaceAttributeName))
                output.Attributes.SetAttribute(new TagHelperAttribute(ReplaceAttributeName, null, HtmlAttributeValueStyle.Minimized));

            if (output.Attributes.TryGetAttribute("nav-replace", out TagHelperAttribute a))
                output.Attributes.Remove(a);
        }
    }
}