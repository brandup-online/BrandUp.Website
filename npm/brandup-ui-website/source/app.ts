import { AjaxQueue, AjaxRequest, request } from "@brandup/ui-ajax";
import { Application, ContextData, EnvironmentModel, StopContext } from "@brandup/ui-app";
import { WebsiteApplicationModel, WebsiteMiddleware } from "./types";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

export class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    /** Ajax queue by current application instance. */
    readonly queue: AjaxQueue;

    constructor(env: EnvironmentModel, model: TModel) {
        super(env, model);

        this.queue = new AjaxQueue({
            canRequest: (request) => this.prepareRequest(request)
        });
    }

    /** Add antiforgery token for request. */
    prepareRequest(request: AjaxRequest) {
        if (!request.headers)
            request.headers = {};

        const middleware = this.middleware<WebsiteMiddleware>(WEBSITE_MIDDLEWARE_NAME);
        middleware.prepareRequest(request);
    }

    /** Request without ajax queue. */
    request(options: AjaxRequest) {
        this.prepareRequest(options);
        return request(options);
    }

    destroy<TData extends ContextData = ContextData>(contextData?: TData | null): Promise<StopContext<Application, TData>> {
        this.queue.destroy();

        return super.destroy(contextData);
    }
}