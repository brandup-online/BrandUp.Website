namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public string[] Aliases { get; set; }
        public WebsiteAdaptiveOptions Adaptive { get; set; } = new WebsiteAdaptiveOptions();

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
                throw new System.InvalidOperationException();

            if (Adaptive != null)
                Adaptive.Validate();
        }
    }

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
                throw new System.InvalidOperationException();
            if (string.IsNullOrEmpty(InitialScale))
                throw new System.InvalidOperationException();
        }
    }
}