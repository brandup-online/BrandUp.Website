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

    public class StartWebsiteContext : WebsiteEventContext
    {
        public IDictionary<string, object> ClientData { get; }

        public StartWebsiteContext(AppPageModel pageModel, IDictionary<string, object> clientData) : base(pageModel)
        {
            ClientData = clientData ?? throw new ArgumentNullException(nameof(clientData));
        }
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

    public class RenderPageTitleContext : WebsiteEventContext
    {
        public string Title { get; set; }

        public RenderPageTitleContext(AppPageModel pageModel) : base(pageModel) { }
    }

    public class WebsiteEventContext
    {
        public AppPageModel PageModel { get; }
        public HttpContext Http => PageModel.HttpContext;
        public CancellationToken CancellationToken => PageModel.CancellationToken;
        public IServiceProvider Services => PageModel.Services;
        public AppPageRequestMode RequestMode => PageModel.RequestMode;
        public WebsiteContext Website => PageModel.WebsiteContext;

        public WebsiteEventContext(AppPageModel pageModel)
        {
            PageModel = pageModel ?? throw new ArgumentNullException(nameof(pageModel));
        }
    }
}