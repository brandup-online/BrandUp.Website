import { ApplicationBuilder } from "brandup-ui-app";
import { WebsiteMiddleware } from "./middlewares/website";
var WebsiteHost = /** @class */ (function () {
    function WebsiteHost() {
    }
    WebsiteHost.prototype.start = function (options, configure, callback) {
        if (WebsiteHost.app)
            throw "Application already started.";
        var appDataElem = document.getElementById("app-data");
        if (!appDataElem)
            throw "Is not defined application startup configuration.";
        var appData = JSON.parse(appDataElem.text);
        var appBuilder = new ApplicationBuilder();
        appBuilder.useMiddleware(new WebsiteMiddleware(appData.nav, options, appData.antiforgery));
        configure(appBuilder);
        WebsiteHost.app = appBuilder.build(appData.env, appData.model);
        var isInitiated = false;
        var appInitFunc = function () {
            if (isInitiated)
                return;
            isInitiated = true;
            WebsiteHost.app.start(callback);
        };
        var isLoaded = false;
        var appLoadFunc = function () {
            if (isLoaded)
                return;
            isLoaded = true;
            WebsiteHost.app.load();
        };
        document.addEventListener("readystatechange", function () {
            switch (document.readyState) {
                case "loading": {
                    break;
                }
                case "interactive": {
                    appInitFunc();
                    break;
                }
                case "complete": {
                    appInitFunc();
                    appLoadFunc();
                    break;
                }
            }
        });
        window.addEventListener("load", function () {
            appInitFunc();
            appLoadFunc();
        });
        if (document.readyState === "complete") {
            appInitFunc();
            appLoadFunc();
        }
        return WebsiteHost.app;
    };
    return WebsiteHost;
}());
export var host = new WebsiteHost();
