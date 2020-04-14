using BrandUp.Website;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExampleWebSite
{
    public class WebsiteStore : IWebsiteStore
    {
        readonly List<CityWebsite> items = new List<CityWebsite>();
        readonly Dictionary<string, int> names = new Dictionary<string, int>();
        readonly Dictionary<string, int> ids = new Dictionary<string, int>();

        public WebsiteStore(IWebHostEnvironment hostingEnvironment)
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
                foreach (var item in serializer.Deserialize<CityWebsite[]>(jsonReader))
                    AddWebsite(item);
            }
        }

        private void AddWebsite(CityWebsite website)
        {
            if (website == null)
                throw new ArgumentNullException(nameof(website));

            var index = items.Count;
            items.Add(website);
            names.Add(website.Name.ToLower(), index);
            if (website.NameAliases != null)
            {
                foreach (var name in website.NameAliases)
                    names.Add(name.ToLower(), index);
            }
            ids.Add(website.Id.ToLower(), index);
        }

        #region IWebsiteStore members

        public Task<IWebsite> FindByIdAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            id = id.ToLower();
            if (!ids.TryGetValue(id, out int index))
                return Task.FromResult<IWebsite>(null);

            return Task.FromResult<IWebsite>(items[index]);
        }
        public Task<IWebsite> FindByNameAsync(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            name = name.ToLower();
            if (!names.TryGetValue(name, out int index))
                return Task.FromResult<IWebsite>(null);

            return Task.FromResult<IWebsite>(items[index]);
        }
        public Task<string[]> GetAliasesAsync(IWebsite website)
        {
            if (website == null)
                throw new ArgumentNullException(nameof(website));
            var w = website as CityWebsite ?? throw new ArgumentException();

            return Task.FromResult(w.NameAliases);
        }
        public Task<TimeZoneInfo> GetTimeZoneAsync(IWebsite website)
        {
            var w = website as CityWebsite ?? throw new ArgumentException();
            var timeZone = w.TimeZone ?? "Europe/Moscow";

            if (!TimeZoneConverter.TZConvert.TryIanaToWindows(timeZone, out string windowsTimeZoneId))
                return Task.FromResult<TimeZoneInfo>(null);

            return Task.FromResult(TimeZoneConverter.TZConvert.GetTimeZoneInfo(windowsTimeZoneId));
        }

        #endregion
    }

    public class CityWebsite : IWebsite
    {
        [JsonProperty("sourceId")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string[] NameAliases { get; set; }
        public string TitleWhere { get; set; }
        public string TitleWherePre { get; set; }
        public string TimeZone { get; set; }

        public CityWebsite(string id, string name, string title, string timeZone)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }
    }
}