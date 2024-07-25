using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages.Examples.PageNavigation
{
    public class IndexModel : AppPageModel
    {
        #region AppPageModel members

        public override string Title => "Page navigation";
        public override string ScriptName => "examples-navigation";

        #endregion

        [FromQuery]
        public string Test { get; set; }
    }
}