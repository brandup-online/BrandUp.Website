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
        }
    }

    public interface IWebsiteBuilder
    {
        IServiceCollection Services { get; }
    }
}