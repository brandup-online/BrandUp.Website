import { Middleware, MiddlewareNext, StartContext } from "@brandup/ui-app";

export class CityMiddleware implements Middleware {
    readonly name: string = "city";

    async start(_context: StartContext, next: MiddlewareNext) {
        await next();
    }
}