import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel, NavigationModel, WebsiteMiddleware } from "./types";
import { WebsiteApplication } from "./app";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";
import { NavigateContext, QueryParams } from "@brandup/ui-app";

export const PAGE_HASHCHANGED_EVENT = "hash-changed";

export class Page<TApplication extends WebsiteApplication = WebsiteApplication, TModel extends PageModel = PageModel> extends UIElement {
    private __context: NavigateContext<TApplication>;
    private __hash: string | null;
    readonly website: TApplication;
    readonly response: NavigationModel;
    readonly queue: AjaxQueue;

    get context() { return this.__context; }

    constructor(context: NavigateContext<TApplication>, response: NavigationModel) {
        super();

        this.__context = context;
        this.__hash = context.hash;

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
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.response.page as TModel; }
    get hash(): string | null { return this.__hash; }

    protected onRenderContent(): Promise<void> { return Promise.resolve(); }
    protected onRenderedContent(): Promise<void> { return Promise.resolve(); }
    protected onChangedHash(_newHash: string | null, _oldHash: string | null, action: PageHashAction): Promise<void> { return Promise.resolve(); }
    protected onSubmitForm(_response: AjaxResponse): Promise<void> { return Promise.resolve(); }

    /** @internal */
    async __render(element: HTMLElement) {
        this.setElement(element);

        await this.onRenderContent();
    }

    /** @internal */
    async __rendered() {
        await this.onRenderedContent();
        await this.__triggerChangeHash();
    }

    /** @internal */
    async __submitted(response: AjaxResponse) {
        if (!this.element)
            return;

        await this.onSubmitForm(response);
    }

    /** @internal */
    async __changedHash(context: NavigateContext<TApplication>) {
        if (!this.element)
            return;

        this.__context = context;
        this.__hash = context.hash;

        await this.__triggerChangeHash();
    }

    private async __triggerChangeHash() {
        const prevHash = this.__context.current?.hash ?? null;
        if (!this.__hash && !prevHash)
            return;

        let action: PageHashAction;
        if (this.__hash && !prevHash)
            action = "add";
        else if (!this.__hash && prevHash)
            action = "remove";
        else if (this.__hash === prevHash)
            action = "unchanged";
        else
            action = "changed";

        await this.onChangedHash(this.__hash, prevHash, action);

        this.trigger(PAGE_HASHCHANGED_EVENT, <PageHashChangedEvent>{
            prev: prevHash,
            new: this.__hash,
            action: action
        });
    }

    submit(form?: HTMLFormElement): HTMLFormElement {
        if (!this.element)
            throw new Error('Page is not rendered.');

        if (!form)
            form = DOM.queryElement(this.element, "form") as HTMLFormElement;
        if (!form)
            throw new Error(`Not found form on page for submit.`);

        form.dispatchEvent(new SubmitEvent("submit", { submitter: form, bubbles: true }));

        return form;
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

export type PageHashAction = "unchanged" | "add" | "remove" | "changed";

export interface PageHashChangedEvent {
    prev: string | null,
    new: string | null,
    action: PageHashAction
}