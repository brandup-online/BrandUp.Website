namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public string[] Aliases { get; set; }
        public string CookiesPrefix { get; set; }
        public string ProtectionPurpose { get; set; } = "BrandUp.Website";

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(Host)}.");

            if (string.IsNullOrEmpty(CookiesPrefix))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(CookiesPrefix)}.");
        }
    }
}