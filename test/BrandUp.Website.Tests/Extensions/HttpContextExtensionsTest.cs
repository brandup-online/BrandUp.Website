using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Website
{
    public class HttpContextExtensionsTest
    {
        [Fact]
        public void RedirectPage_FullRequest_SetsRedirectStatusAndLocation()
        {
            var context = new DefaultHttpContext();

            context.Response.RedirectPage("/contacts");

            Assert.Equal(302, context.Response.StatusCode);
            Assert.Equal("/contacts", context.Response.Headers.Location);
        }

        [Fact]
        public void RedirectPage_FullRequest_Permanent_Sets308()
        {
            var context = new DefaultHttpContext();

            context.Response.RedirectPage("/contacts", isPermanent: true);

            Assert.Equal(308, context.Response.StatusCode);
        }

        [Fact]
        public void RedirectPage_NavRequest_Sets200AndPageLocation()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[PageConstants.HttpHeaderPageNav] = "true";

            context.Response.RedirectPage("/contacts");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("/contacts", context.Response.Headers[PageConstants.HttpHeaderPageLocation]);
            Assert.False(context.Response.Headers.ContainsKey(PageConstants.HttpHeaderPageReplace));
            Assert.False(context.Response.Headers.ContainsKey(PageConstants.HttpHeaderPageReload));
        }

        [Fact]
        public void RedirectPage_NavRequest_ReplaceAndReload_SetHeaders()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[PageConstants.HttpHeaderPageNav] = "true";

            context.Response.RedirectPage("/contacts", replace: true, reload: true);

            Assert.Equal("true", context.Response.Headers[PageConstants.HttpHeaderPageReplace]);
            Assert.Equal("true", context.Response.Headers[PageConstants.HttpHeaderPageReload]);
        }

        [Fact]
        public void IsBot_DetectsBotUserAgent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.UserAgent = "Mozilla/5.0 (compatible; YandexBot/3.0)";

            Assert.True(context.Request.IsBot());
        }

        [Fact]
        public void IsBot_RegularUserAgent_False()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

            Assert.False(context.Request.IsBot());
        }
    }
}
