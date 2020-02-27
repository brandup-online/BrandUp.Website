using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.Pages.Models
{
    public class PageClientModel
    {
        public string Title { get; set; }
        public string CssClass { get; set; }
        public string ScriptName { get; set; }
        public string CanonicalLink { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}