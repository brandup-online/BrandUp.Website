# BrandUp.Website

Базовый инфраструктурный фреймворк для Web-сайтов.

## Installation

NuGet-package: [https://www.nuget.org/packages/BrandUp.Website/](https://www.nuget.org/packages/BrandUp.Website/)


## Конфигурация

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Website": {
    "Host": "localhost",
    "Aliases": [ "test.ru" ],
    "CookiesPrefix": "ex",
    "ProtectionPurpose": "148c26c8-0267-4677-b2f5-dda1256d5947"
  }
}
```

**Website:Host** - доменное имя сайта.

**Website:Aliases** - список алиасов, с которых будет выполняться автоматический редирект на домен Host.

**Website:CookiesPrefix** - префикс ключей Cookies.

**Website:ProtectionPurpose** - The purpose to be assigned to the newly-created Microsoft.AspNetCore.DataProtection.IDataProtector.

Использование параметров сайта:
```
var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebsiteOptions>>()
```

### Запуск

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

Контекст сайта **IWebsiteContext** создаётся для каждого входящего запроса. Он доступен через IServiceProvider.

## Представления и модели

### Представление _Layout

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

### Представление страницы

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

### Модель страницы

Базовым классом модели страницы является класс AppPageModel.

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
    protected override Task OnPageClientBuildAsync(PageBuildContext context)
    {
        return base.OnPageClientBuildAsync(context);
    }

    /// <summary>
    /// Выполняется при конструировании контекста навигации для клиента.
    /// </summary>
    protected override Task OnPageClientNavigationAsync(PageNavidationContext context)
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

### Ссылки и навигация

Чтобы навигация между страницами была без перезагрузки страницы, ссылка должна иметь класс `applink`.

Этот класс для ссылок проставляется автоматически в случаях:

```
<a nav-url="@Url.Page("/Catalog")" clasa="item">Каталог</a>
<a asp-page="@Url.Page("/Catalog")" clasa="item">Каталог</a>
```

Результатом в обоих случаях будет:

```
<a href="/catalog" class="item applink">Каталог</a>
```

Так же во время навигации по ссылке можно перезаписывать текущее состояние навигации:

```
<a nav-url="@Url.Page("/Catalog")" nav-replace clasa="item">Каталог</a>

<a href="/catalog" data-nav-replace class="item applink">Каталог</a>
```

## Клиентское приложение

```

import { host } from "brandup-ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import "./styles.less";

host.start({
    pageTypes: {
        "signin": ()=> import("./pages/signin")
    }
}, (builder) => {
        builder
            .useMiddleware(new AuthMiddleware());
    });

import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, NavigatingContext } from "brandup-ui-app";
import { ajaxRequest } from "brandup-ui";

export class AuthMiddleware extends Middleware<ApplicationModel> {
    start(context: StartContext, next) {
        this.app.registerCommand("signout", () => {
            ajaxRequest({
                url: this.app.uri("api/auth/signout"),
                method: "POST",
                state: null,
                success: () => {
                    this.app.reload();
                }
            });
        });

        console.log(`website id: ${this.app.model.websiteId}`);

        next();
    }

    loaded(context: LoadContext, next) {
        next();
    }

    navigating(context: NavigatingContext, next) {
        next();
    }

    navigate(context: NavigateContext, next) {
        next();
    }
}

```