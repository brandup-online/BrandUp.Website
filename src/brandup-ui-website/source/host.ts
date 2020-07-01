import { ApplicationBuilder, Application, ApplicationModel, EnvironmentModel } from "brandup-ui-app";
import { WebsiteMiddleware, WebsiteOptions } from "./middlewares/website";
import { NavigationModel, AntiforgeryOptions } from "./common";

class AppHost {
    private static app: Application;

    start<TModel extends ApplicationModel>(options: WebsiteOptions, configure: (builder: ApplicationBuilder) => void, callback?: (app: Application<TModel>) => void) {
        if (AppHost.app) {
            AppHost.app.destroy();
            delete AppHost.app;
        }

        if (window["appStartup"]) {
            const appStartup = window["appStartup"] as StartupModel;

            const appBuilder = new ApplicationBuilder();
            appBuilder.useMiddleware(new WebsiteMiddleware(appStartup.nav, options, appStartup.antiforgery));
            configure(appBuilder);

            AppHost.app = appBuilder.build(appStartup.env, appStartup.model);
            let isInitiated = false;
            const appInitFunc = () => {
                if (isInitiated)
                    return;
                isInitiated = true;

                AppHost.app.init();
                if (callback)
                    callback(AppHost.app as Application<TModel>);
            };

            window.setTimeout(() => {
                if (document.readyState === "loading") {
                    document.addEventListener("readystatechange", () => {
                        if (document.readyState !== "loading")
                            appInitFunc();
                    });
                }
                else
                    appInitFunc();

                window.addEventListener("load", () => {
                    appInitFunc();
                    AppHost.app.load();
                });
            }, 0);

            return AppHost.app;
        }

        console.log("Is not find application config.");

        return null;
    }
}

interface StartupModel {
    env: EnvironmentModel;
    model: ApplicationModel;
    nav: NavigationModel;
    antiforgery: AntiforgeryOptions;
}

export const host = new AppHost();