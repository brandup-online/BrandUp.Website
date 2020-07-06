using System;

namespace BrandUp.Website.Infrastructure
{
    public class WebsiteFeature : IWebsiteFeature
    {
        public WebsiteContext WebsiteContext { get; }

        public WebsiteFeature(WebsiteContext context)
        {
            WebsiteContext = context ?? throw new ArgumentNullException(nameof(context));
        }
    }

    public interface IWebsiteFeature
    {
        WebsiteContext WebsiteContext { get; }
    }
}