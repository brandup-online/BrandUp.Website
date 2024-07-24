import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel, NavigationModel, WebsiteMiddleware } from "./types";
import { WebsiteApplication } from "./app";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

export class Page<TModel extends PageModel = PageModel> extends UIElement {
    readonly website: WebsiteApplication;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __hash: string | undefined;

    constructor(website: WebsiteApplication, nav: NavigationModel) {
        super();

        this.website = website;
        this.nav = nav;
        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (website.model.antiforgery && options.method !== "GET" && options.method)
                    options.headers[website.model.antiforgery.headerName] = nav.validationToken;
            }
        });
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.nav.page as TModel; }
    get hash(): string | undefined { return this.__hash; }

    protected onRenderContent(): Promise<void> { return Promise.resolve(); }
    protected onChangedHash(_newHash: string | null, _oldHash: string | null): Promise<void> { return Promise.resolve(); }
    protected onSubmitForm(_response: AjaxResponse): Promise<void> { return Promise.resolve(); }

    /** @internal */
    async __render(element: HTMLElement, hash: string | null | undefined) {
        this.setElement(element);

        if (hash)
            this.__hash = hash;

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
            delete this.__hash;

        await this.onChangedHash(newHash, oldHash);
    }

    async submit(form?: HTMLFormElement) {
        if (!this.element)
            throw new Error('Page is not rendered.');

        if (!form)
            form = DOM.queryElement(this.element, "form") as HTMLFormElement;
        if (!form)
            throw new Error(`Not found form on page for submit.`);

        await this.website.submit({ form, button: null });
    }

    buildUrl(queryParams: { [key: string]: string }): string {
        const params: { [key: string]: string } = {};
        for (const k in this.nav.query)
            params[k] = this.nav.query[k];

        if (queryParams) {
            for (const k in queryParams)
                params[k] = queryParams[k];
        }

        return this.website.buildUrl(this.nav.path, params);
    }

    renderComponents() {
        const middleware = this.website.middleware<WebsiteMiddleware>(WEBSITE_MIDDLEWARE_NAME);
        middleware.renderComponents(this);
    }

    destroy() {
        this.queue.destroy();

        super.destroy();
    }
}