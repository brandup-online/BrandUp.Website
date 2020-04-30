namespace BrandUp.Website
{
    public class WebsiteAdaptiveOptions
    {
        public bool Enable { get; set; } = true;
        public string Width { get; set; } = "device-width";
        public string InitialScale { get; set; } = "1";
        public string MinimumScale { get; set; }
        public string MaximumScale { get; set; }

        public void Validate()
        {
            if (Enable)
                return;

            if (string.IsNullOrEmpty(Width))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(WebsiteAdaptiveOptions.Width)}.");
            if (string.IsNullOrEmpty(InitialScale))
                throw new System.InvalidOperationException($"Не задан параметры {nameof(WebsiteAdaptiveOptions.InitialScale)}.");
        }
    }
}