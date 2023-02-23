using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BrandUp.Website.Pages.Results
{
    public class PageActionResult : ActionResult, IActionResult, IKeepTempDataResult
    {
        public AppPageModel CurrentPage { get; }
        public PageActionType Type { get; }

        public PageActionResult(AppPageModel currentPage, PageActionType type)
        {
            CurrentPage = currentPage ?? throw new ArgumentNullException(nameof(currentPage));
            Type = type;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            response.StatusCode = 200;
            response.Headers.Add("Page-Action", Type.ToString().ToLower());

            return Task.CompletedTask;
        }
    }
}