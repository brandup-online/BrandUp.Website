using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("form")]
    public class FormTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            output.Attributes.AddCssClass("appform");
        }
    }
}