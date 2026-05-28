namespace BrandUp.Website.Pages
{
    public class PageFeature : IPageFeature
    {
        public required AppPageModel PageModel { get; set; }
    }

    public interface IPageFeature
    {
        AppPageModel PageModel { get; }
    }
}