namespace BrandUp.Website
{
    public interface IWebsiteStore
    {
        Task<IWebsite> FindByIdAsync(string id);
        Task<IWebsite> FindByNameAsync(string name);
        Task<string[]> GetAliasesAsync(IWebsite website);
        Task<TimeZoneInfo> GetTimeZoneAsync(IWebsite website);
    }
}