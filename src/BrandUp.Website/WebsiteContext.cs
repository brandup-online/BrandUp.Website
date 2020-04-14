using Microsoft.AspNetCore.Http;
using System;

namespace BrandUp.Website
{
    public class WebsiteContext
    {
        public HttpContext HttpContext { get; }
        public IWebsite Website { get; }
        public TimeZoneInfo TimeZone { get; }
        public IVisitor Visitor { get; set; }

        public WebsiteContext(HttpContext httpContext, IWebsite website, TimeZoneInfo timeZone)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Website = website ?? throw new ArgumentNullException(nameof(website));
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }
    }
}