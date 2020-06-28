using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class ApplicationModel
    {
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}