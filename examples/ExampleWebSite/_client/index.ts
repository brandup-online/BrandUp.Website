import { host } from "brandup-ui-website";

host.start({
    pageTypes: {
        "form": () => import("./pages/form")
    }
}, (builder) => {
});