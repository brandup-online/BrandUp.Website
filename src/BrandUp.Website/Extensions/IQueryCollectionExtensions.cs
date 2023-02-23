using Microsoft.AspNetCore.Http;

namespace BrandUp.Website
{
    public static class IQueryCollectionExtensions
    {
        public static bool TryGetValue(this IQueryCollection collection, string name, out string value)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (!collection.TryGetValue(name, out Microsoft.Extensions.Primitives.StringValues values))
            {
                value = default;
                return false;
            }

            value = values[0];
            return true;
        }
    }
}