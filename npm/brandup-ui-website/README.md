# brandup-ui-website

[![Build Status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status%2FBrandUp%2Fbrandup-website?branchName=master)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=58&branchName=master)

## Installation

Install NPM package [@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website).

```
npm i @brandup/ui-website
```

## Configure and start

```
import { WEBSITE } from "@brandup/ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import "./styles.less";

WEBSITE.run({
        defaultPage: "base",
        pages: {
            "base": { factory: ()=> import("./pages/base"), preload: true }
            "signin": { factory: ()=> import("./pages/signin") }
        },
        components: {
            "test": { factory: () => import("./components/test") }
        }
    }, (builder) => {
        builder
            .useMiddleware(() => new AuthMiddleware());
    })
    .then(() => console.log("website runned"));
```

## Application

Base application type `WebsiteApplication<TModel extends WebsiteApplicationModel>`.

```
class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    /** Ajax queue by current application instance. */
    readonly queue: AjaxQueue;
    /** Add antiforgery token for request. */
    prepareRequest(request: AjaxRequest): void;
    /** Request without ajax queue. */
    request(options: AjaxRequest): Promise<_brandup_ui_ajax.AjaxResponse<any, any>>;
}
```

## Middleware

Develop custom middleware:

```
import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
import { Page, PageModel, WebsiteApplicationModel } from "@brandup/ui-website";
import { request } from "@brandup/ui-ajax";

export class AuthMiddleware extends Middleware<WebsiteApplicationModel> {
    start(context: StartContext, next: MiddlewareNext) {
        this.app.registerCommand("signout", () => {
            request({
                url: context.app.buildUrl("api/auth/signout"),
                method: "POST"
            }).then(() => this.app.reload());
        });

        console.log(`website id: ${context.app.model.websiteId}`);

        return next();
    }

    async loaded(context: StartContext, next: MiddlewareNext) {
        await next();
    }

    navigate(context: NavigateContext, next: MiddlewareNext) {
        return next();
    }
}
```

Use middleware: `builder.useMiddleware(() => new AuthMiddleware());`

## Page

```
import { Page, PageModel } from "@brandup/ui-website";

class SignInPage extends Page<PageModel> {
    get typeName(): string { return "SignInPage" }

    onRenderContent() {
        this.registerCommand("test", () => {
            this.submit();
        });

        super.onRenderContent();
    }
}

export default SignInPage;
```

Export page type require as default.

Register page type:

```
host.start({
    pageTypes: {
        "signin": ()=> import("./pages/signin")
    }
});
```

## Component

Declare component:

```
import { WEBSITE } from "@brandup/ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import "./styles.less";

WEBSITE.run({
        components: {
            "test": { factory: () => import("./components/test") }
        }
    }, (builder) => { });
```

Render component:

```
<div data-content-script="test"></div>
```

Component code:

