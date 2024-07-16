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

    const app = current = appBuilder.build(appData.env, appData.model);

    let isStarted = false;
    const appStartFunc = () => {
        if (isStarted)
            return;
        isStarted = true;

        app.start(() => {
            app.load(() => {
                app.nav({ url: null, callback: () => { 
                    if (callback)
                        callback(app);
                }});
            });
        });
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
                break;
            }
        }
    });

    window.addEventListener("load", () => {
        appStartFunc();
    });

    if (document.readyState === "complete")
        appStartFunc();

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