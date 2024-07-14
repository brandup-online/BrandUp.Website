import { PageModel, NavigationModel, WebsiteContext } from "./common";
import { AjaxQueue } from "brandup-ui-ajax";
import { UIElement } from "brandup-ui";
import { DOM } from "brandup-ui-dom";

export class Page<TModel extends PageModel = { type: string }> extends UIElement {
    readonly website: WebsiteContext;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> | null = [];
    private __scripts: Array<UIElement> | null = [];
    private __isRendered = false;
    private __hash: string | null = null;

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
    get hash(): string | null { return this.__hash; }

    protected onRenderContent() { }
    protected onChangedHash(newHash: string | null, oldHash: string | null) { }
    protected onCallbackHandler(data: any) { }

    render(hash: string | null) {
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
    changedHash(newHash: string | null, oldHash: string | null) {
        this.__hash = newHash;

        this.onChangedHash(newHash, oldHash);
    }
    submit(form?: HTMLFormElement) {
        if (!this.element)
            throw 'Not set page element.';

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
        if (!this.element)
            return;

        const scriptElements = DOM.queryElements(this.element, "[data-content-script]");
        for (let i = 0; i < scriptElements.length; i++) {
            const elem = scriptElements.item(i);
            if (UIElement.hasElement(elem))
                continue;

            const scriptName = elem.getAttribute("data-content-script");
            if (!scriptName)
                continue;

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
        if (!this.__destroyCallbacks)
            throw "Page is destroyed.";

        this.__destroyCallbacks?.push(f);
    }

    attachDestroyElement(elem: UIElement) {
        this.attachDestroyFunc(() => { elem.destroy(); });
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