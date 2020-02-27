using BrandUp.Website.Identiry;
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
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Website.Pages
{
    public abstract class AppPageModel : PageModel, IPageModel
    {
        private IPageNavigationProvider pageNavigationProvider;
        readonly static DateTime StartDate = DateTime.UtcNow;

        public AppPageRequestMode RequestMode { get; private set; } = AppPageRequestMode.Start;
        public Dictionary<string, object> NavigationState { get; } = new Dictionary<string, object>();

        public OpenGraphModel OpenGraph { get; set; }
        public void SetOpenGraph(string type, string image, string title, string url, string description = null)
        {
            OpenGraph = new OpenGraphModel(type, image, title, url, description);
        }
        public void SetOpenGraph(string image, string title, string url, string description = null)
        {
            SetOpenGraph("website", image, title, url, description);
        }
        public void SetOpenGraph(string image, string title, string description = null)
        {
            SetOpenGraph(image, title, Url.Page(null, null, null, Request.Scheme, Request.Host.Value), description);
        }
        public void SetOpenGraph(string image)
        {
            SetOpenGraph(image, Title, Url.Page("", null, null, Request.Scheme, Request.Host.Value), Description);
        }
        public void SetOpenGraph(string image, string url)
        {
            SetOpenGraph(image, Title, url, Description);
        }

        #region IPageModel members

        public abstract string Title { get; }
        public virtual string Description => null;
        public virtual string Keywords => null;
        public virtual string CssClass => null;
        public virtual string ScriptName => null;
        public virtual string CanonicalLink => null;

        #endregion

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (Request.Query.ContainsKey("_content"))
                RequestMode = AppPageRequestMode.Content;
            else if (Request.Query.ContainsKey("_nav"))
            {
                if (Request.Method != "POST")
                {
                    context.Result = BadRequest();
                    return;
                }

                RequestMode = AppPageRequestMode.Navigation;

                using var reader = new StreamReader(Request.Body);
                var navStateData = await reader.ReadToEndAsync();
                if (navStateData != null)
                {
                    var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                    var protector = protectionProvider.CreateProtector("BrandUp.Pages");
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
                            {
                                Response.Headers.Add("Page-Action", "reset");
                                context.Result = new OkResult();
                            }
                        }
                    }
                }
            }
            else
            {
                NavigationState.Add("_start", StartDate.Ticks);
            }

            await OnInitializeAsync(context);

            pageNavigationProvider = HttpContext.RequestServices.GetService<IPageNavigationProvider>();
            if (pageNavigationProvider != null)
                await pageNavigationProvider.InitializeAsync(HttpContext.RequestAborted);

            if (RequestMode == AppPageRequestMode.Navigation)
            {
                var navModel = await GetNavigationClientModelAsync();

                context.Result = new OkObjectResult(navModel);
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public void RenderPage(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (RequestMode == AppPageRequestMode.Content)
                page.Layout = null;
        }
        internal async Task<Models.AppClientModel> GetAppClientModelAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;

            var appClientModel = new Models.AppClientModel
            {
                BaseUrl = httpRequest.PathBase.HasValue ? httpRequest.PathBase.Value : "/",
                Data = new Dictionary<string, object>()
            };

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryToken = antiforgery.GetAndStoreTokens(httpContext);

                appClientModel.Antiforgery = new Models.AntiforgeryModel
                {
                    HeaderName = antiforgeryToken.HeaderName,
                    FormFieldName = antiforgeryToken.FormFieldName
                };
            }

            appClientModel.Nav = await GetNavigationClientModelAsync(cancellationToken);

            await OnBuildApplicationClientDataAsync(appClientModel.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildApplicationClientDataAsync(appClientModel.Data, cancellationToken);

            return appClientModel;
        }
        private async Task<Models.NavigationClientModel> GetNavigationClientModelAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var requestUrl = httpRequest.GetDisplayUrl();
            var requestUri = new Uri(requestUrl);
            var baseUri = requestUri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

            if (httpRequest.Query.ContainsKey("_nav"))
            {
                var query = QueryHelpers.ParseQuery(requestUri.Query);
                query.Remove("_nav");
                var qb = new QueryBuilder();
                foreach (var kv in query)
                    qb.Add(kv.Key, (IEnumerable<string>)kv.Value);

                requestUrl = baseUri + qb.ToQueryString();
            }

            // Временный код, для совместимости.
            if (httpRequest.Query.ContainsKey("handler") && httpRequest.Query["handler"] == "navigation")
            {
                var query = QueryHelpers.ParseQuery(requestUri.Query);
                query.Remove("_nav");
                var qb = new QueryBuilder();
                foreach (var kv in query)
                    qb.Add(kv.Key, (IEnumerable<string>)kv.Value);

                requestUrl = baseUri + qb.ToQueryString();
            }

            var protectionProvider = HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var protector = protectionProvider.CreateProtector("BrandUp.Pages");

            var navModel = new Models.NavigationClientModel
            {
                IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                Url = requestUrl,
                Path = requestUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped),
                Query = new Dictionary<string, object>(),
                Data = new Dictionary<string, object>(),
                State = protector.Protect(JsonSerializer.Serialize(NavigationState))
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

            var accessProvider = httpContext.RequestServices.GetRequiredService<IAccessProvider>();
            navModel.EnableAdministration = await accessProvider.CheckAccessAsync(cancellationToken);

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryTokenSet = antiforgery.GetTokens(httpContext);
                navModel.ValidationToken = antiforgeryTokenSet.RequestToken;
            }

            await OnBuildNavigationClientDataAsync(navModel.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildNavigationClientDataAsync(navModel.Data, cancellationToken);

            navModel.Page = await GetPageClientModelAsync(cancellationToken);

            return navModel;
        }
        private async Task<Models.PageClientModel> GetPageClientModelAsync(CancellationToken cancellationToken = default)
        {
            var model = new Models.PageClientModel
            {
                Title = Title,
                CssClass = CssClass,
                ScriptName = ScriptName,
                CanonicalLink = CanonicalLink,
                Description = Description,
                Keywords = Keywords,
                Data = new Dictionary<string, object>()
            };

            var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttribute<ClientModelAttribute>();
                if (attr == null)
                    continue;

                var name = p.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;

                name = name.Substring(0, 1).ToLower() + name.Substring(1);

                var value = p.GetValue(this);
                if (value is Enum)
                    value = value.ToString();

                model.Data.Add(name, value);
            }

            await OnBuildPageClientDataAsync(model.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildPageClientDataAsync(model.Data, cancellationToken);

            return model;
        }

        protected virtual Task OnInitializeAsync(PageHandlerExecutingContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildApplicationClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildNavigationClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildPageClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public enum AppPageRequestMode
    {
        Start,
        Navigation,
        Content
    }

    public class OpenGraphModel
    {
        readonly Dictionary<string, string> items = new Dictionary<string, string>();

        public OpenGraphModel(string type, string image, string title, string url, string description = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            if (!string.IsNullOrEmpty(description))
                Description = description;
        }

        public string Type { get => Get(OpenGraphProperties.Type); set => Set(OpenGraphProperties.Type, value); }
        public string Image { get => Get(OpenGraphProperties.Image); set => Set(OpenGraphProperties.Image, value); }
        public string Title { get => Get(OpenGraphProperties.Title); set => Set(OpenGraphProperties.Title, value); }
        public string Description { get => Get(OpenGraphProperties.Description); set => Set(OpenGraphProperties.Description, value); }
        public string Url { get => Get(OpenGraphProperties.Url); set => Set(OpenGraphProperties.Url, value); }

        public string Get(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            items.TryGetValue(NormalizeName(name), out string content);
            return content;
        }
        public bool TryGet(string name, out string content)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return items.TryGetValue(NormalizeName(name), out content);
        }
        public bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return items.ContainsKey(NormalizeName(name));
        }
        public void Set(string name, string content)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            items[NormalizeName(name)] = content;
        }
        public bool Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return items.Remove(NormalizeName(name));
        }

        static string NormalizeName(string name)
        {
            return name.Trim().ToLower();
        }
    }

    public static class OpenGraphProperties
    {
        public const string Type = "type";
        public const string Title = "title";
        public const string Image = "image";
        public const string Url = "url";
        public const string Description = "description";
    }
}