namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public string[] Aliases { get; set; }
        public WebsiteAdaptiveOptions Adaptive { get; set; } = new WebsiteAdaptiveOptions();
        public string CookiesPrefix { get; set; }
        public string ProtectionPurpose { get; set; } = "BrandUp.Website";

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(WebsiteOptions.Host)}.");

            if (string.IsNullOrEmpty(CookiesPrefix))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(WebsiteOptions.CookiesPrefix)}.");

            if (Adaptive != null)
                Adaptive.Validate();
        }
    }
}