using System;
using System.Threading.Tasks;
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

        private readonly RequestDelegate next;
        private readonly IOptions<WebsiteOptions> webSiteOptions;
        private readonly string webSiteHost;

        public WebsiteMiddleware(RequestDelegate next, IOptions<WebsiteOptions> webSiteOptions)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.webSiteOptions = webSiteOptions ?? throw new ArgumentNullException(nameof(webSiteOptions));

            webSiteHost = this.webSiteOptions.Value.Host.ToLower();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.Request;
            var requestHost = request.Host.Host.ToLower();
            var websiteStore = context.RequestServices.GetRequiredService<IWebsiteStore>();
            var websiteProvider = context.RequestServices.GetRequiredService<IUrlMapProvider>();

            // Redirect by www subdomain.
            if (requestHost.StartsWith("www", StringComparison.InvariantCultureIgnoreCase))
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: new HostString(webSiteHost), pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", redirectUrl);
                return;
            }

            // Redirect by aliases.
            if (!requestHost.EndsWith(webSiteHost, StringComparison.InvariantCultureIgnoreCase))
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

            var needRedirectToHttps = false;
            if (request.Scheme == "http")
                needRedirectToHttps = true;

            var websiteName = websiteProvider.ExtractName(context, requestHost);
            if (websiteName == null)
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
                        var redirectHost = new HostString((!string.IsNullOrEmpty(website.Name) ? website.Name + "." : "") + webSiteHost);
                        var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: redirectHost, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                        context.Response.StatusCode = 301;
                        context.Response.Headers.Add("Location", redirectUrl);
                        return;
                    }
                }
            }

            if (needRedirectToHttps)
            {
                var redirectUrl = UriHelper.BuildAbsolute(scheme: "https", host: request.Host, pathBase: request.PathBase, path: request.Path, query: request.QueryString);

                context.Response.StatusCode = 301;
                context.Response.Headers.Add("Location", redirectUrl);
                return;
            }

            var websitemTimeZone = await websiteStore.GetTimeZoneAsync(website);
            var websiteContext = new WebsiteContext(context, website, websitemTimeZone);

            #region Visitor

            //var visitorStore = context.RequestServices.GetService<Visitors.IVisitorStore>();
            //if (visitorStore != null && !request.IsBot())
            //{
            //    var visitorProvider = context.RequestServices.GetRequiredService<Visitors.IVisitorProvider>();
            //    var visitorTicket = visitorProvider.Get();
            //    if (visitorTicket != null)
            //    {
            //        var visitor = await visitorStore.FindByIdAsync(visitorTicket.Id);
            //        if (visitor != null)
            //        {
            //            var utcNow = DateTime.UtcNow;

            //            if (request.Method == "GET")
            //                await visitorStore.UpdateLastVisitDateAsync(visitor, utcNow);

            //            websiteContext.SetVisitor(visitor);

            //            if (visitorTicket.IssuedDate <= utcNow)
            //            {
            //                visitorTicket.IssuedDate = utcNow.AddDays(1);
            //                visitorProvider.Set(visitorTicket);
            //            }
            //        }
            //    }
            //}

            #endregion

            context.Features.Set<IWebsiteFeature>(new WebsiteFeature(webSiteOptions.Value, websiteContext));

            await next(context);

            //if (context.Response.HasStarted
            //    || context.Response.StatusCode < 400
            //    || context.Response.StatusCode >= 600
            //    || context.Response.ContentLength.HasValue
            //    || !string.IsNullOrEmpty(context.Response.ContentType))
            //{
            //    return;
            //}

            //var pathFormat = "/notfound";

            //var newPath = new PathString(string.Format(CultureInfo.InvariantCulture, pathFormat, context.Response.StatusCode));

            //var originalPath = context.Request.Path;
            //var originalQueryString = context.Request.QueryString;

            //context.Request.Path = newPath;
            ////context.Request.QueryString = new QueryString();

            //try
            //{
            //    await next(context);
            //}
            //finally
            //{
            //    context.Request.Path = originalPath;
            //    //context.Request.QueryString = originalQueryString;
            //}
        }
    }
}