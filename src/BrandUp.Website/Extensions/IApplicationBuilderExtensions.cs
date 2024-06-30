using Microsoft.AspNetCore.Builder;

namespace BrandUp.Website
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebsite(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<Middlewares.WebsiteMiddleware>();

            return applicationBuilder;
        }
    }
}