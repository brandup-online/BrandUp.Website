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

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            SetOpenGraph(Url.ContentLink("~/images/og.jpg"));

            OpenGraph.SiteName = "Example website";

            return base.OnPageRequestAsync(context);
        }
    }
}