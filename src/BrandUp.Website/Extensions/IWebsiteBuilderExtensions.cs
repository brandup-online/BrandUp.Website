using BrandUp.Website.Builder;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website
{
    public static class IWebsiteBuilderExtensions
    {
        public static IWebsiteBuilder AddSingleWebsite(this IWebsiteBuilder builder, string title)
        {
            builder.Services.Add(new ServiceDescriptor(typeof(IWebsiteStore), new SingleWebsiteStore(title)));
            return builder;
        }

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

        public static IWebsiteBuilder AddWebsiteStore<T>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where T : class, IWebsiteStore
        {
            builder.Services.Add(new ServiceDescriptor(typeof(IWebsiteStore), typeof(T), serviceLifetime));
            return builder;
        }

        public static IWebsiteBuilder AddVisitorStore<T>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where T : class, IVisitorStore
        {
            builder.Services.Add(new ServiceDescriptor(typeof(IVisitorStore), typeof(T), serviceLifetime));
            return builder;
        }

        public static IWebsiteBuilder AddWebsiteProvider<T>(this IWebsiteBuilder builder)
            where T : class, IWebsiteProvider
        {
            builder.Services.AddSingleton<IWebsiteProvider, T>();
            return builder;
        }

        public static IWebsiteBuilder AddAccessProvider<T>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where T : class, Identity.IAccessProvider
        {
            builder.Services.Add(new ServiceDescriptor(typeof(Identity.IAccessProvider), typeof(T), serviceLifetime));
            return builder;
        }
    }
}