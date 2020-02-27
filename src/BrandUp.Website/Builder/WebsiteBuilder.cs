using BrandUp.Website.Identiry;
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
        }
    }

    public interface IWebsiteBuilder
    {
        IServiceCollection Services { get; }
    }
}