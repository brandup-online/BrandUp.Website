using System.Text.RegularExpressions;

namespace BrandUp.Website.Helpers
{
    public static partial class SeoHelper
    {
        [GeneratedRegex("(Google|Yahoo|Rambler|Bot|Yandex|Spider|Snoopy|Crawler|Finder|Mail|bing|Aport|WebAlta|Slurp|curl)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex SearchEngineRegex();

        public static bool IsBot(string? userAgent, out SearchBotName searchBot)
        {
            searchBot = SearchBotName.Unknown;

            if (string.IsNullOrEmpty(userAgent))
                return false;

            var match = SearchEngineRegex().Match(userAgent);

            if (match.Success)
            {
                var searchBotName = match.Groups[1].Value;

                if (!Enum.TryParse(searchBotName, true, out searchBot))
                    searchBot = SearchBotName.Unknown;
            }

            return match.Success;
        }

        /// <summary>
        /// Адрес без перечисленных параметров query. Остальной запрос сохраняется в исходном порядке
        /// и кодировании; если убирать нечего, возвращается тот же экземпляр.
        /// </summary>
        public static Uri RemoveQueryParameters(Uri url, IEnumerable<string> names)
        {
            ArgumentNullException.ThrowIfNull(url);
            ArgumentNullException.ThrowIfNull(names);

            if (url.Query.Length <= 1)
                return url;

            var kept = new List<string>();
            var removed = false;

            foreach (var pair in url.Query[1..].Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                if (names.Contains(QueryParameterName(pair), StringComparer.Ordinal))
                    removed = true;
                else
                    kept.Add(pair);
            }

            if (!removed)
                return url;

            return new UriBuilder(url) { Query = string.Join('&', kept) }.Uri;
        }

        static string QueryParameterName(string pair)
        {
            var separator = pair.IndexOf('=');

            return Uri.UnescapeDataString(separator < 0 ? pair : pair[..separator]);
        }
    }

    public enum SearchBotName
    {
        Unknown,
        Google,
        Yahoo,
        Rambler,
        Bot,
        Yandex,
        Spider,
        Snoopy,
        Crawler,
        Finder,
        Mail,
        Bing,
        Aport,
        WebAlta,
        Slurp,
        Curl
    }
}