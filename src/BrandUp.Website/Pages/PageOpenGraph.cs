using System;
using System.Collections.Generic;

namespace BrandUp.Website.Pages
{
    public class PageOpenGraph
    {
        readonly Dictionary<string, object> items = new Dictionary<string, object>();

        public PageOpenGraph(string type, Uri image, string title, Uri url, string description = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            if (!string.IsNullOrEmpty(description))
                Description = description;
        }

        public string Type { get => Get<string>(OpenGraphProperties.Type); set => Set(OpenGraphProperties.Type, value); }
        public Uri Image { get => Get<Uri>(OpenGraphProperties.Image); set => Set(OpenGraphProperties.Image, value); }
        public string Title { get => Get<string>(OpenGraphProperties.Title); set => Set(OpenGraphProperties.Title, value); }
        public Uri Url { get => Get<Uri>(OpenGraphProperties.Url); set => Set(OpenGraphProperties.Url, value); }
        public string SiteName { get => Get<string>(OpenGraphProperties.SiteName); set => Set(OpenGraphProperties.SiteName, value); }
        public string Description { get => Get<string>(OpenGraphProperties.Description); set => Set(OpenGraphProperties.Description, value); }

        public TValue Get<TValue>(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (items.TryGetValue(NormalizeName(name), out object content))
                return (TValue)content;
            return default;
        }
        public bool TryGet<TValue>(string name, out TValue value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (items.TryGetValue(NormalizeName(name), out object content))
            {
                value = (TValue)content;
                return true;
            }

            value = default;
            return false;
        }
        public bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return items.ContainsKey(NormalizeName(name));
        }
        public void Set<TValue>(string name, TValue content)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            items[NormalizeName(name)] = content;
        }
        public bool Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return items.Remove(NormalizeName(name));
        }

        public ClientModels.OpenGraphModel CreateClientModel()
        {
            return new ClientModels.OpenGraphModel
            {
                Type = Type,
                Image = Image,
                Title = Title,
                Url = Url,
                SiteName = SiteName,
                Description = Description
            };
        }

        static string NormalizeName(string name)
        {
            return name.Trim().ToLower();
        }
    }

    public static class OpenGraphProperties
    {
        public const string Type = "type";
        public const string Title = "title";
        public const string Image = "image";
        public const string Url = "url";
        public const string Description = "description";
        public const string SiteName = "site_name";
    }
}