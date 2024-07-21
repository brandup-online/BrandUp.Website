using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class ApplicationModel
    {
        public string WebsiteId { get; set; }
        public AntiforgeryModel Antiforgery { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}