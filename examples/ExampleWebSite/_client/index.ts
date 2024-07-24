﻿import { WEBSITE } from "@brandup/ui-website";
import { ExampleMiddleware } from "./middlewares/example";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

WEBSITE.run({
    pages: {
        "signin": { factory: () => import("./pages/signin") },
        "form": { factory: () => import("./pages/form"), preload: true },
        "examples-navigation": { factory: () => import("./pages/examples/navigation") }
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
    .then(() => console.log("example runned"))
    .catch(reason => console.log(`example run error: ${reason}`));