using System;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.Visitors
{
    public class CookieVisitorProvider : IVisitorProvider
    {
        readonly HttpContext httpContext;
        readonly IDataProtectionProvider dataProtectionProvider;
        readonly WebsiteOptions webSiteOptions;
        readonly static JsonSerializerOptions jsonOptions;

        static CookieVisitorProvider()
        {
            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };
        }
        public CookieVisitorProvider(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider, IOptions<WebsiteOptions> webSiteOptions)
        {
            httpContext = httpContextAccessor?.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.dataProtectionProvider = dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
            this.webSiteOptions = webSiteOptions?.Value ?? throw new ArgumentNullException(nameof(webSiteOptions));
        }

        public VisitorTicket Get()
        {
            var visitorCookieName = $"{webSiteOptions.CookiesPrefix}_v";
            if (!httpContext.Request.Cookies.TryGetValue(visitorCookieName, out string visitorIdValue))
                return null;

            var protector = dataProtectionProvider.CreateProtector(webSiteOptions.ProtectionPurpose);

            try
            {
                return JsonSerializer.Deserialize<VisitorTicket>(protector.Unprotect(visitorIdValue), jsonOptions);
            }
            catch (System.Security.Cryptography.CryptographicException) { }
            catch (JsonException) { }

            return null;
        }
        public void Set(VisitorTicket visitorTicket)
        {
            var visitorCookieName = $"{webSiteOptions.CookiesPrefix}_v";
            var protector = dataProtectionProvider.CreateProtector(webSiteOptions.ProtectionPurpose);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddMonths(12),
                MaxAge = TimeSpan.FromDays(30 * 24),
                Domain = webSiteOptions.Host,
                Path = "/"
            };

            httpContext.Response.Cookies.Append(visitorCookieName, protector.Protect(JsonSerializer.Serialize(visitorTicket, jsonOptions)), cookieOptions);
        }
    }

    public interface IVisitorProvider
    {
        VisitorTicket Get();
        void Set(VisitorTicket visitorTicket);
    }

    public class VisitorTicket
    {
        public string Id { get; set; }
        public DateTime IssuedDate { get; set; }
    }
}