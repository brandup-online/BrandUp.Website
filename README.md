# BrandUp.Website

Базовый инфраструктурный фреймворк для Web-сайтов на ASP.NET Razor Pages.

[![Build Status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status%2FBrandUp%2Fbrandup-website?branchName=master)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=58&branchName=master)

## Installation

Install NuGet package: [https://www.nuget.org/packages/BrandUp.Website/](https://www.nuget.org/packages/BrandUp.Website/)

Install NPM package [@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website).

## Конфигурация

```
{
  "AllowedHosts": "*",
  "Website": {
    "Host": "localhost",
    "Aliases": [ "test.ru" ],
    "CookiesPrefix": "ex",
    "ProtectionPurpose": "148c26c8-0267-4677-b2f5-dda1256d5947",
    "RedirectToHttps": true
  }
}
```

**Website:Host** - доменное имя сайта.

**Website:Aliases** - список алиасов, с которых будет выполняться автоматический редирект на домен Host.

**Website:CookiesPrefix** - префикс ключей Cookies.

**Website:ProtectionPurpose** - The purpose to be assigned to the newly-created Microsoft.AspNetCore.DataProtection.IDataProtector.

**Website:RedirectToHttps** - выполнять ли автоматический редирект с `http` на `https` (по умолчанию `true`).

Параметры валидируются при старте приложения (fail-fast): если не задан `Host`, `CookiesPrefix` или `ProtectionPurpose`, хост не запустится.

Использование параметров сайта:
```
var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebsiteOptions>>()
```

Регистрация сервисов:

```

services
    .AddWebsite(options =>
    {
        options.MapConfiguration(Configuration);
    })
    .AddUrlMapProvider<BrandUp.Website.Infrastructure.SubdomainUrlMapProvider>()
    .AddSingleWebsite("website title");

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRequestLocalization();
    app.UseWebsite();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapRazorPages();
    });
}

```

### Контекст сайта

Контекст сайта `IWebsiteContext` создаётся для каждого входящего запроса. Доступен через `IServiceProvider`.

Получение контекста сайта для текущего `HttpContext`:

```
var webSiteContext = HttpContext.GetWebsiteContext(); 
```

## Представления и модели

### _Layout

```

@model AppPageModel
<!DOCTYPE html>
<html lang="en" prefix="og: http://ogp.me/ns#">
<head>
    <meta content="en" http-equiv="content-language">
    <meta content="IE=edge" http-equiv="X-UA-Compatible">
    <meta content="width=device-width, initial-scale=1, maximum-scale=2" name="viewport">
    <link type="text/css" rel="stylesheet" href="~/dist/app.css" asp-append-version="true" async>
    <script type="text/javascript" src="~/dist/app.js" asp-append-version="true" async></script>
</head>
<body>
    <div class="app">
        @RenderBody()
    </div>
</body>
</html>

```

### Page view

В представлении страницы необходимо взять весь контент в тег `<page></page>`.

```
@page
@model IndexModel

<page>

    <header class="page-header">
        <h1>@Model.Header</h1>
    </header>

    <section>
        <p><a href="#test1">test1</a></p>
        <p><a href="#test2">test2</a></p>
        <p><a nav-url="/company">hotfound</a></p>
    </section>

</page>
```

### Page model

Базовым классом модели страницы является класс `AppPageModel`.

```
public class IndexModel : AppPageModel
{
    public override string Title => "Main";
    public override string Description => Title;
    public override string Keywords => Title;
    public override string Header => Title;
    public override string ScriptName => "page script name";
    public override string CssClass => "page css class";

    /// <summary>
    /// Выполняется всегда при запросе страницы.
    /// </summary>
    protected override Task OnPageRequestAsync(PageRequestContext context)
    {
        return base.OnPageRequestAsync(context);
    }

    /// <summary>
    /// Выполняется перед рендерингом представления страницы.
    /// </summary>
    protected override Task OnPageRenderAsync(PageRenderContext context)
    {
        return base.OnPageRenderAsync(context);
    }

    /// <summary>
    /// Выполняется при конструировании контекста сайта для клиента.
    /// </summary>
    protected override Task OnPageClientBuildAsync(PageClientBuildContext context)
    {
        return base.OnPageClientBuildAsync(context);
    }

    /// <summary>
    /// Выполняется при конструировании контекста навигации для клиента.
    /// </summary>
    protected override Task OnPageClientNavigationAsync(PageClientNavigationContext context)
    {
        return base.OnPageClientNavigationAsync(context);
    }
}

@page
@model IndexModel

<page>

    <h1>@Model.Header</h1>

    <div>
        @{
            var webSiteContext = HttpContext.GetWebsiteContext(); 
        }
        <p>WebSite Id: @webSiteContext.Website.Id</p>
        <p>WebSite Name: @webSiteContext.Website.Name</p>
        <p>WebSite Title: @webSiteContext.Website.Title</p>
    </div>

</page>
```

### Links and navigation

Чтобы навигация между страницами была без перезагрузки страницы, ссылка должна иметь класс `applink`.

Этот класс для ссылок проставляется автоматически в случаях:

```
<a nav-url="@Url.Page("/Catalog")" class="item">Каталог</a>
<a asp-page="@Url.Page("/Catalog")" class="item">Каталог</a>
```

Результатом в обоих случаях будет:

```
<a href="/catalog" class="item applink">Каталог</a>
```

Так же во время навигации по ссылке можно перезаписывать текущее состояние навигации:

```
<a nav-url="@Url.Page("/Catalog")" nav-replace class="item">Каталог</a>

<a href="/catalog" data-nav-replace class="item applink">Каталог</a>
```

### Open Graph

Метаданные Open Graph задаются через свойство `OpenGraph` модели страницы и автоматически рендерятся в `<head>`, а также передаются клиенту при навигации.

```
public override string Title => "Main";

protected override Task OnPageRequestAsync(PageRequestContext context)
{
    OpenGraph = new PageOpenGraph(
        type: "website",
        image: new Uri("https://example.com/og.jpg"),
        title: Title,
        url: Link,
        description: Description)
    {
        SiteName = "Example"
    };

    return base.OnPageRequestAsync(context);
}
```

### Редиректы

Внутри страницы редирект корректно работает как для обычного запроса (HTTP 301/302), так и для клиентской навигации (заголовки `Page-Location`/`Page-Reload`):

```
protected override Task OnPageRequestAsync(PageRequestContext context)
{
    context.PageRedirect("/contacts");
    // или: context.Result = PageRedirect("/contacts", isPermanent: true, replace: false, reload: false);

    return base.OnPageRequestAsync(context);
}
```

Вне страницы (например, в контроллере или событиях аутентификации) используйте расширение ответа:

```
context.Response.RedirectPage("/contacts");
```

### Несколько сайтов

Для одного сайта используется `AddSingleWebsite("title")`. Для нескольких сайтов нужно реализовать `IWebsiteStore` (`FindByIdAsync`, `FindByNameAsync`, `GetAliasesAsync`, `GetTimeZoneAsync`) и зарегистрировать его:

```
services
    .AddWebsite(options => options.MapConfiguration(Configuration))
    .AddUrlMapProvider<BrandUp.Website.Infrastructure.SubdomainUrlMapProvider>()
    .AddMultyWebsite<MyWebsiteStore>(ServiceLifetime.Scoped);
    // или .AddMultyWebsiteFrom<MyWebsiteStore>() — если стор уже зарегистрирован в DI
```

Провайдер определения имени сайта из запроса:

- `SubdomainUrlMapProvider` — имя сайта берётся из поддомена (`msk.example.com` → `msk`);
- `PathUrlMapProvider` — имя сайта берётся из первого сегмента пути (`/msk/catalog` → `msk`).

### События сайта и страниц

`IWebsiteEvents` (регистрируется через `AddWebsiteEvents<T>()`) — наполнение клиентской модели приложения и кастомизация рендеринга `<head>`/`<body>`:

```
public class MyWebsiteEvents : IWebsiteEvents
{
    public Task StartAsync(StartWebsiteContext context)
    {
        context.ClientData["key"] = "value"; // попадёт в модель клиентского приложения
        return Task.CompletedTask;
    }

    public Task RenderHeadTag(OnRenderTagContext context) => Task.CompletedTask;
    public Task RenderBodyTag(OnRenderTagContext context) => Task.CompletedTask;
}
```

`IPageEvents` (регистрируется через `AddPageEvents<T>()`) — глобальные обработчики жизненного цикла всех страниц: `PageRequestAsync`, `PageRenderAsync`, `PageClientBuildAsync`, `PageClientNavigationAsync`.

## Клиентское приложение

Install NPM package [@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website).

Read [documentation](/npm/brandup-ui-website/README.md).

Точка входа — `WEBSITE.run(options, configure)`. В `pages`/`components` объявляются ленивые загрузчики скриптов страниц и компонентов (`preload: true` — предзагрузить заранее):

```
import { WEBSITE } from "@brandup/ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import "./styles.less";

WEBSITE.run({
    defaultPage: "base",
    pages: {
        "base": { factory: () => import("./pages/base"), preload: true },
        "signin": { factory: () => import("./pages/signin") }
    },
    components: {
        "test": { factory: () => import("./components/test") }
    }
}, (builder) => {
    builder.useMiddleware(() => new AuthMiddleware());
})
    .then(() => console.log("website runned"))
    .catch(reason => console.error(`website run error: ${reason}`));
```

Middleware реализует интерфейс `Middleware` из `@brandup/ui-app`:

```
import { Middleware, MiddlewareNext, StartContext } from "@brandup/ui-app";
import { WebsiteApplication } from "@brandup/ui-website";

export class AuthMiddleware implements Middleware {
    readonly name = "auth";

    start(context: StartContext<WebsiteApplication>, next: MiddlewareNext) {
        context.app.registerCommand("signout", () =>
            context.app.queue.enque({
                url: context.app.buildUrl("api/auth/signout"),
                method: "POST",
                success: () => context.app.reload()
            }));

        return next();
    }
}
```

## Сборка и тесты

Серверная часть (.NET 10):

```
dotnet build
dotnet test
```

Клиентская часть (npm):

```
npm install        # установка зависимостей корня и пакетов
npm test           # jest
cd npm/brandup-ui-website && npm run build   # сборка пакета (rollup)
```