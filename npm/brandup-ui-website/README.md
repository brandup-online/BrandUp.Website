# brandup-ui-website

[![Build Status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status%2FBrandUp%2Fbrandup-website?branchName=master)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=58&branchName=master)

## Installation

Install NPM package [@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website).

```
npm i @brandup/ui-website
```

## Configure and start

```ts
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
    .then(() => console.log("website started"))
    .catch(reason => console.error(`website run error: ${reason}`));
```

## Application

Base application type `WebsiteApplication`.

```ts
class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    /** Ajax queue by current application instance. */
    readonly queue: AjaxQueue;
    /** Add antiforgery token for request. */
    prepareRequest(request: AjaxRequest): void;
    /** Request without ajax queue. */
    request<TData = any, TState = any>(options: AjaxRequest, abortSignal?: AbortSignal): Promise<AjaxResponse<TData, TState>>;
}
```

## Middleware

Develop custom middleware:

```ts
import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
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

        console.log(`website id: ${context.app.model.websiteId}`);

        return next();
    }

    navigate(context: NavigateContext, next: MiddlewareNext) {
        return next();
    }
}
```

Use middleware: `builder.useMiddleware(() => new AuthMiddleware());`

## Page

```ts
import { Page, PageModel } from "@brandup/ui-website";

class SignInPage extends Page<PageModel> {
    get typeName(): string { return "SignInPage"; }

    async onRenderContent() {
        await super.onRenderContent();
    }
}

export default SignInPage;
```

Page type must be exported as `default`. Register page in `WEBSITE.run`:

```ts
WEBSITE.run({
    pages: {
        "signin": { factory: () => import("./pages/signin") }
    }
}, (builder) => { });
```

## Component

Declare component in `WEBSITE.run`:

```ts
WEBSITE.run({
    components: {
        "test": { factory: () => import("./components/test") }
    }
}, (builder) => { });
```

Render component in HTML:

```html
<div data-content-script="test"></div>
```

Component class:

```ts
import { UIElement } from "@brandup/ui";

class TestComponent extends UIElement {
    get typeName(): string { return "TestComponent"; }
}

export default TestComponent;
```
