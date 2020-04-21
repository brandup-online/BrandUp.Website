using BrandUp.Website.Builder;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website
{
    public static class IWebsiteBuilderExtensions
    {
        public static IWebsiteBuilder AddWebsiteEvents<T>(this IWebsiteBuilder builder)
            where T : class, IWebsiteEvents
        {
            builder.Services.AddTransient<IWebsiteEvents, T>();
            return builder;
        }

        public static IWebsiteBuilder AddPageEvents<T>(this IWebsiteBuilder builder)
            where T : class, IPageEvents
        {
            builder.Services.AddTransient<IPageEvents, T>();
            return builder;
        }
    }
}