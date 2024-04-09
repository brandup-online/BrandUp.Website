namespace BrandUp.Website.Infrastructure
{
    internal class DefaultWebsiteClock(IWebsiteContext websiteContext) : IWebsiteClock
    {
        public DateTime Utc => DateTime.UtcNow;
        public DateTime Local => TimeZoneInfo.ConvertTime(Utc, websiteContext.TimeZone);
    }
}