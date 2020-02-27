using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Website.TagHelpers
{
    public class EmbeddingTagHelperComponent : TagHelperComponent
    {
        const string LoadingClass = "bp-state-loading";

        private readonly IJsonHelper jsonHelper;

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public EmbeddingTagHelperComponent(IJsonHelper jsonHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is AppPageModel appPageModel)
            {
                if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
                {
                    output.PostContent.AppendHtml($"    <meta charset=\"utf-8\" />{Environment.NewLine}");
                    output.PostContent.AppendHtml($"    <title>{appPageModel.Title}</title>{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Description))
                        output.PostContent.AppendHtml($"    <meta name=\"description\" content=\"{appPageModel.Description}\">{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Keywords))
                        output.PostContent.AppendHtml($"    <meta name=\"keywords\" content=\"{appPageModel.Keywords}\">{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.CanonicalLink))
                        output.PostContent.AppendHtml($"    <link rel=\"canonical\" href=\"{appPageModel.CanonicalLink}\">{Environment.NewLine}");

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

                    var appClientModel = await appPageModel.GetAppClientModelAsync(ViewContext.HttpContext.RequestAborted);

                    output.PostContent.AppendHtml($"    <script>var appInitOptions = {jsonHelper.Serialize(appClientModel)}</script>{Environment.NewLine}");
                }

                if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
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
                }
            }
        }
    }
}