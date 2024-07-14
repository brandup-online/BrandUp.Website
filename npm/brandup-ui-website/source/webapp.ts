import { ApplicationBuilder, Application, ApplicationModel, EnvironmentModel } from "brandup-ui-app";
import { WebsiteMiddleware, WebsiteOptions } from "./middleware";
import { AntiforgeryOptions } from "./common";

let current: Application<ApplicationModel> | null = null;

const run = (options: WebsiteOptions, configure: (builder: ApplicationBuilder<ApplicationModel>) => void, callback?: (app: Application<ApplicationModel>) => void) => {
    if (current)
        throw "Application already started.";

    const appDataElem = <HTMLScriptElement>document.getElementById("app-data");
    if (!appDataElem)
        throw "Is not defined application startup configuration.";
    const appData = <StartupModel>JSON.parse(appDataElem.text);

    const appBuilder = new ApplicationBuilder();
    appBuilder
        .useMiddleware(new WebsiteMiddleware(options, appData.antiforgery));

    configure(appBuilder);

    current = appBuilder.build(appData.env, appData.model);

    let isStarted = false;
    const appStartFunc = () => {
        if (isStarted)
            return;
        isStarted = true;

        current?.start(callback);
    };

    let isLoaded = false;
    const appLoadFunc = () => {
        if (isLoaded)
            return;
        isLoaded = true;

        current?.load();
    };

    document.addEventListener("readystatechange", () => {
        switch (document.readyState) {
            case "loading": {
                break;
            }
            case "interactive": {
                appStartFunc();
                break;
            }
            case "complete": {
                appStartFunc();
                appLoadFunc();
                break;
            }
        }
    });

    window.addEventListener("load", () => {
        appStartFunc();
        appLoadFunc();
    });

    if (document.readyState === "complete") {
        appStartFunc();
        appLoadFunc();
    }

    return current;
};

interface StartupModel {
    env: EnvironmentModel;
    model: ApplicationModel;
    antiforgery: AntiforgeryOptions;
}

interface WebAppImlp {
    readonly current: Application<ApplicationModel> | null;
    run(options: WebsiteOptions, configure: (builder: ApplicationBuilder<ApplicationModel>) => void, callback?: (app: Application<ApplicationModel>) => void);
}

const WebApp: WebAppImlp = {
    current: current,
    run: run
};

export { WebApp };