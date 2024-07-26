import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel, NavigationModel, WebsiteMiddleware } from "./types";
import { WebsiteApplication } from "./app";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";
import { NavigateContext, QueryParams } from "@brandup/ui-app";

export class Page<TApplication extends WebsiteApplication = WebsiteApplication, TModel extends PageModel = PageModel> extends UIElement {
    readonly context: NavigateContext<TApplication>;
    readonly website: TApplication;
    readonly response: NavigationModel;
    readonly queue: AjaxQueue;
    private __hash: string | null;

    constructor(context: NavigateContext<TApplication>, response: NavigationModel) {
        super();

        this.context = context;
        this.website = context.app;
        this.response = response;
        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (context.app.model.antiforgery && options.method !== "GET" && options.method)
                    options.headers[context.app.model.antiforgery.headerName] = response.validationToken;
            }
        });

        this.__hash = context.hash;
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.response.page as TModel; }
    get hash(): string | null { return this.__hash; }

    protected onRenderContent(): Promise<void> { return Promise.resolve(); }
    protected onChangedHash(_newHash: string | null, _oldHash: string | null): Promise<void> { return Promise.resolve(); }
    protected onSubmitForm(_response: AjaxResponse): Promise<void> { return Promise.resolve(); }

    /** @internal */
    async __render(element: HTMLElement) {
        this.setElement(element);

        await this.onRenderContent();
    }

    /** @internal */
    async __submitted(response: AjaxResponse) {
        if (!this.element)
            return;

        await this.onSubmitForm(response);
    }

    /** @internal */
    async __changedHash(newHash: string | null, oldHash: string | null) {
        if (!this.element)
            return;

        if (newHash)
            this.__hash = newHash;
        else
            this.__hash = null;

        await this.onChangedHash(newHash, oldHash);
    }

    submit(form?: HTMLFormElement) {
        if (!this.element)
            throw new Error('Page is not rendered.');

        if (!form)
            form = DOM.queryElement(this.element, "form") as HTMLFormElement;
        if (!form)
            throw new Error(`Not found form on page for submit.`);

        form.submit();
    }

    buildUrl(query: QueryParams): string {
        const params: QueryParams = {};
        for (const name in this.response.query)
            params[name] = this.response.query[name];

        if (query) {
            for (const name in query)
                params[name] = query[name];
        }

        return this.website.buildUrl(this.response.path, params);
    }

    renderComponents() {
        const middleware = this.website.middleware<WebsiteMiddleware>(WEBSITE_MIDDLEWARE_NAME);
        return middleware.renderComponents(this);
    }

    destroy() {
        this.queue.destroy();

        super.destroy();
    }
}