using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.Pages.Models
{
    public class NavigationClientModel
    {
        public bool IsAuthenticated { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public IDictionary<string, object> Query { get; set; }
        public string ValidationToken { get; set; }
        public string State { get; set; }
        public PageClientModel Page { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}