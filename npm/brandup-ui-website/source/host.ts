import { ApplicationBuilder, Application, ApplicationModel, EnvironmentModel } from "brandup-ui-app";
import { WebsiteMiddleware, WebsiteOptions } from "./middlewares/website";
import { AntiforgeryOptions } from "./common";

class WebsiteHost {
    private static app: Application;

    start<TModel extends ApplicationModel>(options: WebsiteOptions, configure: (builder: ApplicationBuilder) => void, callback?: (app: Application<TModel>) => void) {
        if (WebsiteHost.app)
            throw "Application already started.";

        const appDataElem = <HTMLScriptElement>document.getElementById("app-data");
        if (!appDataElem)
            throw "Is not defined application startup configuration.";
        const appData = <StartupModel>JSON.parse(appDataElem.text);

        const appBuilder = new ApplicationBuilder();
        appBuilder.useMiddleware(new WebsiteMiddleware(options, appData.antiforgery));
        configure(appBuilder);

        WebsiteHost.app = appBuilder.build(appData.env, appData.model);

        let isStarted = false;
        const appStartFunc = () => {
            if (isStarted)
                return;
            isStarted = true;

            WebsiteHost.app.start(callback);
        };

        let isLoaded = false;
        const appLoadFunc = () => {
            if (isLoaded)
                return;
            isLoaded = true;

            WebsiteHost.app.load();
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

        return WebsiteHost.app;
    }
}

interface StartupModel {
    env: EnvironmentModel;
    model: ApplicationModel;
    antiforgery: AntiforgeryOptions;
}

export const host = new WebsiteHost();