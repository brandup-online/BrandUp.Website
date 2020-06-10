using System;

namespace BrandUp.Website.Pages
{
    public static class PageRequestContextExtensions
    {
        public static void PageRedirect(this PageRequestContext context, string pageUrl, bool permament = false)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (pageUrl == null)
                throw new ArgumentNullException(nameof(pageUrl));

            context.Result = context.PageModel.PageRedirect(pageUrl, permament);
        }
    }
}