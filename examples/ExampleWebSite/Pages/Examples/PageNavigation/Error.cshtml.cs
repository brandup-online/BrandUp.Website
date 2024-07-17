using BrandUp.Website.Pages;

namespace ExampleWebSite.Pages.Examples.PageNavigation
{
    public class ErrorModel : AppPageModel
    {
        public override string Title => "Page navigation error";

        public void OnGet()
        {
            throw new Exception("test");
        }
    }
}