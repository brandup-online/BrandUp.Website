using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class ApplicationModel
    {
        public string WebsiteId { get; set; } = null!;
        public AntiforgeryModel? Antiforgery { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; } = [];
    }
}