using System.Security.Claims;

namespace BrandUp.Website
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetVisitorId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(Constants.VisitorKeyName);
        }
    }
}