using Microsoft.AspNetCore.Http;

namespace BrandUp.Website
{
    public class WebsiteContext(HttpContext httpContext, IWebsite website, TimeZoneInfo timeZone) : IWebsiteContext
    {
        public HttpContext HttpContext { get; } = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        public IWebsite Website { get; } = website ?? throw new ArgumentNullException(nameof(website));
        public TimeZoneInfo TimeZone { get; } = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }
}