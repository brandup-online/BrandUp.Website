namespace BrandUp.Website
{
    public class SingleWebsiteStore : IWebsiteStore
    {
        readonly SingleWebsite website = null;

        public SingleWebsiteStore(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title is requiread and not empty.", nameof(title));

            website = new SingleWebsite(title);
        }

        public Task<IWebsite> FindByIdAsync(string id)
        {
            return Task.FromResult<IWebsite>(website.Id == id ? website : null);
        }

        public Task<IWebsite> FindByNameAsync(string name)
        {
            return Task.FromResult<IWebsite>(name == string.Empty ? website : null);
        }

        public Task<string[]> GetAliasesAsync(IWebsite website)
        {
            return Task.FromResult(Array.Empty<string>());
        }

        public Task<TimeZoneInfo> GetTimeZoneAsync(IWebsite website)
        {
            return Task.FromResult(TimeZoneInfo.Local);
        }

        class SingleWebsite(string title) : IWebsite
        {
            public string Id { get; } = "website";
            public string Name { get; } = string.Empty;
            public string Title { get; } = title ?? throw new ArgumentNullException(nameof(title));
        }
    }
}