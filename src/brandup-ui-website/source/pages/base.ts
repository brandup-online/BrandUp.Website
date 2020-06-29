import { UIElement, AjaxRequest, AjaxQueue, DOM } from "brandup-ui";
import { PageModel, NavigationModel, AntiforgeryOptions } from "../common";
import { IApplication, NavigationOptions } from "brandup-ui-app";

export class Page<TModel extends PageModel> extends UIElement {
    readonly website: Website;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];

    constructor(website: Website, nav: NavigationModel, element: HTMLElement) {
        super();

        this.website = website;
        this.nav = nav;
        this.queue = new AjaxQueue({
            preRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (website.antiforgery && options.method !== "GET" && options.method)
                    options.headers[website.antiforgery.headerName] = nav.validationToken;
            }
        });
        this.setElement(element);

        this.onRenderContent();
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.nav.page as TModel; }

    protected onRenderContent() { return; }

    submit(form?: HTMLFormElement, handler?: string) {
        if (!form)
            form = DOM.getElementByName("form") as HTMLFormElement;

        if (!form)
            throw `Not found form by submit.`;

        this.website.submit(form, null, handler);
    }

    buildUrl(queryParams: { [key: string]: string }): string {
        const params: { [key: string]: string } = {};
        for (const k in this.nav.query) {
            params[k] = this.nav.query[k];
        }

        if (queryParams) {
            for (const k in queryParams) {
                params[k] = queryParams[k];
            }
        }

        return this.website.app.uri(this.nav.path, params);
    }

    attachDestroyFunc(f: () => void) {
        this.__destroyCallbacks.push(f);
    }
    attachDestroyElement(elem: UIElement) {
        this.__destroyCallbacks.push(() => { elem.destroy(); });
    }

    destroy() {
        if (this.__destroyCallbacks) {
            this.__destroyCallbacks.map((f) => { f(); });
            this.__destroyCallbacks = null;
        }

        this.queue.destroy();

        super.destroy();
    }
}

export interface Website {
    readonly app: IApplication;
    readonly antiforgery: AntiforgeryOptions;
    readonly queue: AjaxQueue;
    updateHtml(html: string);
    request(options: AjaxRequest, includeAntiforgery?: boolean);
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string;
    nav(options: NavigationOptions);
    submit(form: HTMLFormElement, url?: string, handler?: string);
}