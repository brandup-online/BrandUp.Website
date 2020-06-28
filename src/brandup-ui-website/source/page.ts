import { UIElement, AjaxQueue } from "brandup-ui";
import { PageModel, NavigationModel } from "./common";
import { IApplication } from "brandup-ui-app";

export class Page<TModel extends PageModel> extends UIElement {
    readonly app: IApplication;
    readonly nav: NavigationModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];
    private __scripts: Array<UIElement> = [];

    constructor(app: IApplication, nav: NavigationModel, element: HTMLElement) {
        super();

        this.app = app;
        this.nav = nav;
        this.queue = new AjaxQueue({
            onPreRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (app.model.antiforgery && options.method !== "GET" && options.method)
                    options.headers[app.model.antiforgery.headerName] = nav.validationToken;
            }
        });
        this.setElement(element);

        this.onRenderContent();
    }

    get typeName(): string { return "BrandUp.Page"; }
    get model(): TModel { return this.nav.page as TModel; }

    protected onRenderContent() {
        return;
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

        return this.app.uri(this.nav.path, params);
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