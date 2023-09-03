using System.ComponentModel.DataAnnotations;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages.Examples.PageHandlers
{
    public class IndexModel : AppPageModel
    {
        public override string Title => "Page handlers";
        public override string ScriptName => "form";

        [BindProperty, Required]
        public string Key { get; set; }

        [BindProperty]
        public string Value { get; set; }

        public void OnGet()
        {
            Value = "test";
        }

        public void OnPostGenerate()
        {
            ModelState.Clear();

            Value = "test2";
        }

        public void OnPostSave()
        {
        }
    }
}