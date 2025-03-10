﻿import { WEBSITE } from "@brandup/ui-website";
import { ExampleMiddleware } from "./middlewares/example";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

WEBSITE.run({
    defaultPage: "base",
    pages: {
        "base": { factory: () => import("./pages/base"), preload: true },
        "form": { factory: () => import("./pages/form"), preload: true },
        "signin": { factory: () => import("./pages/signin") },
        "examples-navigation": { factory: () => import("./pages/examples/navigation") },
        "example-submit": { factory: () => import("./pages/examples/submit") }
    },
    components: {
        "test": { factory: () => import("./components/test"), preload: true }
    }
}, (builder) => {
    builder
        .useMiddleware(() => new ExampleMiddleware())
        .useMiddleware(() => new AuthMiddleware())
        .useMiddleware(() => new CityMiddleware());
})
    .then(() => console.log("website runned"))
    .catch(reason => console.log(`website run error: ${reason}`));