using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement(Attributes = "page-content")]
    public class PageContentTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            output.Attributes.SetAttribute(new TagHelperAttribute("id", "page-content"));

            output.Attributes.RemoveAll("page-content");
        }
    }
}