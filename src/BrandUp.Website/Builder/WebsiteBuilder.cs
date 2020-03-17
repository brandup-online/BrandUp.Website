using BrandUp.Website.Identiry;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            services.AddTransient(s =>
            {
                var wo = s.GetRequiredService<IOptions<WebsiteOptions>>();
                return wo.Value.Events;
            });

            services.AddScoped(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                return httpContextAccessor.HttpContext.GetWebsiteContext();
            });

            services.AddScoped<IWebsiteClock, DefaultWebsiteClock>();
        }
    }

    public interface IWebsiteBuilder
    {
        IServiceCollection Services { get; }
    }
}