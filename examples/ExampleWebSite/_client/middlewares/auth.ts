import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext } from "brandup-ui-app";
import { ajaxRequest } from "brandup-ui";

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

        console.log(`Website ID: ${this.app.model.websiteId}`);
        console.log(`Visitor ID: ${context.items["nav"].visitorId}`);

        next();

        console.log(context.items["nav"]);
    }

    loaded(context: LoadContext, next) {
        next();

        console.log(context.items["nav"]);
    }

    navigate(context: NavigateContext, next) {
        next();

        console.log(context.items["nav"]);
    }
}