using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "nav-ignore")]
    public class NavIgnoreTagHelper : TagHelper
    {
        const string AttributeName = "data-nav-ignore";

        [HtmlAttributeName("nav-ignore")]
        public bool NavIgnore { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (NavIgnore)
                output.Attributes.SetAttribute(new TagHelperAttribute(AttributeName, null, HtmlAttributeValueStyle.Minimized));
        }
    }
}