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
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var feature = httpContext.Features.Get<IWebsiteFeature>();
            if (feature == null)
                throw new InvalidOperationException("Http request is not website.");

            return feature.Context;
        }

        public static void SetMinifyHtml(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var minifyHtmlFeature = httpContext.Features.Get<IMinifyHtmlFeature>();
            if (minifyHtmlFeature != null)
                minifyHtmlFeature.SetMinify();
        }

        public static bool IsBot(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.Headers.TryGetValue("User-agent", out StringValues userAgent))
                return false;

            foreach (var value in userAgent)
            {
                if (SeoHelper.IsBot(value, out _))
                    return true;
            }

            return false;
        }

        public static void RedirectPage(this HttpResponse response, string pageLocation, bool isPermament = false, bool replaceUrl = false)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (pageLocation == null)
                throw new ArgumentNullException(nameof(pageLocation));

            var pageFeature = response.HttpContext.Features.Get<IPageFeature>();
            if (pageFeature == null || pageFeature.PageModel == null || pageFeature.PageModel.RequestMode == AppPageRequestMode.Start)
            {
                response.StatusCode = (int)(isPermament ? HttpStatusCode.PermanentRedirect : HttpStatusCode.Redirect);
                response.Headers.Location = pageLocation;
            }
            else
            {
                response.StatusCode = 200;
                response.Headers[PageConstants.HttpHeaderPageLocation] = pageLocation;
                if (replaceUrl)
                    response.Headers[PageConstants.HttpHeaderPageReplace] = "true";
            }
        }
    }
}