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
            SetOpenGraph(Url.ContentLink("~/images/og.jpg"));

            OpenGraph.SiteName = "Example website";

            return base.OnPageRequestAsync(context);
        }

        /// <summary>
        /// Вызывается при конструировании контекста сайта для клиента.
        /// </summary>
        protected override Task OnPageBuildAsync(PageBuildContext context)
        {
            return base.OnPageBuildAsync(context);
        }

        /// <summary>
        /// Вызывается при конструировании контекста навигации для клиента.
        /// </summary>
        protected override Task OnPageNavigationAsync(PageNavidationContext context)
        {
            return base.OnPageNavigationAsync(context);
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