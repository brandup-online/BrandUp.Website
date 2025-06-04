import { Middleware, MiddlewareNext, NavigateContext, StartContext, SubmitContext } from "@brandup/ui-app";

export class ExampleMiddleware implements Middleware {
    readonly name: string = "example";

    async start(context: StartContext, next: MiddlewareNext) {
        await next();

        console.log("start", context.data);
    }

    loaded(context: StartContext, next: MiddlewareNext) {
        return next();
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        console.log("navigate", context.data);
    }

    async submit(context: SubmitContext, next: MiddlewareNext) {
        await next();

        console.log(context);
    }
}