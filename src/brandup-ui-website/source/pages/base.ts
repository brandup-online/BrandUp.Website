import { UIElement, AjaxQueue, AjaxRequestOptions } from "brandup-ui";
import { PageModel, NavigationModel, AntiforgeryOptions } from "../common";
import { IApplication } from "brandup-ui-app";

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
            onPreRequest: (options) => {
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

    protected onRenderContent() {
        return;
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
    updateHtml(html: string);
    request(options: AjaxRequestOptions);
    buildUrl(queryParams: { [key: string]: string }): string;
}