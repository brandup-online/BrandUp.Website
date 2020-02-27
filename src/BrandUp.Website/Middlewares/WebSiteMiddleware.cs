using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BrandUp.Website.Middlewares
{
    public class WebSiteMiddleware
    {
        public const string HttpContextDomainIsAliasKey = "DOMAIN-IS-ALIAS";

        private readonly RequestDelegate next;
        private readonly IOptions<WebSiteOptions> webSiteOptions;
        private readonly string webSiteHost;

        public WebSiteMiddleware(RequestDelegate next, IOptions<WebSiteOptions> webSiteOptions)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.webSiteOptions = webSiteOptions ?? throw new ArgumentNullException(nameof(webSiteOptions));

            webSiteHost = this.webSiteOptions.Value.Host.ToLower();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var requestHost = request.Host.Host.ToLower();

            // Redirect by www subdomain.
            if (requestHost.StartsWith("www"))
            {
                var value = UriHelper.BuildAbsolute(scheme: "https", host: new HostString(webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);
                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", value);
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

                            context.Response.StatusCode = 301;
                            context.Response.Headers.Add("Location", $"https://{webSiteHost}");
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
                var value = UriHelper.BuildAbsolute(scheme: "https", host: request.Host, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", value);
                return;
            }

            await next(context);
        }
    }
}