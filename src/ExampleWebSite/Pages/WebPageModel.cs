using BrandUp.Website;
using BrandUp.Website.Pages;

namespace ExampleWebSite.Pages
{
    public class WebPageModel : AppPageModel
    {
        string title = "page";
        string description = null;
        string keywords = null;
        string header = null;

        public override string Title => title;
        public override string Description => description;
        public override string Keywords => keywords;
        public override string Header => header ?? title;

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            OpenGraph = new PageOpenGraph("website", Url.ContentLink("~/images/og.jpg"), Title, Link, Description);

            return base.OnPageRequestAsync(context);
        }

        public void SetTitle(string title)
        {
            ArgumentNullException.ThrowIfNull(title);

            this.title = title;
        }

        public void SetDescription(string description)
        {
            ArgumentNullException.ThrowIfNull(description);

            this.description = description;
        }

        public void SetKeywords(string keywords)
        {
            ArgumentNullException.ThrowIfNull(keywords);

            this.keywords = keywords;
        }

        public void SetHeader(string header)
        {
            ArgumentNullException.ThrowIfNull(header);

            this.header = header;
        }
    }
}