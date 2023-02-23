using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class PageModel
    {
        public string Type { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}