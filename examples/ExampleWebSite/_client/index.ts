import { host } from "brandup-ui-website";
import { WebsiteMiddleware } from "./middlewares/website";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

host.start({
    pageTypes: {
        "signin": () => import("./pages/signin"),
        "form": () => import("./pages/form")
    },
    scripts: {
        "test": () => import("./components/test")
    }
}, (builder) => {
    builder
        .useMiddleware(new WebsiteMiddleware())
        .useMiddleware(new AuthMiddleware())
        .useMiddleware(new CityMiddleware());
});