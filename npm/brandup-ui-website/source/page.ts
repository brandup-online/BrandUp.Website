import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel, NavigationModel } from "./common";
import { WebsiteApplication } from "./app";

export class Page<TModel extends PageModel = { type: string }> extends UIElement {
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

    async render(element: HTMLElement, hash: string | null) {
        this.setElement(element);

        if (hash)
            this.__hash = hash;

        await this.onRenderContent();
    }

    async formSubmitted(response: AjaxResponse) {
        if (!this.element)
            return;

        await this.onSubmitForm(response);
    }

    async changedHash(newHash: string | null, oldHash: string | null) {
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

    destroy() {
        this.queue.destroy();

        super.destroy();
    }
}