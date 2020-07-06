using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("form")]
    public class FormTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.AddCssClass("appform");
        }
    }
}