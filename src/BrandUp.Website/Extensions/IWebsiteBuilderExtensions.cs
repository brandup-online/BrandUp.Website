using BrandUp.Website.Builder;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Website
{
    public static class IWebsiteBuilderExtensions
    {
        public static IWebsiteBuilder AddSingleWebsite(this IWebsiteBuilder builder, string title)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Add(new ServiceDescriptor(typeof(IWebsiteStore), new SingleWebsiteStore(title)));
            return builder;
        }

        public static IWebsiteBuilder AddWebsiteEvents<T>(this IWebsiteBuilder builder)
            where T : class, IWebsiteEvents
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTransient<IWebsiteEvents, T>();
            return builder;
        }

        public static IWebsiteBuilder AddPageEvents<T>(this IWebsiteBuilder builder)
            where T : class, IPageEvents
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTransient<IPageEvents, T>();
            return builder;
        }

        public static IWebsiteBuilder AddWebsiteStore<TImplementation>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where TImplementation : class, IWebsiteStore
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Add(new ServiceDescriptor(typeof(IWebsiteStore), typeof(TImplementation), serviceLifetime));
            return builder;
        }

        public static IWebsiteBuilder AddWebsiteStoreFrom<TFrom>(this IWebsiteBuilder builder)
            where TFrom : class, IWebsiteStore
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Add(new ServiceDescriptor(typeof(IWebsiteStore), (s) => s.GetRequiredService<TFrom>(), ServiceLifetime.Transient));
            return builder;
        }

        public static IWebsiteBuilder AddVisitorStore<TImplementation>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where TImplementation : class, IVisitorStore
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Add(new ServiceDescriptor(typeof(IVisitorStore), typeof(TImplementation), serviceLifetime));
            return builder;
        }

        public static IWebsiteBuilder AddWebsiteProvider<T>(this IWebsiteBuilder builder)
            where T : class, IWebsiteProvider
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IWebsiteProvider, T>();
            return builder;
        }

        public static IWebsiteBuilder AddAccessProvider<T>(this IWebsiteBuilder builder, ServiceLifetime serviceLifetime)
            where T : class, Identity.IAccessProvider
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Add(new ServiceDescriptor(typeof(Identity.IAccessProvider), typeof(T), serviceLifetime));
            return builder;
        }
    }
}