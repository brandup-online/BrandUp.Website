namespace BrandUp.Website.ClientModels
{
    public class OpenGraphModel
    {
        public string Type { get; set; } = null!;
        public Uri Image { get; set; } = null!;
        public string Title { get; set; } = null!;
        public Uri Url { get; set; } = null!;
        public string? SiteName { get; set; }
        public string? Description { get; set; }
    }
}