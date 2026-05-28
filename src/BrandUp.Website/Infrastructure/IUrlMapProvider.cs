using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.Infrastructure
{
    public interface IUrlMapProvider
    {
        string? ExtractName(HttpContext context, string requestHost);
    }

    public class SubdomainUrlMapProvider : IUrlMapProvider
    {
        readonly string webSiteHost;

        public SubdomainUrlMapProvider(IOptions<WebsiteOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            webSiteHost = options.Value.Host.ToLower();
        }

        public string? ExtractName(HttpContext context, string requestHost)
        {
            ArgumentNullException.ThrowIfNull(requestHost);

            if (string.Equals(requestHost, webSiteHost, StringComparison.InvariantCultureIgnoreCase))
                return null;

            var suffix = "." + webSiteHost;
            if (!requestHost.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return requestHost[..^suffix.Length].ToLower();
        }
    }

    public class PathUrlMapProvider : IUrlMapProvider
    {
        public string? ExtractName(HttpContext context, string requestHost)
        {
            ArgumentNullException.ThrowIfNull(context);

            string? websiteName = null;
            var requestPath = context.Request.Path;
            if (requestPath.HasValue)
            {
                var path = requestPath.Value!.ToLower().Trim(['/']);
                var firstSlashIndex = path.IndexOf('/', StringComparison.InvariantCultureIgnoreCase);
                if (firstSlashIndex > 0)
                    websiteName = path[..firstSlashIndex];
                else
                    websiteName = path;
            }

            return websiteName;
        }
    }
}