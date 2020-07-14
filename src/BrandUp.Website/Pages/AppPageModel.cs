using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Website.Pages
{
    public abstract class AppPageModel : PageModel, IPageModel
    {
        private IPageEvents pageEvents;
        private IWebsiteEvents websiteEvents;
        readonly static DateTime StartDate = DateTime.UtcNow;

        #region Properties

        public WebsiteContext WebsiteContext { get; private set; }
        public AppPageRequestMode RequestMode { get; private set; } = AppPageRequestMode.Start;
        public Dictionary<string, object> NavigationState { get; } = new Dictionary<string, object>();
        public CancellationToken CancellationToken => HttpContext.RequestAborted;
        public IServiceProvider Services => HttpContext.RequestServices;

        #endregion

        #region OpenGraph members

        public PageOpenGraph OpenGraph { get; set; }
        public void SetOpenGraph(string type, Uri image, string title, Uri url, string description = null)
        {
            OpenGraph = new PageOpenGraph(type, image, title, url, description);
        }
        public void SetOpenGraph(Uri image, string title, Uri url, string description = null)
        {
            SetOpenGraph("website", image, title, url, description);
        }
        public void SetOpenGraph(Uri image, string title, string description = null)
        {
            SetOpenGraph(image, title, Link, description);
        }
        public void SetOpenGraph(Uri image)
        {
            SetOpenGraph(image, Title, Link, Description);
        }
        public void SetOpenGraph(Uri image, Uri url)
        {
            SetOpenGraph(image, Title, url, Description);
        }

        #endregion

        #region IPageModel members

        public Uri Link { get; private set; }
        public abstract string Title { get; }
        public virtual string Description { get; }
        public virtual string Keywords { get; }
        public virtual string CssClass { get; }
        public virtual string ScriptName { get; }
        public virtual Uri CanonicalLink => Link;
        public virtual string Header => Title;

        #endregion

        #region PageModel members

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var requestQuery = httpRequest.Query;

            httpContext.Features.Set<IPageFeature>(new PageFeature { PageModel = this });

            if (httpRequest.QueryString.HasValue)
            {
                var newQuery = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
                foreach (var qv in requestQuery)
                {
                    var name = qv.Key;
                    if (name[0] == '_')
                    {
                        switch (name)
                        {
                            case "_nav":
                                RequestMode = AppPageRequestMode.Navigation;
                                break;
                            case "_content":
                                RequestMode = AppPageRequestMode.Content;
                                break;
                            default:
                                break;
                        }

                        continue;
                    }

                    newQuery.Add(name, qv.Value);
                }

                httpRequest.QueryString = Microsoft.AspNetCore.Http.QueryString.Create(newQuery);
            }

            WebsiteContext = HttpContext.RequestServices.GetRequiredService<WebsiteContext>();
            websiteEvents = HttpContext.RequestServices.GetService<IWebsiteEvents>();
            Link = new Uri(HttpContext.Request.GetDisplayUrl());

            var request = Request;
            var isGetRequest = request.Method == "GET";
            var webSiteOptions = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebsiteOptions>>();
            var webSiteHost = webSiteOptions.Value.Host;
            var cookiesPrefix = webSiteOptions.Value.CookiesPrefix;
            var isBot = request.IsBot();

            #region Navigation

            switch (RequestMode)
            {
                case AppPageRequestMode.Content:
                    {
                        if (isBot)
                        {
                            context.Result = BadRequest();
                            return;
                        }

                        break;
                    }
                case AppPageRequestMode.Navigation:
                    {
                        if (Request.Method != "POST" || isBot)
                        {
                            context.Result = BadRequest();
                            return;
                        }

                        using var reader = new StreamReader(Request.Body);
                        var navStateData = await reader.ReadToEndAsync();
                        if (navStateData != null)
                        {
                            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                            var protector = protectionProvider.CreateProtector("BrandUp.Pages");
                            try
                            {
                                var requestState = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(protector.Unprotect(navStateData));
                                if (requestState != null)
                                {
                                    foreach (var kv in requestState)
                                    {
                                        object val;
                                        switch (kv.Value.ValueKind)
                                        {
                                            case JsonValueKind.String:
                                                val = kv.Value.GetString();
                                                break;
                                            case JsonValueKind.False:
                                            case JsonValueKind.True:
                                                val = kv.Value.GetBoolean();
                                                break;
                                            case JsonValueKind.Null:
                                                val = null;
                                                break;
                                            case JsonValueKind.Number:
                                                val = kv.Value.GetInt64();
                                                break;
                                            default:
                                                throw new InvalidOperationException();
                                        }

                                        NavigationState.Add(kv.Key, val);
                                    }

                                    if (NavigationState.TryGetValue("_start", out object startValue))
                                    {
                                        if ((long)startValue != StartDate.Ticks)
                                            throw new Exception("Reset application.");
                                    }

                                    string navAreaName = null;
                                    if (NavigationState.TryGetValue("_area", out object areaNameValue))
                                        navAreaName = (string)areaNameValue;

                                    RouteData.TryGetAreaName(out string curAreaName);

                                    if (navAreaName == null || !string.Equals(navAreaName, curAreaName, StringComparison.InvariantCultureIgnoreCase))
                                        throw new InvalidOperationException("Change area by navigating.");
                                }
                            }
                            catch
                            {
                                HttpContext.Response.Headers.Add("Page-Reload", "true");
                            }
                        }

                        break;
                    }
                default:
                    {
                        NavigationState.Add("_start", StartDate.Ticks);

                        RouteData.TryGetAreaName(out string areaName);
                        if (areaName == null)
                            areaName = string.Empty;

                        NavigationState.Add("_area", areaName.ToLower());

                        break;
                    }
            }

            #endregion

            #region Visitor

            IVisitor visitor = null;
            var visitorStore = HttpContext.RequestServices.GetService<IVisitorStore>();
            if (!isBot && visitorStore != null)
            {
                var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                var visitorCookieName = $"{cookiesPrefix}_v";
                if (await accessProvider.IsAuthenticatedAsync())
                {
                    var userId = await accessProvider.GetUserIdAsync();
                    if (string.IsNullOrEmpty(userId))
                        throw new InvalidOperationException();

                    visitor = await visitorStore.FindByUserIdAsync(userId);
                    if (visitor == null)
                    {
                        visitor = await visitorStore.CreateNewAsync(WebsiteContext.Website.Id);
                        await visitorStore.SetUserAsync(visitor, userId);
                    }
                    else
                    {
                        if (string.Equals(visitor.WebsiteId, WebsiteContext.Website.Id, StringComparison.InvariantCultureIgnoreCase))
                            await visitorStore.SetWebsiteAsync(visitor, WebsiteContext.Website.Id);
                    }

                    if (request.Cookies.ContainsKey(visitorCookieName))
                    {
                        // Удаляем значение клиента в cookie, если оно есть, так как его не должно быть для авторизованного пользователя.

                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddMonths(-6),
                            Domain = webSiteHost,
                            Path = "/"
                        };

                        Response.Cookies.Delete(visitorCookieName, cookieOptions);
                    }
                }
                else
                {
                    var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                    var protector = protectionProvider.CreateProtector(webSiteOptions.Value.ProtectionPurpose);

                    if (request.Cookies.TryGetValue(visitorCookieName, out string visitorIdValue))
                    {
                        try
                        {
                            var visitorId = protector.Unprotect(visitorIdValue);
                            visitor = await visitorStore.FindByIdAsync(visitorId);
                        }
                        catch (System.Security.Cryptography.CryptographicException) { }
                    }

                    if (visitor == null)
                    {
                        if (isGetRequest || RequestMode == AppPageRequestMode.Navigation)
                        {
                            // Создаём нового посетителя, только при GET запросе.

                            visitor = await visitorStore.CreateNewAsync(WebsiteContext.Website.Id);
                            HttpContext.Items[Constants.VisitorHttpContextKeyName] = visitor.Id;

                            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                Expires = DateTimeOffset.UtcNow.AddMonths(6),
                                MaxAge = TimeSpan.FromDays(30 * 12),
                                Domain = webSiteHost,
                                Path = "/"
                            };

                            Response.Cookies.Append(visitorCookieName, protector.Protect(visitor.Id), cookieOptions);
                        }
                        else
                        {
                            context.Result = BadRequest();
                            return;
                        }
                    }
                }

                if (isGetRequest)
                    await visitorStore.UpdateLastVisitDateAsync(visitor, DateTime.UtcNow);
            }

            #endregion

            WebsiteContext.Visitor = visitor;

            #region Website

            if (visitor != null)
            {
                var website = WebsiteContext.Website;
                var websiteStore = HttpContext.RequestServices.GetRequiredService<IWebsiteStore>();
                var cityCookieName = $"{cookiesPrefix}_w";
                if (string.IsNullOrEmpty(visitor.WebsiteId))
                {
                    // Если для посетителя еще не задан город.
                    if (!isGetRequest)
                    {
                        context.Result = BadRequest();
                        return;
                    }

                    if (request.Cookies.TryGetValue(cityCookieName, out string cookieWebsiteId) && cookieWebsiteId != website.Id)
                    {
                        var cookieWebsite = await websiteStore.FindByIdAsync(cookieWebsiteId);
                        if (cookieWebsite != null)
                        {
                            await visitorStore.SetWebsiteAsync(visitor, cookieWebsite.Id);
                            var redirectUrl = string.Concat(request.Scheme, "://", !string.IsNullOrEmpty(cookieWebsite.Name) ? cookieWebsite.Name + "." : "", webSiteHost, request.PathBase.ToUriComponent(), request.Path.ToUriComponent());
                            if (RequestMode == AppPageRequestMode.Navigation)
                            {
                                var query = QueryHelpers.ParseQuery(request.QueryString.ToUriComponent());
                                query.Remove("_nav");
                                var qb = new QueryBuilder();
                                foreach (var kv in query)
                                    qb.Add(kv.Key, (IEnumerable<string>)kv.Value);

                                redirectUrl += qb.ToQueryString();

                                Response.Headers.Add("Page-Location", redirectUrl);
                                context.Result = new OkResult();
                                return;
                            }
                            else
                            {
                                if (request.QueryString.HasValue)
                                    redirectUrl += request.QueryString.ToUriComponent();

                                context.Result = Redirect(redirectUrl);
                            }

                            return;
                        }

                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            Domain = webSiteHost,
                            Path = "/"
                        };
                        Response.Cookies.Delete(cityCookieName, cookieOptions);
                    }

                    await visitorStore.SetWebsiteAsync(visitor, website.Id);
                }
                else
                {
                    if (website.Id != visitor.WebsiteId)
                    {
                        // Если сайт посетителя не совпадает с сайтом домена в запросе, то редирект.

                        var visitorWebsite = await websiteStore.FindByIdAsync(visitor.WebsiteId);
                        var redirectUrl = string.Concat(request.Scheme, "://", !string.IsNullOrEmpty(visitorWebsite.Name) ? visitorWebsite.Name + "." : "", webSiteHost, request.PathBase.ToUriComponent(), request.Path.ToUriComponent());

                        if (RequestMode == AppPageRequestMode.Navigation)
                        {
                            var query = QueryHelpers.ParseQuery(request.QueryString.ToUriComponent());
                            query.Remove("_nav");
                            var qb = new QueryBuilder();
                            foreach (var kv in query)
                                qb.Add(kv.Key, (IEnumerable<string>)kv.Value);

                            redirectUrl += qb.ToQueryString();

                            Response.Headers.Add("Page-Location", redirectUrl);
                            context.Result = new OkResult();
                        }
                        else if (isGetRequest)
                        {
                            if (request.QueryString.HasValue)
                                redirectUrl += request.QueryString.ToUriComponent();

                            context.Result = Redirect(redirectUrl);
                        }
                        else
                            context.Result = BadRequest();

                        return;
                    }
                }
            }

            #endregion

            var pageRequestContext = new PageRequestContext(this);
            await OnPageRequestAsync(pageRequestContext);

            pageEvents = HttpContext.RequestServices.GetService<IPageEvents>();
            if (pageEvents != null)
                await pageEvents.PageRequestAsync(pageRequestContext);

            if (pageRequestContext.Result != null)
            {
                context.Result = pageRequestContext.Result;
                return;
            }

            if (RequestMode == AppPageRequestMode.Navigation)
            {
                var navModel = await GetNavigationClientModelAsync();

                context.Result = new OkObjectResult(navModel);
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);

            var pageRenderContext = new PageRenderContext(this);
            await OnPageRenderAsync(pageRenderContext);
            if (pageEvents != null)
                await pageEvents.PageRenderAsync(pageRenderContext);
        }

        #endregion

        public void RenderPage(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (RequestMode == AppPageRequestMode.Content)
                page.Layout = null;
        }
        internal async Task<ClientModels.StartupModel> GetStartupClientModelAsync(AppPageModel appPageModel)
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;

            var startupModel = new ClientModels.StartupModel
            {
                Env = new ClientModels.EnvironmentModel
                {
                    BasePath = httpRequest.PathBase.HasValue ? httpRequest.PathBase.Value : "/"
                },
                Model = new ClientModels.ApplicationModel
                {
                    WebsiteId = WebsiteContext.Website.Id,
                    VisitorId = WebsiteContext.Visitor?.Id,
                    Data = new Dictionary<string, object>()
                }
            };

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryToken = antiforgery.GetAndStoreTokens(httpContext);

                startupModel.Antiforgery = new ClientModels.AntiforgeryModel
                {
                    HeaderName = antiforgeryToken.HeaderName,
                    FormFieldName = antiforgeryToken.FormFieldName
                };
            }

            var startContext = new StartWebsiteContext(appPageModel, startupModel.Model.Data);
            await websiteEvents.StartAsync(startContext).ConfigureAwait(false);

            startupModel.Nav = await appPageModel.GetNavigationClientModelAsync();

            return startupModel;
        }
        internal async Task<ClientModels.NavigationModel> GetNavigationClientModelAsync()
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var protector = protectionProvider.CreateProtector("BrandUp.Pages");

            var renderTitleContext = new RenderPageTitleContext(this);
            await websiteEvents.RenderPageTitle(renderTitleContext).ConfigureAwait(false);

            var navModel = new ClientModels.NavigationModel
            {
                IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                Url = Link,
                Path = Link.GetComponents(UriComponents.Path, UriFormat.UriEscaped),
                Query = new Dictionary<string, object>(),
                Data = new Dictionary<string, object>(),
                State = protector.Protect(JsonSerializer.Serialize(NavigationState)),
                Title = renderTitleContext.Title,
                CanonicalLink = CanonicalLink,
                Description = Description,
                Keywords = Keywords,
                BodyClass = CssClass,
                OpenGraph = OpenGraph?.CreateClientModel()
            };

            foreach (var kv in httpRequest.Query)
            {
                var value = kv.Value;
                if (value.Count == 1)
                    navModel.Query.Add(kv.Key, value[0]);
                else if (value.Count > 1)
                    navModel.Query.Add(kv.Key, value.ToArray());
            }

            navModel.Query.Remove("_nav");
            navModel.Query.Remove("_content");
            navModel.Query.Remove("_");

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryTokenSet = antiforgery.GetTokens(httpContext);
                navModel.ValidationToken = antiforgeryTokenSet.RequestToken;
            }

            var pageNavigationContext = new PageNavidationContext(this, navModel.Data);
            await OnPageNavigationAsync(pageNavigationContext).ConfigureAwait(false);

            if (pageEvents != null)
                await pageEvents.PageNavigationAsync(pageNavigationContext).ConfigureAwait(false);

            navModel.Page = await GetPageClientModelAsync().ConfigureAwait(false);

            return navModel;
        }
        private async Task<ClientModels.PageModel> GetPageClientModelAsync()
        {
            var model = new ClientModels.PageModel
            {
                Type = ScriptName,
                Data = new Dictionary<string, object>()
            };

            Helpers.ClientModelHelper.CopyProperties(this, model.Data);

            var pageBuildContext = new PageBuildContext(this, model.Data);
            await OnPageBuildAsync(pageBuildContext).ConfigureAwait(false);

            if (pageEvents != null)
                await pageEvents.PageBuildAsync(pageBuildContext).ConfigureAwait(false);

            return model;
        }

        #region Page lifetime methods

        protected virtual Task OnPageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageNavigationAsync(PageNavidationContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageBuildAsync(PageBuildContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageRenderAsync(PageRenderContext context)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Result methods

        public Results.PageRedirectResult PageRedirect(string pageUrl, bool isPermament = false, bool replaceUrl = false)
        {
            if (pageUrl == null)
                throw new ArgumentNullException(nameof(pageUrl));

            return new Results.PageRedirectResult(this, pageUrl) { IsPermament = isPermament, ReplaceUrl = replaceUrl };
        }
        public Results.PageActionResult PageAction(PageActionType actionType)
        {
            return new Results.PageActionResult(this, actionType);
        }

        #endregion
    }
}