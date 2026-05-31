using System.Text.Json.Serialization;

namespace BrandUp.Website.ClientModels
{
    public class NavigationModel
    {
        public bool IsAuthenticated { get; set; }
        public Uri Url { get; set; } = null!;
        public string Path { get; set; } = null!;
        public IDictionary<string, object> Query { get; set; } = null!;
        public string? ValidationToken { get; set; }
        public string State { get; set; } = null!;
        public string? Title { get; set; }
        public Uri? CanonicalLink { get; set; }
        public string? Description { get; set; }
        public string? Keywords { get; set; }
        public string? BodyClass { get; set; }
        public IDictionary<string, string>? OpenGraph { get; set; }
        public PageModel Page { get; set; } = null!;
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; } = [];
    }
}