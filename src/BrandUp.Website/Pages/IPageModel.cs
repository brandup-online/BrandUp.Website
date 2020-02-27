namespace BrandUp.Website.Pages
{
    public interface IPageModel
    {
        string Title { get; }
        string Description { get; }
        string Keywords { get; }
        string CssClass { get; }
        string ScriptName { get; }
        string CanonicalLink { get; }
    }
}