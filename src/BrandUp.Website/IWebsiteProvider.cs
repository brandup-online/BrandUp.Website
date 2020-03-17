using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace BrandUp.Website
{
    public interface IWebsiteProvider
    {
        string GetWebsiteName(HttpContext context, string requestHost);
    }

    public class SubdomainWebsiteProvider : IWebsiteProvider
    {
        readonly string webSiteHost;

        public SubdomainWebsiteProvider(IOptions<WebsiteOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            webSiteHost = options.Value.Host.ToLower();
        }

        public string GetWebsiteName(HttpContext context, string requestHost)
        {
            string websiteName = null;
            if (!string.Equals(requestHost, webSiteHost, StringComparison.OrdinalIgnoreCase))
                websiteName = requestHost.Substring(0, webSiteHost.Length + 1).ToLower();
            return websiteName;
        }
    }

    public class PathWebsiteProvider : IWebsiteProvider
    {
        public string GetWebsiteName(HttpContext context, string requestHost)
        {
            string websiteName = null;
            var requestPath = context.Request.Path;
            if (requestPath.HasValue)
            {
                var path = requestPath.Value.ToLower().Trim(new char[] { '/' });
                var firstShlashIndex = path.IndexOf('/');
                if (firstShlashIndex > 0)
                    websiteName = path.Substring(0, firstShlashIndex);
                else
                    websiteName = path;
            }
            return websiteName;
        }
    }
}