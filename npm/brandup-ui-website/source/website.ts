import { ApplicationBuilder, EnvironmentModel, ContextData } from "@brandup/ui-app";
import { WebsiteMiddleware, WebsiteOptions } from "./middleware";
import { WebsiteApplication } from "./app";
import { WebsiteApplicationModel } from "./common";

let current: WebsiteApplication | null = null;

const run = (options: WebsiteOptions, configure: (builder: ApplicationBuilder<WebsiteApplicationModel>) => void, context?: ContextData): Promise<WebsiteApplication> => {
    if (current)
        Promise.reject("Application already started.");

    if (!context)
        context = {};

    context["start"] = true;

    return new Promise<WebsiteApplication>((resolve, reject) => {
        const appDataElem = <HTMLScriptElement>document.getElementById("app-data");
        if (!appDataElem)
            throw "Is not defined application startup configuration.";
        const appData = <StartupModel>JSON.parse(appDataElem.text);

        const appBuilder = new ApplicationBuilder<WebsiteApplicationModel>();
        appBuilder
            .useApp(WebsiteApplication)
            .useMiddleware(() => new WebsiteMiddleware(options));

        configure(appBuilder);

        const app = current = <WebsiteApplication>appBuilder.build(appData.env, appData.model);

        let isStarted = false;
        const appStartFunc = () => {
            if (isStarted)
                return;
            isStarted = true;

            app.run(context)
                .then(() => {
                    console.log("website started");
                    resolve(app);
                })
                .catch(reason => {
                    console.error(`website run error: ${reason}`);
                    reject(reason);
                });
        };

        document.addEventListener("readystatechange", () => {
            switch (document.readyState) {
                case "loading":
                    break;
                case "interactive":
                    appStartFunc();
                    break;
                case "complete":
                    appStartFunc();
                    break;
            }
        });

        window.addEventListener("load", () => {
            appStartFunc();
        });

        if (document.readyState === "complete")
            appStartFunc();

        return current;
    });
};

interface StartupModel {
    env: EnvironmentModel;
    model: WebsiteApplicationModel;
}

interface IWebsiteInstance {
    /** Current runned website application. */
    readonly current: WebsiteApplication | null;
    /**
     * Run website application.
     * @param options Website options.
     * @param configure Configuration website application callback.
     * @param context Custom run application context.
     * @returns Application instance.
     */
    run(options: WebsiteOptions, configure: (builder: ApplicationBuilder<WebsiteApplicationModel>) => void, context?: ContextData): Promise<WebsiteApplication>;
}

/** Website instance singleton point. */
const WEBSITE: IWebsiteInstance = {
    current: current,
    run: run
};

export { WEBSITE };