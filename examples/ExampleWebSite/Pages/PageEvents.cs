using BrandUp.Website.Pages;
using System.Threading.Tasks;

namespace ExampleWebSite.Pages
{
    public class PageEvents : IPageEvents
    {
        public Task PageBuildAsync(PageBuildContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageNavigationAsync(PageNavidationContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRenderAsync(PageRenderContext context)
        {
            return Task.CompletedTask;
        }
    }
}