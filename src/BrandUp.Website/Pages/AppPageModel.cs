﻿using System.Text.Json;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Website.Pages
{
    public abstract class AppPageModel : PageModel, IPageModel
    {
        IWebsiteEvents websiteEvents;
        IPageEvents pageEvents;

        public const string NavStateKey_Version = "_version";
        public const string NavStateKey_Area = "_area";

        #region Properties

        public WebsiteContext WebsiteContext { get; private set; }
        public AppPageRequestMode RequestMode { get; private set; } = AppPageRequestMode.Full;
        public Dictionary<string, object> NavigationState { get; } = [];
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
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;

            httpContext.Features.Set<IPageFeature>(new PageFeature { PageModel = this });

            if (httpRequest.Headers.TryGetValue(PageConstants.HttpHeaderPageNav, out string navigationData))
                RequestMode = AppPageRequestMode.Content;

            var websiteFeature = HttpContext.Features.Get<IWebsiteFeature>() ?? throw new InvalidOperationException($"Is not defined {nameof(IWebsiteFeature)} in HttpContext features.");
            WebsiteContext = websiteFeature.Context;
            websiteEvents = HttpContext.RequestServices.GetService<IWebsiteEvents>();
            pageEvents = HttpContext.RequestServices.GetService<IPageEvents>();
            Link = new Uri(HttpContext.Request.GetDisplayUrl());

            var request = Request;
            var webSiteOptions = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebsiteOptions>>();
            var websiteVersion = HttpContext.RequestServices.GetRequiredService<IWebsiteVersion>();

            #region Navigation

            switch (RequestMode)
            {
                case AppPageRequestMode.Content:
                    {
                        if (request.IsBot())
                        {
                            context.Result = BadRequest();
                            return;
                        }

                        var needReloadPage = false;

                        if (navigationData != null)
                        {
                            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                            var protector = protectionProvider.CreateProtector(webSiteOptions.Value.ProtectionPurpose);

                            try
                            {
                                var requestState = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(protector.Unprotect(navigationData));
                                if (requestState != null)
                                {
                                    foreach (var kv in requestState)
                                    {
                                        object value = kv.Value.ValueKind switch
                                        {
                                            JsonValueKind.String => kv.Value.GetString(),
                                            JsonValueKind.False or JsonValueKind.True => kv.Value.GetBoolean(),
                                            JsonValueKind.Null => null,
                                            JsonValueKind.Number => kv.Value.GetInt64(),
                                            _ => throw new InvalidOperationException("Unknown nav state value type."),
                                        };
                                        NavigationState.Add(kv.Key, value);
                                    }

                                    if (!NavigationState.TryGetValue(NavStateKey_Version, out object versionValue) || versionValue == null || !Version.TryParse(versionValue.ToString(), out var navVersion) || navVersion != websiteVersion.GetVersion())
                                        needReloadPage = true;

                                    if (!needReloadPage)
                                    {
                                        string navAreaName = null;
                                        if (NavigationState.TryGetValue(NavStateKey_Area, out object areaNameValue))
                                            navAreaName = (string)areaNameValue;

                                        RouteData.TryGetAreaName(out string curAreaName);
                                        if (navAreaName == null || !string.Equals(navAreaName, curAreaName, StringComparison.InvariantCultureIgnoreCase))
                                            needReloadPage = true; // Если изменилась area, то перезагружаем страницу
                                    }
                                }
                            }
                            catch { needReloadPage = true; }
                        }
                        else
                            needReloadPage = true;

                        if (needReloadPage)
                            HttpContext.Response.Headers[PageConstants.HttpHeaderPageReload] = "true";

                        break;
                    }
                default:
                    {
                        NavigationState.Add(NavStateKey_Version, websiteVersion.GetVersion().ToString());

                        RouteData.TryGetAreaName(out string areaName);
                        NavigationState.Add(NavStateKey_Area, areaName?.ToLower() ?? string.Empty);

                        break;
                    }
            }

            #endregion

            var pageRequestContext = new PageRequestContext(this);
            await OnPageRequestAsync(pageRequestContext);

            if (pageEvents != null)
                await pageEvents.PageRequestAsync(pageRequestContext);

            if (pageRequestContext.Result != null)
            {
                context.Result = pageRequestContext.Result;
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        #endregion

        internal async Task RaiseRenderPageAsync(PageRenderContext renderContext)
        {
            if (pageEvents != null)
                await pageEvents.PageRenderAsync(renderContext);

            await OnPageRenderAsync(renderContext);
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
                    Data = []
                }
            };

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryToken = antiforgery.GetAndStoreTokens(httpContext);

                startupModel.Model.Antiforgery = new ClientModels.AntiforgeryModel
                {
                    HeaderName = antiforgeryToken.HeaderName,
                    FormFieldName = antiforgeryToken.FormFieldName
                };
            }

            var startContext = new StartWebsiteContext(this, startupModel.Model.Data);
            await websiteEvents.StartAsync(startContext).ConfigureAwait(false);

            return startupModel;
        }

        internal async Task<ClientModels.NavigationModel> GetNavigationClientModelAsync()
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var websiteFeature = HttpContext.Features.Get<IWebsiteFeature>();
            var protector = protectionProvider.CreateProtector(websiteFeature.Options.ProtectionPurpose);

            var navModel = new ClientModels.NavigationModel
            {
                IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                Url = Link,
                Path = Link.GetComponents(UriComponents.Path, UriFormat.UriEscaped),
                Query = new Dictionary<string, object>(),
                Data = [],
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
                Data = []
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

        public Results.PageRedirectResult PageRedirect(string pageUrl, bool isPermament = false, bool replace = false, bool reload = false)
        {
            ArgumentNullException.ThrowIfNull(pageUrl);

            return new Results.PageRedirectResult(pageUrl) { IsPermament = isPermament, Replace = replace, Reload = reload };
        }

        public Results.PageRedirectResult PageRedirect(Uri pageUrl, bool isPermament = false, bool replace = false, bool reload = false)
        {
            return PageRedirect(pageUrl.ToString(), isPermament, replace, reload);
        }
    }
}