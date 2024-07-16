import { Middleware, ApplicationModel, NavigateContext, StartContext, LoadContext, Application, SubmitContext } from "brandup-ui-app";

export class ExampleMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
    start(context: StartContext, next: VoidFunction, end: VoidFunction) {
        next();
    }

    loaded(context: LoadContext, next: VoidFunction, end: VoidFunction) {
        next();
    }

    navigate(context: NavigateContext, next: VoidFunction, end: VoidFunction) {
        next();
    }

    submit(context: SubmitContext, next: VoidFunction, end: VoidFunction) {
        next();

        console.log(context);
    }
}