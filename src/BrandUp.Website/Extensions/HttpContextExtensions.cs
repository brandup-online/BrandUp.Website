using System.Net;
using BrandUp.Website.Helpers;
using BrandUp.Website.Infrastructure;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BrandUp.Website
{
    public static class HttpContextExtensions
    {
        public static IWebsiteContext GetWebsiteContext(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var feature = httpContext.Features.Get<IWebsiteFeature>() ?? throw new InvalidOperationException("Http request is not website.");
            return feature.Context;
        }

        public static bool IsBot(this HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!request.Headers.TryGetValue("User-agent", out StringValues userAgent))
                return false;

            foreach (var value in userAgent)
            {
                if (SeoHelper.IsBot(value, out _))
                    return true;
            }

            return false;
        }

        public static void RedirectPage(this HttpResponse response, string pageLocation, bool isPermament = false, bool replace = false, bool reload = false)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(pageLocation);

            var pageFeature = response.HttpContext.Features.Get<IPageFeature>();
            if (pageFeature == null || pageFeature.PageModel == null || pageFeature.PageModel.RequestMode == AppPageRequestMode.Full)
            {
                response.StatusCode = (int)(isPermament ? HttpStatusCode.PermanentRedirect : HttpStatusCode.Redirect);
                response.Headers.Location = pageLocation;
            }
            else
            {
                response.StatusCode = 200;
                response.Headers[PageConstants.HttpHeaderPageLocation] = pageLocation;
                if (replace)
                    response.Headers[PageConstants.HttpHeaderPageReplace] = "true";
                if (reload)
                    response.Headers[PageConstants.HttpHeaderPageReload] = "true";
            }
        }
    }
}