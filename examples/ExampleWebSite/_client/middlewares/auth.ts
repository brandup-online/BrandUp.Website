import { ajaxRequest } from "@brandup/ui-ajax";
import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";

export class AuthMiddleware implements Middleware {
    readonly name: string = "auth";

    start(context: StartContext, next: MiddlewareNext) {
        context.app.registerCommand("signout", () => {
            ajaxRequest({
                url: context.app.buildUrl("api/auth/signout"),
                method: "POST",
                state: null,
                success: () => {
                    context.app.reload();
                }
            });
        });

        console.log(`website id: ${context.app.model.websiteId}`);

        return next();
    }
}