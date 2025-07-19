import { Middleware, MiddlewareNext, StartContext } from "@brandup/ui-app";
import { WebsiteApplication } from "@brandup/ui-website";

export class AuthMiddleware implements Middleware {
    readonly name: string = "auth";

    start(context: StartContext<WebsiteApplication>, next: MiddlewareNext) {
        context.app.registerCommand("signout", () => {
            context.app.queue.reset();
            return context.app.queue.enque({
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