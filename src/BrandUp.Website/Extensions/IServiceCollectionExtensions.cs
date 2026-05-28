using BrandUp.Website.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
            services.AddOptions<WebsiteOptions>()
                .Configure(setupAction)
                .ValidateOnStart();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WebsiteOptions>, WebsiteOptionsValidator>());

            return new WebsiteBuilder(services);
        }
    }
}