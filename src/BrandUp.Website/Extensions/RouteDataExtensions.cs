using Microsoft.AspNetCore.Routing;

namespace BrandUp.Website
{
    public static class RouteDataExtensions
    {
        public static bool TryGetAreaName(this RouteData routeData, out string areaName)
        {
            if (routeData == null)
                throw new ArgumentNullException(nameof(routeData));

            if (routeData.Values.TryGetValue("area", out object? areaNameValue))
            {
                areaName = areaNameValue as string ?? string.Empty;
                return true;
            }
            else
            {
                areaName = string.Empty;
                return false;
            }
        }
    }
}