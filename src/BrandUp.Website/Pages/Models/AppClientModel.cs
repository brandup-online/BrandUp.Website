using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Website.Pages.Models
{
    public class AppClientModel
    {
        public string BaseUrl { get; set; }
        public AntiforgeryModel Antiforgery { get; set; }
        public NavigationClientModel Nav { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}