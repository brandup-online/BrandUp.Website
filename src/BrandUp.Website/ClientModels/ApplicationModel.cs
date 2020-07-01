using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class ApplicationModel
    {
        public string WebsiteId { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}