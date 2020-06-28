using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Website.TagHelpers
{
    public class EmbeddingTagHelperComponent : TagHelperComponent
    {
        const string LoadingClass = "bp-state-loading";

        private readonly IJsonHelper jsonHelper;
        private readonly IWebsiteEvents websiteEvents;

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public EmbeddingTagHelperComponent(IJsonHelper jsonHelper, IWebsiteEvents websiteEvents)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
            this.websiteEvents = websiteEvents ?? throw new ArgumentNullException(nameof(websiteEvents));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is AppPageModel appPageModel)
            {
                if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
                {
                    var websiteOptions = ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<WebsiteOptions>>().Value;

                    output.PostContent.AppendHtml($"    <meta charset=\"utf-8\" />{Environment.NewLine}");
                    if (websiteOptions.Adaptive != null && websiteOptions.Adaptive.Enable)
                    {
                        var viewportParams = new List<string>
                        {
                            $"width={websiteOptions.Adaptive.Width}",
                            $"initial-scale={websiteOptions.Adaptive.InitialScale}"
                        };
                        if (!string.IsNullOrEmpty(websiteOptions.Adaptive.MinimumScale))
                            viewportParams.Add($"initial-scale={websiteOptions.Adaptive.MinimumScale}");
                        if (!string.IsNullOrEmpty(websiteOptions.Adaptive.MaximumScale))
                            viewportParams.Add($"initial-scale={websiteOptions.Adaptive.MaximumScale}");

                        output.PostContent.AppendHtml($"    <meta name=\"viewport\" content=\"{string.Join(", ", viewportParams)}\" />{Environment.NewLine}");
                    }

                    var renderTitleContext = new RenderPageTitleContext(appPageModel);
                    await websiteEvents.RenderPageTitle(renderTitleContext);
                    output.PostContent.AppendHtml($"    <title>{renderTitleContext.Title ?? ""}</title>{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Description))
                        output.PostContent.AppendHtml($"    <meta id=\"page-meta-description\" name=\"description\" content=\"{appPageModel.Description}\">{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Keywords))
                        output.PostContent.AppendHtml($"    <meta id=\"page-meta-keywords\" name=\"keywords\" content=\"{appPageModel.Keywords}\">{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.CanonicalLink))
                        output.PostContent.AppendHtml($"    <link id=\"page-link-canonical\" rel=\"canonical\" href=\"{appPageModel.CanonicalLink}\">{Environment.NewLine}");

                    var og = appPageModel.OpenGraph;
                    if (og != null)
                    {
                        output.PostContent.AppendHtml($"    <meta property=\"og:{OpenGraphProperties.Type}\" content=\"{og.Type}\">{Environment.NewLine}");
                        output.PostContent.AppendHtml($"    <meta property=\"og:{OpenGraphProperties.Image}\" content=\"{og.Image}\">{Environment.NewLine}");
                        output.PostContent.AppendHtml($"    <meta property=\"og:{OpenGraphProperties.Title}\" content=\"{og.Title}\">{Environment.NewLine}");
                        output.PostContent.AppendHtml($"    <meta property=\"og:{OpenGraphProperties.Url}\" content=\"{og.Url}\">{Environment.NewLine}");
                        if (og.Description != null)
                            output.PostContent.AppendHtml($"    <meta property=\"og:{OpenGraphProperties.Description}\" content=\"{og.Description}\" />{Environment.NewLine}");
                    }

                    var startupModel = await appPageModel.GetStartupClientModelAsync(appPageModel);

                    output.PostContent.AppendHtml($"    <script>var appStartup = {jsonHelper.Serialize(startupModel)}</script>{Environment.NewLine}");

                    await websiteEvents.RenderHeadTag(new OnRenderTagContext(ViewContext, context, output));
                }
                else if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
                {
                    string cssClass = null;
                    if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute attribute))
                        cssClass = attribute.Value.ToString();

                    if (!string.IsNullOrEmpty(cssClass))
                        cssClass += " " + LoadingClass;
                    else
                        cssClass = LoadingClass;

                    if (!string.IsNullOrEmpty(appPageModel.CssClass))
                        cssClass += " " + appPageModel.CssClass;

                    output.Attributes.SetAttribute("class", cssClass);

                    await websiteEvents.RenderBodyTag(new OnRenderTagContext(ViewContext, context, output));
                }
            }
        }
    }
}