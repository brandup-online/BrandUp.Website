using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExampleWebSite.Pages
{
    public class DeliveryModel : AppPageModel
    {
        public override string Title => "Доставка";

        //public IActionResult OnGet()
        //{
        //    return PageRedirect(Url.Page("/Contacts"));
        //}

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            context.PageRedirect(Url.Page("/Contacts"));
            return Task.CompletedTask;
        }
    }
}