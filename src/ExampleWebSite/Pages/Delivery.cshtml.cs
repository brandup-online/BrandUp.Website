using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages
{
    public class DeliveryModel : WebPageModel
    {
        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            context.PageRedirect(Url.Page("/Contacts") ?? "/");

            return Task.CompletedTask;
        }
    }
}