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

        /// <summary>
        /// Нормализует URL GET-запросов (убирает завершающий "/", приводит путь к нижнему регистру) через 301-редирект.
        /// </summary>
        /// <param name="excludedPaths">
        /// Префиксы путей, которые не нужно нормализовать (сравнение по началу пути, регистронезависимо).
        /// Например "/dist/" для статических бандлов webpack, где регистр значим.
        /// </param>
        public static IApplicationBuilder UseNormalizeUrl(this IApplicationBuilder applicationBuilder, params string[] excludedPaths)
        {
            applicationBuilder.UseMiddleware<Middlewares.NormalizeUrlMiddleware>([excludedPaths ?? []]);

            return applicationBuilder;
        }
    }
}