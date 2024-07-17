import { WEBSITE } from "brandup-ui-website";
import { ExampleMiddleware } from "./middlewares/example";
import { AuthMiddleware } from "./middlewares/auth";
import { CityMiddleware } from "./middlewares/city";
import "./styles.less";

WEBSITE.run({
        pageTypes: {
            "signin": () => import("./pages/signin"),
            "form": () => import("./pages/form"),
            "examples-navigation": () => import("./pages/examples/navigation")
        },
        scripts: {
            "test": () => import("./components/test")
        }
    }, (builder) => {
        builder
            .useMiddleware(new ExampleMiddleware())
            .useMiddleware(new AuthMiddleware())
            .useMiddleware(new CityMiddleware());
    })
    .then(() => console.log("example runned"))
    .catch(reason => console.log(`example run error: ${reason}`));