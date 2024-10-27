using System.ComponentModel.DataAnnotations;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages.Examples.PageSubmit
{
    public class IndexModel : AppPageModel
    {
        public override string Title => "Page submit";
        public override string ScriptName => "example-submit";

        [BindProperty, Required]
        public string Key { get; set; }

        [BindProperty]
        public string Value { get; set; }

        public bool IsSaved { get; private set; }

        public void OnGet()
        {
            Value = "test";
        }

        public void OnPostGenerate()
        {
            ModelState.Clear();

            Value = "test2";
        }

        public IActionResult OnPostSave()
        {
            IsSaved = true;

            return Page();
        }

        public IActionResult OnPostInternalRedirect()
        {
            return PageRedirect(Url.Page("/Index"));
        }

        public IActionResult OnPostInternalRedirectReload()
        {
            return PageRedirect(Url.Page("/Index"), reload: true);
        }

        public IActionResult OnPostExtenalRedirect()
        {
            return PageRedirect("https://yandex.ru");
        }

        public IActionResult OnPostError()
        {
            return BadRequest();
        }

        public async Task<IActionResult> OnPostLong()
        {
            await Task.Delay(5000, HttpContext.RequestAborted);

            return new OkResult();
        }
    }
}