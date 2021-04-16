using System;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Website
{
    public static class HttpContextExtensions
    {
        public static WebsiteContext GetWebsiteContext(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var feature = httpContext.Features.Get<IWebsiteFeature>();
            if (feature == null)
                throw new InvalidOperationException("Http request is not website.");

            return feature.Context;
        }
        //public static Visitors.IVisitor GetVisitor(this HttpContext httpContext)
        //{
        //    if (httpContext == null)
        //        throw new ArgumentNullException(nameof(httpContext));

        //    var websiteContext = GetWebsiteContext(httpContext);
        //    return websiteContext?.Visitor;
        //}
    }
}