using System;

namespace BrandUp.Website.ClientModels
{
    public class OpenGraphModel
    {
        public string Type { get; set; }
        public Uri Image { get; set; }
        public string Title { get; set; }
        public Uri Url { get; set; }
        public string SiteName { get; set; }
        public string Description { get; set; }
    }
}