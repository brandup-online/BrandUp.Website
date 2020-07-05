using Microsoft.AspNetCore.Mvc;
using System;

namespace BrandUp.Website
{
    public static class IUrlHelperExtensions
    {
        public static Uri ContentLink(this IUrlHelper urlHelper, string contentPath)
        {
            if (urlHelper == null)
                throw new ArgumentNullException(nameof(urlHelper));

            var httpRequest = urlHelper.ActionContext.HttpContext.Request;
            var urlPath = urlHelper.Content(contentPath);

            var uriBuilder = new UriBuilder(httpRequest.Scheme, httpRequest.Host.Host)
            {
                Path = urlPath
            };

            if (httpRequest.Host.Port.HasValue)
                uriBuilder.Port = httpRequest.Host.Port.Value;

            return uriBuilder.Uri;
        }
    }
}