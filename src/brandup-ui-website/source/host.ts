import { IApplication, ApplicationBuilder, Application, ApplicationModel, EnvironmentModel } from "brandup-ui-app";
import { WebsiteMiddleware, WebsiteOptions } from "./middlewares/website";
import { NavigationModel, AntiforgeryOptions } from "./common";

class AppHost {
    private app: IApplication;

    start<TModel extends ApplicationModel>(options: WebsiteOptions, configure: (builder: ApplicationBuilder) => void, callback?: (app: Application<TModel>) => void) {
        if (this.app) {
            this.app.destroy();
            delete this.app;
        }

        if (window["appStartup"]) {
            const appStartup = window["appStartup"] as StartupModel;

            const appBuilder = new ApplicationBuilder();
            appBuilder.useMiddleware(new WebsiteMiddleware(appStartup.nav, options, appStartup.antiforgery));
            configure(appBuilder);

            const app = appBuilder.build<TModel>(appStartup.env, appStartup.model as TModel);
            let isInitiated = false;
            const appInitFunc = () => {
                if (isInitiated)
                    return;
                isInitiated = true;

                app.init();
                if (callback)
                    callback(app);
            };

            window.setTimeout(() => {
                this.app = app;

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
                    app.load();
                });
            }, 0);

            return app;
        }

        console.log("Is not find application config.");

        return null;
    }
}

interface StartupModel {
    env: EnvironmentModel;
    nav: NavigationModel;
    model: ApplicationModel;
    antiforgery: AntiforgeryOptions;
}

export const host = new AppHost();