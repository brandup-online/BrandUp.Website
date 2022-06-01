﻿using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using System;

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
    }
}