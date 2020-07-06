namespace BrandUp.Website.Pages
{
    public class PageFeature : IPageFeature
    {
        public AppPageModel PageModel { get; set; }
    }

    public interface IPageFeature
    {
        AppPageModel PageModel { get; }
    }
}