﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BrandUp.Website.Pages.Results
{
    public class PageRedirectResult : ActionResult, IActionResult, IKeepTempDataResult
    {
        public AppPageModel CurrentPage { get; }
        public string PageUrl { get; }
        public bool IsPermament { get; set; }
        public bool ReplaceUrl { get; set; }

        public PageRedirectResult(AppPageModel currentPage, string pageUrl)
        {
            CurrentPage = currentPage ?? throw new ArgumentNullException(nameof(currentPage));
            PageUrl = pageUrl ?? throw new ArgumentNullException(nameof(pageUrl));
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            if (CurrentPage.RequestMode == AppPageRequestMode.Start)
            {
                response.StatusCode = (int)(IsPermament ? HttpStatusCode.PermanentRedirect : HttpStatusCode.Redirect);
                response.Headers.Add("Location", PageUrl);
            }
            else
            {
                response.StatusCode = 200;
                response.Headers.Add("Page-Location", PageUrl);
                if (ReplaceUrl)
                    response.Headers.Add("Page-Replace", "true");
            }

            return Task.CompletedTask;
        }
    }
}