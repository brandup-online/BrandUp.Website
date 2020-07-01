import { UIElement, AjaxRequest, AjaxQueue, DOM } from "brandup-ui";
import { PageModel, NavigationModel, AntiforgeryOptions } from "../common";
import { NavigationOptions, Application } from "brandup-ui-app";

export class Page<TModel extends PageModel = { type: string }> extends UIElement {
    readonly website: Website;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];
    private __scripts: Array<UIElement> = [];

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

        this.refreshScripts();

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
    refreshScripts() {
        const scriptElements = DOM.queryElements(this.element, "[data-content-script]");
        for (let i = 0; i < scriptElements.length; i++) {
            const elem = scriptElements.item(i);
            if (elem.hasAttribute("brandup-ui-element"))
                continue;

            const scriptName = elem.getAttribute("data-content-script");
            const script = this.website.getScript(scriptName);
            if (script) {
                script.then((t) => {
                    if (!this.__scripts)
                        return;

                    const uiElem: UIElement = new t.default(elem);
                    this.__scripts.push(uiElem);
                });
            }
        }
    }

    attachDestroyFunc(f: () => void) {
        this.__destroyCallbacks.push(f);
    }
    attachDestroyElement(elem: UIElement) {
        this.__destroyCallbacks.push(() => { elem.destroy(); });
    }

    destroy() {
        if (this.__scripts) {
            this.__scripts.map((elem) => { elem.destroy(); });
            this.__scripts = null;
        }

        if (this.__destroyCallbacks) {
            this.__destroyCallbacks.map((f) => { f(); });
            this.__destroyCallbacks = null;
        }

        this.queue.destroy();

        super.destroy();
    }
}

export interface Website {
    readonly app: Application;
    readonly antiforgery: AntiforgeryOptions;
    readonly queue: AjaxQueue;
    readonly id: string;
    updateHtml(html: string);
    request(options: AjaxRequest, includeAntiforgery?: boolean);
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string;
    nav(options: NavigationOptions);
    submit(form: HTMLFormElement, url?: string, handler?: string);
    getScript(name: string): Promise<{ default: any }>;
}