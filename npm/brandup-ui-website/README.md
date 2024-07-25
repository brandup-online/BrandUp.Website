# brandup-ui-website

[![Build Status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status%2FBrandUp%2Fbrandup-website?branchName=master)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=58&branchName=master)

## Installation

Install NPM package [brandup-ui-website](https://www.npmjs.com/package/brandup-ui-website).

## Configure and start

```
import { WEBSITE } from "@brandup/ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

WEBSITE.run({
    defaultPage: "base",
    pages: {
        "base": ()=> import("./pages/base")
        "signin": ()=> import("./pages/signin")
    },
    components: {
        "test": () => import("./components/test")
    }
}, (builder) => {
        builder
            .useMiddleware(new AuthMiddleware());
    });
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

Use middleware: `builder.useMiddleware(new AuthMiddleware());`

## Page

```
import { Page, PageModel } from "brandup-ui-website";

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