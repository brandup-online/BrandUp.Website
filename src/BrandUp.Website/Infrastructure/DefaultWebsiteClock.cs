using System;

namespace BrandUp.Website.Infrastructure
{
    internal class DefaultWebsiteClock : IWebsiteClock
    {
        readonly private WebsiteContext websiteContext;

        public DateTime Utc => DateTime.UtcNow;
        public DateTime Local => TimeZoneInfo.ConvertTime(Utc, websiteContext.TimeZone);

        public DefaultWebsiteClock(WebsiteContext websiteContext)
        {
            this.websiteContext = websiteContext ?? throw new ArgumentNullException(nameof(websiteContext));
        }
    }
}