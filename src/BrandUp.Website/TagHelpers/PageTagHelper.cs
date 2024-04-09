using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("page", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override int Order => 1000;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is AppPageModel appPageModel && ViewContext.View is RazorView razorView)
                await appPageModel.RenderPageAsync(razorView.RazorPage);

            output.SuppressOutput();
        }
    }
}