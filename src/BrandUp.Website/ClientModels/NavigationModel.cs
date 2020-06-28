using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class NavigationModel
    {
        public bool IsAuthenticated { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public IDictionary<string, object> Query { get; set; }
        public string ValidationToken { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public string CanonicalLink { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string BodyClass { get; set; }
        public PageModel Page { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}