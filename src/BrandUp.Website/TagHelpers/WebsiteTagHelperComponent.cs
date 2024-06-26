﻿using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    public class WebsiteTagHelperComponent(IJsonHelper jsonHelper, IWebsiteEvents websiteEvents) : TagHelperComponent
    {
        const string LoadingClass = "bp-state-loading";

        readonly IJsonHelper jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        readonly IWebsiteEvents websiteEvents = websiteEvents ?? throw new ArgumentNullException(nameof(websiteEvents));

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not AppPageModel appPageModel)
                return;

            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                var outputContent = output.PreContent;

                outputContent.AppendHtml($"{Environment.NewLine}    <meta charset=\"utf-8\" />{Environment.NewLine}");

                outputContent.AppendHtml($"    <title>{appPageModel.Title ?? ""}</title>{Environment.NewLine}");

                if (!string.IsNullOrEmpty(appPageModel.Description))
                    outputContent.AppendHtml($"    <meta id=\"page-meta-description\" name=\"description\" content=\"{appPageModel.Description}\">{Environment.NewLine}");

                if (!string.IsNullOrEmpty(appPageModel.Keywords))
                    outputContent.AppendHtml($"    <meta id=\"page-meta-keywords\" name=\"keywords\" content=\"{appPageModel.Keywords}\">{Environment.NewLine}");

                var canonicalLink = appPageModel.CanonicalLink;
                if (canonicalLink == null)
                    canonicalLink = appPageModel.Link;
                outputContent.AppendHtml($"    <link id=\"page-link-canonical\" rel=\"canonical\" href=\"{canonicalLink}\">{Environment.NewLine}");

                var og = appPageModel.OpenGraph;
                if (og != null)
                {
                    outputContent.AppendHtml($"    <meta id=\"og-type\" property=\"og:{OpenGraphProperties.Type}\" content=\"{og.Type}\">{Environment.NewLine}");
                    outputContent.AppendHtml($"    <meta id=\"og-image\" property=\"og:{OpenGraphProperties.Image}\" content=\"{og.Image}\">{Environment.NewLine}");
                    outputContent.AppendHtml($"    <meta id=\"og-title\" property=\"og:{OpenGraphProperties.Title}\" content=\"{og.Title}\">{Environment.NewLine}");
                    outputContent.AppendHtml($"    <meta id=\"og-url\" property=\"og:{OpenGraphProperties.Url}\" content=\"{og.Url}\">{Environment.NewLine}");
                    if (og.SiteName != null)
                        outputContent.AppendHtml($"    <meta id=\"og-site_name\" property=\"og:{OpenGraphProperties.SiteName}\" content=\"{og.SiteName}\" />{Environment.NewLine}");
                    if (og.Description != null)
                        outputContent.AppendHtml($"    <meta id=\"og-description\" property=\"og:{OpenGraphProperties.Description}\" content=\"{og.Description}\" />{Environment.NewLine}");
                }

                var startupModel = await appPageModel.GetStartupClientModelAsync();

                outputContent.AppendHtml($"    <script id=\"app-data\" type=\"application/json\">{jsonHelper.Serialize(startupModel)}</script>{Environment.NewLine}");

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