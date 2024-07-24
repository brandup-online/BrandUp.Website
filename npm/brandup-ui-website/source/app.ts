import { AjaxQueue } from "@brandup/ui-ajax";
import { Application, ContextData, EnvironmentModel, StopContext } from "@brandup/ui-app";
import { WebsiteApplicationModel } from "./common";
import { WebsiteMiddleware, WEBSITE_MIDDLEWARE_NAME } from "./middleware";

export class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    readonly queue: AjaxQueue;

    constructor(env: EnvironmentModel, model: TModel) {
        super(env, model);

        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                const middleware = this.middleware<WebsiteMiddleware>(WEBSITE_MIDDLEWARE_NAME);
                const current = middleware.current;

                if (this.model.antiforgery && options.method !== "GET" && options.method && current)
                    options.headers[this.model.antiforgery.headerName] = current.model.validationToken;
            }
        });
    }

    destroy<TData extends ContextData = ContextData>(contextData?: TData | null): Promise<StopContext<Application, TData>> {
        this.queue.destroy();

        return super.destroy(contextData);
    }
}