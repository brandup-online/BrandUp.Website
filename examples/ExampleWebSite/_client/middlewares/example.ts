import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext } from "brandup-ui-app";

export class ExampleMiddleware extends Middleware<ApplicationModel> {
    start(context: StartContext, next) {
        next();
    }

    loaded(context: LoadContext, next) {
        next();
    }

    navigate(context: NavigateContext, next) {
        next();
    }
}