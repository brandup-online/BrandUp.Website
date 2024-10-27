using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.Middlewares
{
    public class WebsiteMiddleware
    {
        public const string HttpContextDomainIsAliasKey = "BRANDUP-WEBSITE-ISALIAS";
        const string Localhost = "localhost";

        readonly RequestDelegate next;
        readonly IOptions<WebsiteOptions> webSiteOptions;
        readonly string webSiteHost;
        readonly bool isLocalhost;

        public WebsiteMiddleware(RequestDelegate next, IOptions<WebsiteOptions> webSiteOptions)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.webSiteOptions = webSiteOptions ?? throw new ArgumentNullException(nameof(webSiteOptions));

            webSiteHost = this.webSiteOptions.Value.Host.ToLower();
            isLocalhost = webSiteHost.Equals(Localhost, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var websiteStore = context.RequestServices.GetRequiredService<IWebsiteStore>();
            var websiteProvider = context.RequestServices.GetRequiredService<IUrlMapProvider>();

            var request = context.Request;
            var requestHost = request.Host.Host.ToLower();
            var isLocalIp = isLocalhost && request.Host.Host.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase);

            // Redirect by www subdomain.
            if (!isLocalIp && requestHost.StartsWith("www", StringComparison.InvariantCultureIgnoreCase))
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: ReplaceHost(request.Host, webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Location = redirectUrl;
                return;
            }

            // Redirect by aliases.
            if (!isLocalIp && !requestHost.EndsWith(webSiteHost, StringComparison.InvariantCultureIgnoreCase))
            {
                if (webSiteOptions.Value.Aliases != null)
                {
                    foreach (var aliasHost in webSiteOptions.Value.Aliases)
                    {
                        if (aliasHost.Equals(requestHost, StringComparison.CurrentCultureIgnoreCase))
                        {
                            context.Items[HttpContextDomainIsAliasKey] = true;

                            if (request.Path.HasValue && request.Path.Value == "/robots.txt")
                            {
                                await next(context);
                                return;
                            }

                            var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: ReplaceHost(request.Host, webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                            context.Response.StatusCode = 301;
                            context.Response.Headers.Location = redirectUrl;
                            return;
                        }
                    }
                }

                context.Response.StatusCode = 404;
                return;
            }

            var needRedirectToHttps = request.Scheme == "http";

            string websiteName;
            if (!isLocalIp)
            {
                websiteName = websiteProvider.ExtractName(context, requestHost);
                websiteName ??= string.Empty;
            }
            else
                websiteName = string.Empty;

            var website = await websiteStore.FindByNameAsync(websiteName);
            if (website == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var aliases = await websiteStore.GetAliasesAsync(website);
            if (aliases != null && aliases.Length > 0)
            {
                foreach (var alias in aliases)
                {
                    if (string.Equals(websiteName, alias, StringComparison.OrdinalIgnoreCase))
                    {
                        var scheme = request.Scheme;
                        if (needRedirectToHttps && webSiteOptions.Value.RedirectToHttps)
                            scheme = "https";

                        var newHostValue = (!string.IsNullOrEmpty(website.Name) ? website.Name + "." : "") + webSiteHost;
                        var redirectUrl = UriHelper.BuildAbsolute(scheme: scheme, host: ReplaceHost(request.Host, newHostValue), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                        context.Response.StatusCode = 301;
                        context.Response.Headers.Location = redirectUrl;
                        return;
                    }
                }
            }

            if (needRedirectToHttps && webSiteOptions.Value.RedirectToHttps)
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: request.Host, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Location = redirectUrl;
                return;
            }

            var websitemTimeZone = await websiteStore.GetTimeZoneAsync(website);
            var websiteContext = new WebsiteContext(context, website, websitemTimeZone);

            context.Features.Set<IWebsiteFeature>(new WebsiteFeature(webSiteOptions.Value, websiteContext)
            {
                IsLocalIp = isLocalIp
            });

            if (request.QueryString.HasValue && request.Query.ContainsKey("_"))
            {
                // remove cache param from query string

                var newQuery = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(request.Query, StringComparer.OrdinalIgnoreCase);
                newQuery.Remove("_");

                request.Query = new QueryCollection(newQuery);
                request.QueryString = QueryString.Create(newQuery);
            }

            await next(context);
        }

        static HostString ReplaceHost(HostString current, string newHostValue)
        {
            if (!current.Port.HasValue || current.Port == 80 || current.Port == 443)
                return new HostString(newHostValue);

            return new HostString(newHostValue, current.Port.Value);
        }
    }
}