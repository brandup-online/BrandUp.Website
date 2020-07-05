using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class NotFoundModel : AppPageModel
    {
        public override string Title => "Not found";
        public override string Description => Title;
        public override string Keywords => Title;
    }
}