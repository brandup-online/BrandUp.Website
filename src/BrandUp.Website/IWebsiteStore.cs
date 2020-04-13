using System;
using System.Threading.Tasks;

namespace BrandUp.Website
{
    public interface IWebsiteStore
    {
        Task<IWebsite> FindWebsiteByNameAsync(string name);
        Task<string[]> GetAliasesAsync(IWebsite website);
        Task<TimeZoneInfo> GetTimeZoneAsync(IWebsite website);
    }

    public interface IWebsite
    {
        string Id { get; }
        string Name { get; }
        string Title { get; }
    }
}