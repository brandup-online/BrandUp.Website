using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website
{
    public class DefaultWebsiteEvents : IWebsiteEvents
    {
        public virtual Task StartAsync(StartWebsiteContext context)
        {
            return Task.CompletedTask;
        }
        public virtual Task RenderBodyTag(OnRenderTagContext context)
        {
            return Task.CompletedTask;
        }
        public virtual Task RenderHeadTag(OnRenderTagContext context)
        {
            return Task.CompletedTask;
        }
    }

    public interface IWebsiteEvents
    {
        Task StartAsync(StartWebsiteContext context);
        Task RenderHeadTag(OnRenderTagContext context);
        Task RenderBodyTag(OnRenderTagContext context);
    }

    public class StartWebsiteContext(AppPageModel pageModel, IDictionary<string, object> clientData) : WebsiteEventContext(pageModel)
    {
        public IDictionary<string, object> ClientData { get; } = clientData ?? throw new ArgumentNullException(nameof(clientData));
    }

    public class OnRenderTagContext(ViewContext viewContext, TagHelperContext tagContext, TagHelperOutput tagOutput)
    {
        public ViewContext ViewContext { get; } = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
        public TagHelperContext TagContext { get; } = tagContext ?? throw new ArgumentNullException(nameof(tagContext));
        public TagHelperOutput TagOutput { get; } = tagOutput ?? throw new ArgumentNullException(nameof(tagOutput));
    }

    public class RenderPageTitleContext(AppPageModel pageModel) : WebsiteEventContext(pageModel)
    {
        public string Title { get; set; }
    }

    public class WebsiteEventContext(AppPageModel pageModel)
    {
        public AppPageModel PageModel { get; } = pageModel ?? throw new ArgumentNullException(nameof(pageModel));
        public HttpContext Http => PageModel.HttpContext;
        public CancellationToken CancellationToken => PageModel.CancellationToken;
        public IServiceProvider Services => PageModel.Services;
        public AppPageRequestMode RequestMode => PageModel.RequestMode;
        public WebsiteContext Website => PageModel.WebsiteContext;
    }
}