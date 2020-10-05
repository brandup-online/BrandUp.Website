using System;

namespace BrandUp.Website.Infrastructure
{
    public class WebsiteFeature : IWebsiteFeature
    {
        public WebsiteOptions Options { get; }
        public WebsiteContext Context { get; }

        public WebsiteFeature(WebsiteOptions options, WebsiteContext context)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }

    public interface IWebsiteFeature
    {
        WebsiteOptions Options { get; }
        WebsiteContext Context { get; }
    }
}