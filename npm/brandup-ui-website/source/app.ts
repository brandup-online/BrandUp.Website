import { AjaxQueue, AjaxRequest } from "@brandup/ui-ajax";
import { Application, ContextData, EnvironmentModel, StopContext } from "@brandup/ui-app";
import { WebsiteApplicationModel, WebsiteMiddleware } from "./types";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

export class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    readonly queue: AjaxQueue;

    constructor(env: EnvironmentModel, model: TModel) {
        super(env, model);

        this.queue = new AjaxQueue({
            canRequest: (request) => this.prepareRequest(request)
        });
    }

    prepareRequest(request: AjaxRequest) {
        if (!request.headers)
            request.headers = {};

        const middleware = this.middleware<WebsiteMiddleware>(WEBSITE_MIDDLEWARE_NAME);
        const current = middleware.current;

        if (current && this.model.antiforgery && request.method && request.method !== "GET")
            request.headers[this.model.antiforgery.headerName] = current.model.validationToken;
    }

    destroy<TData extends ContextData = ContextData>(contextData?: TData | null): Promise<StopContext<Application, TData>> {
        this.queue.destroy();

        return super.destroy(contextData);
    }
}