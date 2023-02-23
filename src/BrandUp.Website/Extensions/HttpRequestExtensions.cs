using BrandUp.Website.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BrandUp.Website
{
    public static class HttpRequestExtensions
    {
        public static bool IsBot(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.Headers.TryGetValue("User-agent", out StringValues userAgent))
                return false;

            foreach (var value in userAgent)
            {
                if (SeoHelper.IsBot(value, out _))
                    return true;
            }

            return false;
        }
    }
}