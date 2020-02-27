using BrandUp.Website.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Website
{
    public static class IServiceCollectionExtensions
    {
        public static IWebsiteBuilder AddWebsite(this IServiceCollection services)
        {
            return AddWebsite(services, options => { });
        }

        public static IWebsiteBuilder AddWebsite(this IServiceCollection services, Action<WebSiteOptions> setupAction)
        {
            services.Configure(setupAction);
            return new WebsiteBuilder(services);
        }
    }
}