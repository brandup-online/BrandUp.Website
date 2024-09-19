using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "nav-replace")]
    public class NavReplaceTagHelper : TagHelper
    {
        const string ReplaceAttributeName = "data-nav-replace";
        const string ScopeAttributeName = "data-nav-scope";

        [HtmlAttributeName("nav-replace")]
        public bool NavReplace { get; set; }

        [HtmlAttributeName("nav-scope")]
        public string NavScope { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (NavReplace)
                output.Attributes.SetAttribute(new TagHelperAttribute(ReplaceAttributeName, null, HtmlAttributeValueStyle.Minimized));

            if (NavScope != null)
                output.Attributes.SetAttribute(new TagHelperAttribute(ScopeAttributeName, NavScope));
        }
    }
}