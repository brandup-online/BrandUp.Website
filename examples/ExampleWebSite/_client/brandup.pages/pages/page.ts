import { UIElement, DOM, AjaxQueue } from "brandup-ui";
import { PageClientModel, PageNavState, IPage, IApplication } from "../typings/website";

class Page<TModel extends PageClientModel> extends UIElement implements IPage {
    private __nav: PageNavState;
    private __model: TModel;
    readonly app: IApplication;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];
    private __scripts: Array<UIElement> = [];

    constructor(app: IApplication, nav: PageNavState, model: TModel, element: HTMLElement) {
        super();

        this.app = app;
        this.__nav = nav;
        this.__model = model;
        this.queue = new AjaxQueue({
            onPreRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (app.model.antiforgery && options.method !== "GET" && options.method)
                    options.headers[app.model.antiforgery.headerName] = app.navigation.validationToken;
            }
        });
        this.setElement(element);

        if (this.__model.cssClass) {
            document.body.classList.add(this.__model.cssClass);
            this.attachDestroyFunc(() => { document.body.classList.remove(this.__model.cssClass); });
        }

        this.refreshScripts();

        this.onRenderContent();
    }

    get typeName(): string { return "BrandUpPages.Page"; }
    get nav(): PageNavState { return this.__nav; }
    get model(): TModel { return this.__model; }

    update(nav: PageNavState, model: TModel) {
        this.__nav = nav;
        this.__model = model;

        this.onUpdate(nav, model);
    }

    protected onRenderContent() {
        console.log("onRenderContent");
    }
    protected onUpdate(nav: PageNavState, model: TModel) {
        console.log("onUpdate");
    }

    buildUrl(queryParams: { [key: string]: any; }): string {
        const params: { [key: string]: string; } = {};
        for (const k in this.__nav.params) {
            params[k] = this.__nav.params[k];
        }

        if (queryParams) {
            for (const k in queryParams) {
                params[k] = queryParams[k];
            }
        }

        return this.app.uri(this.__nav.path, params);
    }

    attachDestroyFunc(f: () => void) {
        this.__destroyCallbacks.push(f);
    }
    attachDestroyElement(elem: UIElement) {
        this.__destroyCallbacks.push(() => { elem.destroy(); });
    }

    refreshScripts() {
        const scriptElements = DOM.queryElements(this.element, "[content-script]");
        for (let i = 0; i < scriptElements.length; i++) {
            const elem = scriptElements.item(i);
            if (elem.hasAttribute("brandup-ui-element"))
                continue;

            const scriptName = elem.getAttribute("content-script");
            const script = this.app.script(scriptName);
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

export default Page;