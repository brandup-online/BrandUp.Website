namespace BrandUp.Website.Infrastructure
{
    public interface IMinifyHtmlFeature
    {
        bool AllowMinify { get; }
        void SetMinify();
    }
}