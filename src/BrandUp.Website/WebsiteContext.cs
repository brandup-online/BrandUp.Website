using Microsoft.AspNetCore.Http;
using System;

namespace BrandUp.Website
{
    public class WebsiteContext
    {
        public HttpContext HttpContext { get; }
        public IWebsite Website { get; }

        public WebsiteContext(HttpContext httpContext, IWebsite website)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Website = website ?? throw new ArgumentNullException(nameof(website));
        }
    }
}