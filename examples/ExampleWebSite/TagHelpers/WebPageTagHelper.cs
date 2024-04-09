using ExampleWebSite.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExampleWebSite.TagHelpers
{
    [HtmlTargetElement("page")]
    public class WebPageTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("title")]
        public string Title { get; set; }
        [HtmlAttributeName("header")]
        public string Header { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not WebPageModel webPageModel)
                return;

            if (!string.IsNullOrEmpty(Title))
                webPageModel.SetTitle(Title);

            output.SuppressOutput();
        }
    }
}