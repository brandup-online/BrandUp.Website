using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "nav-replace")]
    public class NavReplaceTagHelper : TagHelper
    {
        const string AttributeName = "data-nav-replace";

        [HtmlAttributeName("nav-replace")]
        public bool NavReplace { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (NavReplace)
                output.Attributes.SetAttribute(new TagHelperAttribute(AttributeName, null, HtmlAttributeValueStyle.Minimized));
        }
    }
}