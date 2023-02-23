using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Website.Pages
{
    public interface IPageEvents
    {
        Task PageRequestAsync(PageRequestContext context);
        Task PageClientNavigationAsync(PageClientNavidationContext context);
        Task PageClientBuildAsync(PageClientBuildContext context);
        Task PageRenderAsync(PageRenderContext context);
    }

    public class PageRequestContext : WebsiteEventContext
    {
        public IActionResult Result { get; set; }

        public PageRequestContext(AppPageModel pageModel) : base(pageModel) { }
    }

    public class PageClientNavidationContext : WebsiteEventContext
    {
        public IDictionary<string, object> ClientData { get; }

        public PageClientNavidationContext(AppPageModel pageModel, IDictionary<string, object> clientData) : base(pageModel)
        {
            ClientData = clientData ?? throw new ArgumentNullException(nameof(clientData));
        }
    }

    public class PageClientBuildContext : WebsiteEventContext
    {
        public IDictionary<string, object> ClientData { get; }

        public PageClientBuildContext(AppPageModel pageModel, IDictionary<string, object> clientData) : base(pageModel)
        {
            ClientData = clientData ?? throw new ArgumentNullException(nameof(clientData));
        }
    }

    public class PageRenderContext : WebsiteEventContext
    {
        public PageRenderContext(AppPageModel pageModel, Microsoft.AspNetCore.Mvc.Razor.IRazorPage page) : base(pageModel)
        {
            Page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public Microsoft.AspNetCore.Mvc.Razor.IRazorPage Page { get; }
    }
}