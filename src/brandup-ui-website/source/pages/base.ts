import { UIElement } from "brandup-ui";
import { PageModel, NavigationModel, AntiforgeryOptions } from "../common";
import { NavigationOptions, Application } from "brandup-ui-app";
import { AjaxQueue, AjaxRequest } from "brandup-ui-ajax";
import { DOM } from "brandup-ui-dom";

export class Page<TModel extends PageModel = { type: string }> extends UIElement {
    readonly website: WebsiteContext;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];
    private __scripts: Array<UIElement> = [];
    private __isRendered = false;
    private __hash: string = null;

    constructor(website: WebsiteContext, nav: NavigationModel, element: HTMLElement) {
        super();

        this.setElement(element);

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
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.nav.page as TModel; }
    get hash(): string { return this.__hash; }

    protected onRenderContent() { return; }
    protected onChangedHash(newHash: string, oldHash: string) {
        return;
    }
    protected onCallbackHandler(data: any) { }

    render(hash: string) {
        if (this.__isRendered)
            return;
        this.__isRendered = true;
        this.__hash = hash;

        this.refreshScripts();

        this.onRenderContent();
    }
    callbackHandler(data: any) {
        this.onCallbackHandler(data);
    }
    changedHash(newHash: string, oldHash: string) {
        this.__hash = newHash;

        this.onChangedHash(newHash, oldHash);
    }
    submit(form?: HTMLFormElement) {
        if (!form)
            form = DOM.queryElement(this.element, "form") as HTMLFormElement;
        if (!form)
            throw `Not found form by submit.`;

        this.website.app.submit({ form, button: null });
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
            if (UIElement.hasElement(elem))
                continue;

            const scriptName = elem.getAttribute("data-content-script");
            const script = this.website.getScript(scriptName);
            if (script) {
                script.then((t) => {
                    if (!this.__scripts)
                        return;

                    const uiElem: UIElement = new t.default(elem, this.website, this);
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

export interface WebsiteContext {
    readonly app: Application;
    readonly antiforgery: AntiforgeryOptions;
    readonly queue: AjaxQueue;
    readonly id: string;
    readonly visitorId: string;
    readonly validationToken: string;
    updateHtml(html: string);
    request(options: AjaxRequest, includeAntiforgery?: boolean);
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string;
    nav(options: NavigationOptions);
    getScript(name: string): Promise<{ default: any }>;
}