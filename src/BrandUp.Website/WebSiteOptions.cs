namespace BrandUp.Website
{
    public class WebSiteOptions
    {
        public string Host { get; set; } = "localhost";
        public string[] Aliases { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
                throw new System.InvalidOperationException();
        }
    }
}