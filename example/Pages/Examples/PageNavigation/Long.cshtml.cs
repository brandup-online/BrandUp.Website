using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages.Examples.PageNavigation
{
    public class LongModel : AppPageModel
    {
        public override string Title => "Page navigation long";

        public async Task<IActionResult> OnGet()
        {
            await Task.Delay(5000, HttpContext.RequestAborted);

            return Page();
        }
    }
}