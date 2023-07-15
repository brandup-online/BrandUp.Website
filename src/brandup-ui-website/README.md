# brandup-ui-website

## Configuration and start

```
import { host } from "brandup-ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

host.start({
    pageTypes: {
        "signin": ()=> import("./pages/signin")
    },
    scripts: {
        "test": () => import("./components/test")
    }
}, (builder) => {
        builder
            .useMiddleware(new AuthMiddleware())
            .useMiddleware(new CityMiddleware());
    });
```

Custom page script:

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