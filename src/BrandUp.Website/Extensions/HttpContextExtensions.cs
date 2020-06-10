using Microsoft.AspNetCore.Http;
using System;

namespace BrandUp.Website
{
    public static class HttpContextExtensions
    {
        public static WebsiteContext GetWebsiteContext(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            return (WebsiteContext)httpContext.Items[Middlewares.WebsiteMiddleware.HttpContextWebsiteKey];
        }
    }
}