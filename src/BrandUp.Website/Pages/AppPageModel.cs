using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

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

            if (RequestMode == AppPageRequestMode.Content || RequestMode == AppPageRequestMode.Start)
                HttpContext.SetMinifyHtml();

            var websiteFeature = HttpContext.Features.Get<Infrastructure.IWebsiteFeature>() ?? throw new InvalidOperationException($"Is not defined {nameof(Infrastructure.IWebsiteFeature)} in HttpContext features.");
            WebsiteContext = websiteFeature.Context;
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
                            var protector = protectionProvider.CreateProtector(webSiteOptions.Value.ProtectionPurpose);
                            try
                            {
                                var requestState = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(protector.Unprotect(navStateData));
                                if (requestState != null)
                                {
                                    foreach (var kv in requestState)
                                    {
                                        object val = kv.Value.ValueKind switch
                                        {
                                            JsonValueKind.String => kv.Value.GetString(),
                                            JsonValueKind.False or JsonValueKind.True => kv.Value.GetBoolean(),
                                            JsonValueKind.Null => null,
                                            JsonValueKind.Number => kv.Value.GetInt64(),
                                            _ => throw new InvalidOperationException(),
                                        };
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
                        areaName ??= string.Empty;

                        NavigationState.Add("_area", areaName.ToLower());

                        break;
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
        }

        #endregion

        public async Task RenderPageAsync(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (RequestMode == AppPageRequestMode.Content)
                page.Layout = null;

            var pageRenderContext = new PageRenderContext(this, page);
            await OnPageRenderAsync(pageRenderContext);

            if (pageEvents != null)
                await pageEvents.PageRenderAsync(pageRenderContext);
        }
        internal async Task<ClientModels.StartupModel> GetStartupClientModelAsync()
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

            var startContext = new StartWebsiteContext(this, startupModel.Model.Data);
            await websiteEvents.StartAsync(startContext).ConfigureAwait(false);

            startupModel.Nav = await GetNavigationClientModelAsync();

            return startupModel;
        }
        internal async Task<ClientModels.NavigationModel> GetNavigationClientModelAsync()
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var websiteFeature = HttpContext.Features.Get<Infrastructure.IWebsiteFeature>();
            var protector = protectionProvider.CreateProtector(websiteFeature.Options.ProtectionPurpose);

            var navModel = new ClientModels.NavigationModel
            {
                IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                Url = Link,
                Path = Link.GetComponents(UriComponents.Path, UriFormat.UriEscaped),
                Query = new Dictionary<string, object>(),
                Data = new Dictionary<string, object>(),
                State = protector.Protect(JsonSerializer.Serialize(NavigationState)),
                Title = Title,
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

            var pageNavigationContext = new PageClientNavidationContext(this, navModel.Data);
            await OnPageClientNavigationAsync(pageNavigationContext).ConfigureAwait(false);

            if (pageEvents != null)
                await pageEvents.PageClientNavigationAsync(pageNavigationContext).ConfigureAwait(false);

            navModel.Page = await GetPageClientModelAsync().ConfigureAwait(false);

            return navModel;
        }
        async Task<ClientModels.PageModel> GetPageClientModelAsync()
        {
            var model = new ClientModels.PageModel
            {
                Type = ScriptName,
                Data = new Dictionary<string, object>()
            };

            Helpers.ClientModelHelper.CopyProperties(this, model.Data);

            var pageBuildContext = new PageClientBuildContext(this, model.Data);
            await OnPageClientBuildAsync(pageBuildContext).ConfigureAwait(false);

            if (pageEvents != null)
                await pageEvents.PageClientBuildAsync(pageBuildContext).ConfigureAwait(false);

            return model;
        }

        #region Page lifetime methods

        protected virtual Task OnPageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageRenderAsync(PageRenderContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageClientNavigationAsync(PageClientNavidationContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnPageClientBuildAsync(PageClientBuildContext context)
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