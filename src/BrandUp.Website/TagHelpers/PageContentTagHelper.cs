using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "page-content")]
    public class PageContentTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            output.Attributes.SetAttribute(new TagHelperAttribute("id", "page-content"));

            output.Attributes.RemoveAll("page-content");
        }
    }
}