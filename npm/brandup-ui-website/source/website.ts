import { ApplicationBuilder, EnvironmentModel, ContextData, NavigateContext } from "@brandup/ui-app";
import { WebsiteMiddleware, PagesOptions } from "./middleware";
import { WebsiteApplication } from "./app";
import { WebsiteApplicationModel } from "./common";

let current: WebsiteApplication | null = null;

const run = (pagesOptions: PagesOptions, configure: (builder: ApplicationBuilder<WebsiteApplicationModel>) => void, context?: ContextData): Promise<NavigateContext<WebsiteApplication>> => {
    if (current)
        Promise.reject("Application already started.");

    if (!context)
        context = {};

    return new Promise<NavigateContext<WebsiteApplication>>((resolve, reject) => {
        const appDataElem = <HTMLScriptElement>document.getElementById("app-data");
        if (!appDataElem)
            throw "Is not defined application startup configuration.";
        const appData = <StartupModel>JSON.parse(appDataElem.text);

        const appBuilder = new ApplicationBuilder<WebsiteApplicationModel>(appData.model);
        appBuilder
            .useApp(WebsiteApplication)
            .useMiddleware(() => new WebsiteMiddleware(pagesOptions));

        configure(appBuilder);

        const app = current = <WebsiteApplication>appBuilder.build(appData.env);

        let isStarted = false;
        const appStartFunc = () => {
            if (isStarted)
                return;
            isStarted = true;

            app.run(context)
                .then((navContext) => {
                    console.log("website started");
                    resolve(navContext);
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
    run(pagesOptions: PagesOptions, configure: (builder: ApplicationBuilder<WebsiteApplicationModel>) => void, context?: ContextData): Promise<NavigateContext<WebsiteApplication>>;
}

/** Website instance singleton point. */
const WEBSITE: IWebsiteInstance = {
    current: current,
    run: run
};

export { WEBSITE };