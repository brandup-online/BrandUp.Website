using System;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Website
{
    public class WebsiteContext
    {
        public HttpContext HttpContext { get; }
        public IWebsite Website { get; }
        public TimeZoneInfo TimeZone { get; }
        public Visitors.IVisitor Visitor { get; private set; }

        public WebsiteContext(HttpContext httpContext, IWebsite website, TimeZoneInfo timeZone)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Website = website ?? throw new ArgumentNullException(nameof(website));
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        //internal void SetVisitor(Visitors.IVisitor visitor)
        //{
        //    Visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
        //}
    }
}