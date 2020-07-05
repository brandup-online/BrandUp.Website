using System;

namespace BrandUp.Website.Pages
{
    public interface IPageModel
    {
        Uri Link { get; }
        string Title { get; }
        string Description { get; }
        string Keywords { get; }
        string CssClass { get; }
        string ScriptName { get; }
        Uri CanonicalLink { get; }
        string Header { get; }
    }
}