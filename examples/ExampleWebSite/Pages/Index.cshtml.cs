using BrandUp.Website;
using BrandUp.Website.Pages;
using System.Threading.Tasks;

namespace ExampleWebSite.Pages
{
    public class IndexModel : AppPageModel
    {
        public override string Title => "Main";
        public override string Description => Title;
        public override string Keywords => Title;
        public override string Header => Title;

        /// <summary>
        /// Вызывается при запросе страницы.
        /// </summary>
        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            OpenGraph = new PageOpenGraph("website", Url.ContentLink("~/images/og.jpg"), Title, Link, Description);

            return base.OnPageRequestAsync(context);
        }

        /// <summary>
        /// Вызывается при конструировании контекста сайта для клиента.
        /// </summary>
        protected override Task OnPageClientBuildAsync(PageClientBuildContext context)
        {
            return base.OnPageClientBuildAsync(context);
        }

        /// <summary>
        /// Вызывается при конструировании контекста навигации для клиента.
        /// </summary>
        protected override Task OnPageClientNavigationAsync(PageClientNavidationContext context)
        {
            return base.OnPageClientNavigationAsync(context);
        }

        /// <summary>
        /// Вызывается при рендеринге представления страницы.
        /// </summary>
        protected override Task OnPageRenderAsync(PageRenderContext context)
        {
            return base.OnPageRenderAsync(context);
        }
    }
}