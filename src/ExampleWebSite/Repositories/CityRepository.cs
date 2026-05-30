using BrandUp.Website;
using Newtonsoft.Json;

namespace ExampleWebSite.Repositories
{
    public class CityRepository : IWebsiteStore
    {
        readonly List<City> items = new List<City>();
        readonly Dictionary<string, int> names = new Dictionary<string, int>();
        readonly Dictionary<string, int> ids = new Dictionary<string, int>();

        public CityRepository(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));

            var serializer = JsonSerializer.Create();

            var dataFile = hostingEnvironment.ContentRootFileProvider.GetFileInfo("_data/cities.json");
            if (dataFile.Exists)
            {
                using var dataFileStream = dataFile.CreateReadStream();
                using var streamReader = new StreamReader(dataFileStream);
                using var jsonReader = new JsonTextReader(streamReader);
                foreach (var item in serializer.Deserialize<City[]>(jsonReader))
                    AddWebsite(item);
            }
        }

        private void AddWebsite(City website)
        {
            if (website == null)
                throw new ArgumentNullException(nameof(website));

            var index = items.Count;
            items.Add(website);
            names.Add(NormalizeIdentifier(website.Name), index);
            if (website.NameAliases != null)
            {
                foreach (var name in website.NameAliases)
                    names.Add(NormalizeIdentifier(name), index);
            }
            ids.Add(NormalizeIdentifier(website.Id), index);
        }
        private string NormalizeIdentifier(string value)
        {
            return value.Trim().ToLower();
        }

        public IQueryable<City> Cities => items.AsQueryable();

        #region IWebsiteStore members

        Task<IWebsite> IWebsiteStore.FindByIdAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (!ids.TryGetValue(NormalizeIdentifier(id), out int index))
                return Task.FromResult<IWebsite>(null);

            return Task.FromResult<IWebsite>(items[index]);
        }
        Task<IWebsite> IWebsiteStore.FindByNameAsync(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!names.TryGetValue(NormalizeIdentifier(name), out int index))
                return Task.FromResult<IWebsite>(null);

            return Task.FromResult<IWebsite>(items[index]);
        }
        Task<string[]> IWebsiteStore.GetAliasesAsync(IWebsite website)
        {
            if (website == null)
                throw new ArgumentNullException(nameof(website));
            var w = website as City ?? throw new ArgumentException();

            return Task.FromResult(w.NameAliases);
        }
        Task<TimeZoneInfo> IWebsiteStore.GetTimeZoneAsync(IWebsite website)
        {
            var w = website as City ?? throw new ArgumentException();
            var timeZone = w.TimeZone ?? "Europe/Moscow";

            if (!TimeZoneConverter.TZConvert.TryIanaToWindows(timeZone, out string windowsTimeZoneId))
                return Task.FromResult<TimeZoneInfo>(null);

            return Task.FromResult(TimeZoneConverter.TZConvert.GetTimeZoneInfo(windowsTimeZoneId));
        }

        #endregion
    }

    public class City : IWebsite
    {
        [JsonProperty("sourceId")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string[] NameAliases { get; set; }
        public string TitleWhere { get; set; }
        public string TitleWherePre { get; set; }
        public string TimeZone { get; set; }

        public City(string id, string name, string title, string timeZone)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }
    }
}