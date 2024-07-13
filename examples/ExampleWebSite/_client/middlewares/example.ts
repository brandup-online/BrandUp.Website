import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, Application } from "brandup-ui-app";

export class ExampleMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
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