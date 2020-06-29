using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExampleWebSite.Pages
{
    public class DeliveryModel : AppPageModel
    {
        public override string Title => "Delivery";
        public override string Description => Title;
        public override string Keywords => Title;

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            context.PageRedirect(Url.Page("/Contacts"), true);

            var p = Page();

            return Task.CompletedTask;
        }
    }
}