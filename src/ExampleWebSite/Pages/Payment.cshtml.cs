using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages
{
    [Authorize]
    public class PaymentModel : AppPageModel
    {
        public override string Title => "Payment";
        public override string Description => Title;
        public override string Keywords => Title;

        [FromQuery]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public string Value { get; set; }

        public IActionResult OnPostRedirect()
        {
            if (string.IsNullOrEmpty(ReturnUrl))
                ReturnUrl = Url.Page("/Contacts");

            return PageRedirect(ReturnUrl);
        }

        public IActionResult OnPostPaste()
        {
            Value = "100";

            return Page();
        }
    }
}