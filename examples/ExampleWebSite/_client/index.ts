﻿import { host } from "brandup-ui-website";
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