import { AjaxQueue, AjaxRequest, request } from "@brandup/ui-ajax";
import { Application, ContextData, EnvironmentModel, StopContext } from "@brandup/ui-app";
import { WebsiteApplicationModel, WebsiteMiddleware, WebsiteOptions } from "./types";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

export class WebsiteApplication<TModel extends WebsiteApplicationModel = WebsiteApplicationModel> extends Application<TModel> {
    readonly options: WebsiteOptions;
    /** Ajax queue by current application instance. */
    readonly queue: AjaxQueue;

    constructor(env: EnvironmentModel, model: TModel, options: WebsiteOptions) {
        super(env, model);

        this.options = options;
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
    request(options: AjaxRequest, abortSignal?: AbortSignal) {
        this.prepareRequest(options);
        return request(options, abortSignal);
    }

    destroy<TData extends ContextData = ContextData>(contextData?: TData | null): Promise<StopContext<this, TData>> {
        this.queue.destroy();

        return super.destroy(contextData);
    }
}