﻿using BrandUp.Website.Identity;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website.Builder
{
    public class WebsiteBuilder : IWebsiteBuilder
    {
        public WebsiteBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            AddCoreServices(services);
        }

        public IServiceCollection Services { get; }

        private static void AddCoreServices(IServiceCollection services)
        {
            services.AddSingleton<IAccessProvider, EmptyAccessProvider>();
            services.AddSingleton<IWebsiteVersion, AssemblyWebsiteVersion>();
            services.AddTransient<ITagHelperComponent, TagHelpers.WebsiteTagHelperComponent>();

            services.AddScoped(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                return httpContextAccessor.HttpContext.GetWebsiteContext();
            });

            services.AddScoped<IWebsiteClock, DefaultWebsiteClock>();
            services.AddTransient<IWebsiteEvents, DefaultWebsiteEvents>();
            services.AddTransient<IAccessProvider, HttpAccessProvider>();
        }
    }

    public interface IWebsiteBuilder
    {
        IServiceCollection Services { get; }
    }
}