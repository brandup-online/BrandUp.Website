import { AjaxQueue } from "@brandup/ui-ajax";
import { Application, EnvironmentModel, ContextData, NavigationOptions } from "@brandup/ui-app";
import { WebsiteApplicationModel } from "./common";

export class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    readonly queue: AjaxQueue;

    constructor(env: EnvironmentModel, model: TModel) {
        super(env, model);

        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                //if (this.model.antiforgery && options.method !== "GET" && options.method && this.__current)
                //    options.headers[this.model.antiforgery.headerName] = this.__current.model.validationToken;
            }
        });
    }

    protected onInitialize() {
        super.onInitialize();

        //this.invoker.next(() => new WebsiteMiddleware(options));
    }

    nav<TData extends ContextData>(options?: NavigationOptions<TData> | string | null) {
        return super.nav(options);
    }

    async pageHandler() {
        await this.invoker.invoke("pageHandler", {
            app: this,
            data: {}
        });
    }
}