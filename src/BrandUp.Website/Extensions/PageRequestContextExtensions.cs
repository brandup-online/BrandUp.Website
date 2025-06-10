namespace BrandUp.Website.Pages
{
    public static class PageRequestContextExtensions
    {
        public static void PageRedirect(this PageRequestContext context, string pageUrl, bool isPermament = false, bool replaceUrl = false)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(pageUrl);

            context.Result = context.PageModel.PageRedirect(pageUrl, isPermament, replaceUrl);
        }
    }
}