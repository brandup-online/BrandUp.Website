using BrandUp.Website.Identity;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddTransient<ITagHelperComponent, TagHelpers.EmbeddingTagHelperComponent>();

            services.AddScoped(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                return httpContextAccessor.HttpContext.GetWebsiteContext();
            });

            services.AddScoped<IWebsiteClock, DefaultWebsiteClock>();
            services.AddTransient<IWebsiteEvents, DefaultWebsiteEvents>();
        }
    }

    public interface IWebsiteBuilder
    {
        IServiceCollection Services { get; }
    }
}