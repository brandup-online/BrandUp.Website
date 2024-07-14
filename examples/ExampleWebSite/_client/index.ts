import { WebApp } from "brandup-ui-website";
import { ExampleMiddleware } from "./middlewares/example";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

WebApp.run({
    pageTypes: {
        "signin": () => import("./pages/signin"),
        "form": () => import("./pages/form")
    },
    scripts: {
        "test": () => import("./components/test")
    }
}, (builder) => {
    builder
        .useMiddleware(new ExampleMiddleware())
        .useMiddleware(new AuthMiddleware())
        .useMiddleware(new CityMiddleware());
});