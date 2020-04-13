using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BrandUp.Website.Middlewares
{
    public class WebSiteMiddleware
    {
        public const string HttpContextWebsiteKey = "BRANDUP-WEBSITE-CONTEXT";
        public const string HttpContextDomainIsAliasKey = "BRANDUP-WEBSITE-ISALIAS";

        private readonly RequestDelegate next;
        private readonly IOptions<WebsiteOptions> webSiteOptions;
        private readonly IWebsiteStore websiteStore;
        private readonly IWebsiteProvider websiteProvider;
        private readonly string webSiteHost;

        public WebSiteMiddleware(RequestDelegate next, IOptions<WebsiteOptions> webSiteOptions, IWebsiteStore websiteStore, IWebsiteProvider websiteProvider)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.webSiteOptions = webSiteOptions ?? throw new ArgumentNullException(nameof(webSiteOptions));
            this.websiteStore = websiteStore ?? throw new ArgumentNullException(nameof(websiteStore));
            this.websiteProvider = websiteProvider ?? throw new ArgumentNullException(nameof(websiteProvider));

            webSiteHost = this.webSiteOptions.Value.Host.ToLower();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var requestHost = request.Host.Host.ToLower();

            // Redirect by www subdomain.
            if (requestHost.StartsWith("www"))
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: new HostString(webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", redirectUrl);
                return;
            }

            // Redirect by aliases.
            if (!requestHost.EndsWith(webSiteHost))
            {
                if (webSiteOptions.Value.Aliases != null)
                {
                    foreach (var aliasHost in webSiteOptions.Value.Aliases)
                    {
                        if (aliasHost.ToLower() == requestHost)
                        {
                            context.Items[HttpContextDomainIsAliasKey] = true;

                            if (request.Path.HasValue && request.Path.Value == "/robots.txt")
                            {
                                await next(context);
                                return;
                            }

                            var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: new HostString(webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                            context.Response.StatusCode = 301;
                            context.Response.Headers.Add("Location", redirectUrl);
                            return;
                        }
                    }
                }

                context.Response.StatusCode = 404;
                return;
            }

            // Redirect to https.
            if (request.Scheme == "http")
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: request.Host, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", redirectUrl);
                return;
            }

            var websiteName = websiteProvider.GetWebsiteName(context, requestHost);
            if (websiteName == null)
                websiteName = string.Empty;

            var website = await websiteStore.FindWebsiteByNameAsync(websiteName);
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
                        var redirectHost = new HostString((!string.IsNullOrEmpty(website.Name) ? website.Name + "." : "") + webSiteHost);
                        var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: redirectHost, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                        context.Response.StatusCode = 301;
                        context.Response.Headers.Add("Location", redirectUrl);
                    }
                }
            }

            var websitemTimeZone = await websiteStore.GetTimeZoneAsync(website);

            var websiteContext = new WebsiteContext(context, website, websitemTimeZone);

            context.Items.Add(HttpContextWebsiteKey, websiteContext);

            await next(context);
        }
    }
}