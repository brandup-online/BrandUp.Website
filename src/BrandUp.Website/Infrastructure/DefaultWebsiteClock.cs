namespace BrandUp.Website.Infrastructure
{
    internal class DefaultWebsiteClock : IWebsiteClock
    {
        readonly private IWebsiteContext websiteContext;

        public DateTime Utc => DateTime.UtcNow;
        public DateTime Local => TimeZoneInfo.ConvertTime(Utc, websiteContext.TimeZone);

        public DefaultWebsiteClock(IWebsiteContext websiteContext)
        {
            this.websiteContext = websiteContext ?? throw new ArgumentNullException(nameof(websiteContext));
        }
    }
}