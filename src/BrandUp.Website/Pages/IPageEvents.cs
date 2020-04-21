using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Website.Pages
{
    public interface IPageEvents
    {
        Task PageRequestAsync(PageRequestContext context);
        Task PageNavigationAsync(PageNavidationContext context);
        Task PageBuildAsync(PageBuildContext context);
        Task PageRenderAsync(PageRenderContext context);
    }

    public class PageRequestContext : WebsiteEventContext
    {
        public IActionResult Result { get; set; }

        public PageRequestContext(AppPageModel pageModel) : base(pageModel) { }
    }

    public class PageNavidationContext : WebsiteEventContext
    {
        public IDictionary<string, object> ClientData { get; }

        public PageNavidationContext(AppPageModel pageModel, IDictionary<string, object> clientData) : base(pageModel)
        {
            ClientData = clientData ?? throw new ArgumentNullException(nameof(clientData));
        }
    }

    public class PageBuildContext : WebsiteEventContext
    {
        public IDictionary<string, object> ClientData { get; }

        public PageBuildContext(AppPageModel pageModel, IDictionary<string, object> clientData) : base(pageModel)
        {
            ClientData = clientData ?? throw new ArgumentNullException(nameof(clientData));
        }
    }

    public class PageRenderContext : WebsiteEventContext
    {
        public PageRenderContext(AppPageModel pageModel) : base(pageModel)
        {
        }
    }
}