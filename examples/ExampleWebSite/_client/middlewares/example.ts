import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, Application } from "brandup-ui-app";

export class ExampleMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
    start(context: StartContext, next, end) {
        super.start(context, next, end);
    }

    loaded(context: LoadContext, next, end) {
        super.loaded(context, next, end);
    }

    navigate(context: NavigateContext, next, end) {
        super.navigate(context, next, end);
    }

    submit(context, next, end) {
        super.submit(context, next, end);

        console.log(context);
    }
}