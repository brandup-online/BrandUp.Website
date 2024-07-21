import { Middleware, MiddlewareNext, NavigateContext, StartContext, SubmitContext } from "@brandup/ui-app";

export class ExampleMiddleware implements Middleware {
    readonly name: string = "example";

    start(context: StartContext, next: MiddlewareNext) {
        return next();
    }

    loaded(context: StartContext, next: MiddlewareNext) {
        return next();
    }

    navigate(context: NavigateContext, next: MiddlewareNext) {
        return next();
    }

    async submit(context: SubmitContext, next: MiddlewareNext) {
        await next();

        console.log(context);
    }
}