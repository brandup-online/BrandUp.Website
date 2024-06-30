using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website.Pages
{
    public interface IPageEvents
    {
        Task PageRequestAsync(PageRequestContext context);
        Task PageClientNavigationAsync(PageClientNavidationContext context);
        Task PageClientBuildAsync(PageClientBuildContext context);
        Task PageRenderAsync(PageRenderContext context);
    }

    public class PageRequestContext(AppPageModel pageModel) : WebsiteEventContext(pageModel)
    {
        public IActionResult Result { get; set; }

        public bool HasResult => Result != null;
    }

    public class PageClientNavidationContext(AppPageModel pageModel, IDictionary<string, object> clientData) : WebsiteEventContext(pageModel)
    {
        public IDictionary<string, object> ClientData { get; } = clientData ?? throw new ArgumentNullException(nameof(clientData));
    }

    public class PageClientBuildContext(AppPageModel pageModel, IDictionary<string, object> clientData) : WebsiteEventContext(pageModel)
    {
        public IDictionary<string, object> ClientData { get; } = clientData ?? throw new ArgumentNullException(nameof(clientData));
    }

    public class PageRenderContext(AppPageModel pageModel, Microsoft.AspNetCore.Mvc.Razor.IRazorPage page, TagHelperOutput output) : WebsiteEventContext(pageModel)
    {
        public Microsoft.AspNetCore.Mvc.Razor.IRazorPage Page { get; } = page ?? throw new ArgumentNullException(nameof(page));

        public string TagName
        {
            get => output.TagName;
            set => output.TagName = value;
        }

        public TagHelperAttributeList Attributes => output.Attributes;
    }
}