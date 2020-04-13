using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Website
{
    public class WebsiteEvents
    {
        public Func<OnRenderTagContext, Task> OnRenderHeadTag { get; set; } = (context) => Task.CompletedTask;
        public Func<OnRenderTagContext, Task> OnRenderBodyTag { get; set; } = (context) => Task.CompletedTask;
        public Func<OnRenderPageTitleContext, Task> OnRenderPageTitle { get; set; } = (context) =>
        {
            context.Title = context.PageModel.Title;

            return Task.CompletedTask;
        };
    }

    public class OnRenderTagContext
    {
        public ViewContext ViewContext { get; }
        public TagHelperContext TagContext { get; }
        public TagHelperOutput TagOutput { get; }

        public OnRenderTagContext(ViewContext viewContext, TagHelperContext tagContext, TagHelperOutput tagOutput)
        {
            ViewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
            TagContext = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
            TagOutput = tagOutput ?? throw new ArgumentNullException(nameof(tagOutput));
        }
    }

    public class OnRenderPageTitleContext
    {
        public Pages.AppPageModel PageModel { get; }
        public string Title { get; set; }

        public OnRenderPageTitleContext(Pages.AppPageModel pageModel)
        {
            PageModel = pageModel ?? throw new ArgumentNullException(nameof(pageModel));
        }
    }
}