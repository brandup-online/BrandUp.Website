import { host } from "brandup-ui-website";
import { AuthMiddleware } from "./middlewares/auth";
import "./styles.less";

host.start({
    pageTypes: {
        "signin": ()=> import("./pages/signin")
    }
}, (builder) => {
        builder.useMiddleware(new AuthMiddleware());
    });