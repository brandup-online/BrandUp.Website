using System.Text.RegularExpressions;

namespace BrandUp.Website.Helpers
{
    public static partial class SeoHelper
    {
        [GeneratedRegex("(Google|Yahoo|Rambler|Bot|Yandex|Spider|Snoopy|Crawler|Finder|Mail|bing|Aport|WebAlta|Slurp|curl)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex SearchEngineRegex();

        public static bool IsBot(string userAgent, out SearchBotName searchBot)
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