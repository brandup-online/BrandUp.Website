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

        public static IWebsiteBuilder AddWebsite(this IServiceCollection services, Action<WebsiteOptions> setupAction)
        {
            services.Configure(setupAction).PostConfigure<WebsiteOptions>(options => options.Validate());
            return new WebsiteBuilder(services);
        }
    }
}