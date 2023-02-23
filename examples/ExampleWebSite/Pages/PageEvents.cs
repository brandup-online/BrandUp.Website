using BrandUp.Website.Pages;

namespace ExampleWebSite.Pages
{
    public class PageEvents : IPageEvents
    {
        public Task PageClientBuildAsync(PageClientBuildContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageClientNavigationAsync(PageClientNavidationContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRenderAsync(PageRenderContext context)
        {
            return Task.CompletedTask;
        }
    }
}