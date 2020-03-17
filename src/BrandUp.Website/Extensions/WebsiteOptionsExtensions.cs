using Microsoft.Extensions.Configuration;
using System;

namespace BrandUp.Website
{
    public static class WebsiteOptionsExtensions
    {
        public static void MapConfiguration(this WebsiteOptions options, IConfiguration configuration, string key)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var section = configuration.GetSection(key);

            if (!section.Exists())
                throw new InvalidOperationException();

            section.Bind(options);
        }

        public static void MapConfiguration(this WebsiteOptions options, IConfiguration configuration)
        {
            MapConfiguration(options, configuration, "Website");
        }
    }
}