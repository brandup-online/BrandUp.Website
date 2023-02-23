namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public List<string> Aliases { get; set; }
        public string CookiesPrefix { get; set; }
        public string ProtectionPurpose { get; set; } = "BrandUp.Website";

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
                throw new System.InvalidOperationException($"Не задан параметр {nameof(Host)}.");

            if (string.IsNullOrEmpty(CookiesPrefix))
                throw new System.InvalidOperationException($"Не задан параметр {nameof(CookiesPrefix)}.");

            if (string.IsNullOrEmpty(ProtectionPurpose))
                throw new System.InvalidOperationException($"Не задан параметр {nameof(ProtectionPurpose)}.");
        }
    }
}