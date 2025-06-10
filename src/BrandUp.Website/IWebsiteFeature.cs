namespace BrandUp.Website
{
    internal class WebsiteFeature(WebsiteOptions options, WebsiteContext context) : IWebsiteFeature
    {
        public WebsiteOptions Options { get; } = options ?? throw new ArgumentNullException(nameof(options));
        public WebsiteContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
        public bool IsLocalIp { get; internal set; }
    }

    public interface IWebsiteFeature
    {
        WebsiteOptions Options { get; }
        WebsiteContext Context { get; }
        bool IsLocalIp { get; }
    }
}