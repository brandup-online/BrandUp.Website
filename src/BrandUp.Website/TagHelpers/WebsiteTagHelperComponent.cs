using System.Text;
using System.Text.Encodings.Web;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.TagHelpers
{
    public class WebsiteTagHelperComponent(IJsonHelper jsonHelper, IWebsiteEvents websiteEvents, HtmlEncoder htmlEncoder) : TagHelperComponent
    {
        const string LoadingClass = "bp-state-loading";

        readonly IJsonHelper jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        readonly IWebsiteEvents websiteEvents = websiteEvents ?? throw new ArgumentNullException(nameof(websiteEvents));
        readonly HtmlEncoder htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not AppPageModel appPageModel)
                return;

            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                var outputContent = output.PreContent;

                var head = new StringBuilder();

                head.Append(Environment.NewLine).Append("    <meta charset=\"utf-8\" />").Append(Environment.NewLine);
                head.Append("    <title>").Append(Encode(appPageModel.Title)).Append("</title>").Append(Environment.NewLine);

                if (!string.IsNullOrEmpty(appPageModel.Description))
                    head.Append("    <meta id=\"page-meta-description\" name=\"description\" content=\"").Append(Encode(appPageModel.Description)).Append("\">").Append(Environment.NewLine);

                if (!string.IsNullOrEmpty(appPageModel.Keywords))
                    head.Append("    <meta id=\"page-meta-keywords\" name=\"keywords\" content=\"").Append(Encode(appPageModel.Keywords)).Append("\">").Append(Environment.NewLine);

                var canonicalLink = appPageModel.CanonicalLink ?? appPageModel.Link;
                head.Append("    <link id=\"page-link-canonical\" rel=\"canonical\" href=\"").Append(Encode(canonicalLink?.ToString())).Append("\">").Append(Environment.NewLine);

                var og = appPageModel.OpenGraph;
                if (og != null)
                {
                    foreach (var (name, content) in og.CreateClientModel())
                    {
                        head.Append("    <meta id=\"og-").Append(Encode(name)).Append("\" property=\"og:").Append(Encode(name)).Append("\" content=\"").Append(Encode(content)).Append("\">").Append(Environment.NewLine);
                    }
                }

                outputContent.AppendHtml(head.ToString());

                var startupModel = await appPageModel.GetStartupClientModelAsync();

                outputContent.AppendHtml($"    <script id=\"app-data\" type=\"application/json\">{jsonHelper.Serialize(startupModel)}</script>{Environment.NewLine}");

                await websiteEvents.RenderHeadTag(new OnRenderTagContext(ViewContext, context, output));
            }
            else if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                string? cssClass = null;
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

        string Encode(string? value) => value == null ? string.Empty : htmlEncoder.Encode(value);
    }
}