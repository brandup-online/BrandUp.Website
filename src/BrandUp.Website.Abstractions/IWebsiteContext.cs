using System;

namespace BrandUp.Website
{
    public interface IWebsiteContext
    {
        IWebsite Website { get; }
        TimeZoneInfo TimeZone { get; }
    }
}