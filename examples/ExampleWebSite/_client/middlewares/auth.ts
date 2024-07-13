import { ajaxRequest } from "brandup-ui-ajax";
import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, Application } from "brandup-ui-app";

export class AuthMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
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

        next();
    }

    loaded(context: LoadContext, next) {
        next();
    }

    navigate(context: NavigateContext, next) {
        next();
    }
}