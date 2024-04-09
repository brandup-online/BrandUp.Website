using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Website.Identity
{
    public class HttpAccessProvider(IHttpContextAccessor httpContextAccessor) : IAccessProvider
    {
        public Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = httpContextAccessor.HttpContext;
            return Task.FromResult(IsAuthenticated(httpContext));
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (!IsAuthenticated(httpContext))
                return Task.FromResult<string>(null);
            return Task.FromResult(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private static bool IsAuthenticated(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            return httpContext.User != null && httpContext.User.Identity.IsAuthenticated;
        }
    }
}