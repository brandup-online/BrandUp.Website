import { ajaxRequest } from "brandup-ui-ajax";
import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, NavigatingContext } from "brandup-ui-app";

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
        console.log(`visitor id: ${this.app.model.visitorId}`);

        next();
    }

    loaded(context: LoadContext, next) {
        next();

        console.log(context.items);
    }

    navigating(context: NavigatingContext, next) {
        console.log(context);
        next();
    }

    navigate(context: NavigateContext, next) {
        next();

        console.log(context.items);
    }
}