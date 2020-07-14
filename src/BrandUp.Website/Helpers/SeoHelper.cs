using System;
using System.Text.RegularExpressions;

namespace BrandUp.Website.Helpers
{
    public static class SeoHelper
    {
        const string RegexExpression = "(Google|Yahoo|Rambler|Bot|Yandex|Spider|Snoopy|Crawler|Finder|Mail|bing|Aport|WebAlta|Slurp|curl)";
        readonly static Regex searchEngineRegex = new Regex(RegexExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public static bool IsBot(string userAgent, out SearchBotName searchBot)
        {
            searchBot = SearchBotName.Unknown;

            if (string.IsNullOrEmpty(userAgent))
                return false;

            var match = searchEngineRegex.Match(userAgent);

            if (match.Success)
            {
                var searchBotName = match.Groups[1].Value;

                if (!Enum.TryParse(searchBotName, true, out searchBot))
                    throw new InvalidOperationException($"Unknown search bot name \"{searchBotName}\"");
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