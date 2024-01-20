using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BrandUp.Website.Pages.Results
{
    public class PageActionResult(AppPageModel currentPage, PageActionType type) : ActionResult, IActionResult, IKeepTempDataResult
    {
        public AppPageModel CurrentPage { get; } = currentPage ?? throw new ArgumentNullException(nameof(currentPage));
        public PageActionType Type { get; } = type;

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            response.StatusCode = 200;
            response.Headers[PageConstants.HttpHeaderPageAction] = Type.ToString().ToLower();

            return Task.CompletedTask;
        }
    }
}