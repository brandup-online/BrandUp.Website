using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("page", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageTagHelper(IJsonHelper jsonHelper) : TagHelper
    {
        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override int Order => int.MaxValue;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not AppPageModel appPageModel || ViewContext.View is not RazorView razorView)
                throw new InvalidOperationException($"Tag helper ${typeof(PageTagHelper).FullName} require page model {typeof(AppPageModel).FullName}.");

            if (appPageModel.RequestMode == AppPageRequestMode.Content)
                razorView.RazorPage.Layout = null;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id", "page-content");

            var pageRenderContext = new PageRenderContext(appPageModel, razorView.RazorPage, output);
            await appPageModel.RaiseRenderPageAsync(pageRenderContext);

            #region Navigation JSON

            var navClientModel = await appPageModel.GetNavigationClientModelAsync();
            var navJson = jsonHelper.Serialize(navClientModel);

            var pageModelScriptTag = new TagBuilder("script");
            pageModelScriptTag.Attributes.Add("id", "nav-data");
            pageModelScriptTag.Attributes.Add("type", "application/json");
            pageModelScriptTag.InnerHtml.SetHtmlContent(navJson);
            output.PreContent.AppendHtml(pageModelScriptTag);

            #endregion
        }
    }
}