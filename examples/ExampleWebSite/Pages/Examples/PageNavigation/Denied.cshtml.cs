using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Authorization;

namespace ExampleWebSite.Pages.Examples.PageNavigation
{
    [Authorize]
    public class DeniedModel : AppPageModel
    {
        public override string Title => "Page navigation denied";
    }
}