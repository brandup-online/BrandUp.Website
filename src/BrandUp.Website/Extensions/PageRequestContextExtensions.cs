using System;

namespace BrandUp.Website.Pages
{
    public static class PageRequestContextExtensions
    {
        public static void PageRedirect(this PageRequestContext context, string pageUrl, bool isPermament = false, bool replaceUrl = false)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (pageUrl == null)
                throw new ArgumentNullException(nameof(pageUrl));

            context.Result = context.PageModel.PageRedirect(pageUrl, isPermament, replaceUrl);
        }

        public static void PageAction(this PageRequestContext context, PageActionType actionType)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.Result = context.PageModel.PageAction(actionType);
        }
    }
}