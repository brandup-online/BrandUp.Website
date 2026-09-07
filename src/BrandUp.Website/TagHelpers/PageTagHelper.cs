using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    [HtmlTargetElement("page", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageTagHelper(IJsonHelper jsonHelper, IRazorViewEngine viewEngine) : TagHelper
    {
        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override int Order => int.MaxValue;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not AppPageModel appPageModel || ViewContext.View is not RazorView razorView)
                throw new InvalidOperationException($"Tag helper ${typeof(PageTagHelper).FullName} require page model {typeof(AppPageModel).FullName}.");

            appPageModel.ApplyLayout(ResolveLayoutPath(razorView.RazorPage));

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

        /// <summary>
        /// Resolves the layout name of the page to the path of the layout file, the same way
        /// <see cref="RazorView"/> does it. Different pages may refer to different layout files
        /// by the same relative name, so only the resolved path identifies a layout.
        /// </summary>
        string ResolveLayoutPath(IRazorPage razorPage)
        {
            var layoutName = razorPage.Layout;
            if (string.IsNullOrEmpty(layoutName))
                return string.Empty;

            var layoutPage = viewEngine.GetPage(razorPage.Path, layoutName).Page ?? viewEngine.FindPage(ViewContext, layoutName).Page;

            return layoutPage?.Path ?? layoutName;
        }
    }
}