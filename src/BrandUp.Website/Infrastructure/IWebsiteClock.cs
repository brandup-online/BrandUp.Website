using System;

namespace BrandUp.Website.Infrastructure
{
    public interface IWebsiteClock
    {
        DateTime Utc { get; }
        DateTime Local { get; }
    }
}