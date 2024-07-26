using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages.Examples.PageNavigation
{
    public class RedirectModel : AppPageModel
    {
        public override string Title => "Page redirect";

        public IActionResult OnGet([FromQuery] bool replace, [FromQuery] bool reload)
        {
            return PageRedirect(Url.Page("/About"), replace: replace, reload: reload);
        }
    }
}