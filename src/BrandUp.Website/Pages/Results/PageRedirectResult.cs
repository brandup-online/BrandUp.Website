﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BrandUp.Website.Pages.Results
{
    public class PageRedirectResult(string pageUrl) : ActionResult, IActionResult, IKeepTempDataResult
    {
        public string PageUrl { get; } = pageUrl ?? throw new ArgumentNullException(nameof(pageUrl));
        public bool IsPermament { get; set; }
        public bool ReplaceUrl { get; set; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.RedirectPage(PageUrl, IsPermament, ReplaceUrl);

            return Task.CompletedTask;
        }
    }
}