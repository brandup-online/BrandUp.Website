using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.Infrastructure
{
    public interface IUrlMapProvider
    {
        string ExtractName(HttpContext context, string requestHost);
    }

    public class SubdomainUrlMapProvider : IUrlMapProvider
    {
        readonly string webSiteHost;

        public SubdomainUrlMapProvider(IOptions<WebsiteOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            webSiteHost = options.Value.Host.ToLower();
        }

        public string ExtractName(HttpContext context, string requestHost)
        {
            if (requestHost == null)
                throw new ArgumentNullException(nameof(requestHost));

            string websiteName = null;
            if (!string.Equals(requestHost, webSiteHost, StringComparison.InvariantCultureIgnoreCase))
                websiteName = requestHost[..(requestHost.Length - webSiteHost.Length - 1)].ToLower();
            return websiteName;
        }
    }

    public class PathUrlMapProvider : IUrlMapProvider
    {
        public string ExtractName(HttpContext context, string requestHost)
        {
            ArgumentNullException.ThrowIfNull(context);

            string websiteName = null;
            var requestPath = context.Request.Path;
            if (requestPath.HasValue)
            {
                var path = requestPath.Value.ToLower().Trim(['/']);
                var firstShlashIndex = path.IndexOf('/', StringComparison.InvariantCultureIgnoreCase);
                if (firstShlashIndex > 0)
                    websiteName = path[..firstShlashIndex];
                else
                    websiteName = path;
            }

            return websiteName;
        }
    }
}