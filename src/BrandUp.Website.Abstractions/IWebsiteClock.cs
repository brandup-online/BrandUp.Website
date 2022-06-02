using System;

namespace BrandUp.Website
{
    public interface IWebsiteClock
    {
        DateTime Utc { get; }
        DateTime Local { get; }
    }
}