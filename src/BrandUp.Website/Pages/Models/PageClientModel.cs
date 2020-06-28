using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.Pages.Models
{
    public class PageClientModel
    {
        public string CssClass { get; set; }
        public string ScriptName { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}