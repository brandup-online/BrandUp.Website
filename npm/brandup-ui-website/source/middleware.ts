import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AJAXMethod, AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import { Middleware, NavigateContext, StartContext, StopContext, SubmitContext, MiddlewareNext, BROWSER } from "@brandup/ui-app";
import { NavigationModel, NavigationEntry, WebsiteNavigateData } from "./common";
import { Page } from "./page";
import { scriptReplace } from "./helpers/script";
import { extractHashFromUrl } from "./helpers/url";
import { minWaitAsync } from "./helpers/wait";
import { WebsiteApplication } from "./app";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "page-reload";
const pageActionHeader = "page-action";
const pageLocationHeader = "page-location";
const pageReplaceHeader = "page-replace";
const navDataElemId = "nav-data";
const pageElemId = "page-content";

export const WEBSITE_MIDDLEWARE_NAME = "website-pages";
const DEFAULT_OPTIONS: PagesOptions = {
    navMinTime: 0,
    submitMinTime: 500
};

export class WebsiteMiddleware implements Middleware {
    readonly name: string = WEBSITE_MIDDLEWARE_NAME;
    readonly options: PagesOptions;
    private __queue?: AjaxQueue;
    private __current?: NavigationEntry;
    private __navCounter = 0;

    constructor(options: PagesOptions) {
        this.options = Object.assign(options, DEFAULT_OPTIONS);

        if (!this.options.pageTypes)
            this.options.pageTypes = {};
        if (!this.options.scripts)
            this.options.scripts = {};
    }

    get validationToken(): string | null { return this.__current?.model.validationToken || null; }
    get current(): NavigationEntry | undefined { return this.__current; }

    // Middleware members

    start(context: StartContext<WebsiteApplication>, next: MiddlewareNext) {
        const bodyElem = document.body;

        bodyElem.appendChild(this.__loaderElem = DOM.tag("div", { class: "bp-page-loader" }));
        this.__showNavigationProgress();

        if (allowHistory)
            window.addEventListener("popstate", (e: PopStateEvent) => this.__onPopState(context, e));

        bodyElem.addEventListener("invalid", (event: Event) => {
            event.preventDefault();

            const elem = event.target as HTMLElement;
            elem.classList.add("invalid");

            if (elem.hasAttribute("required"))
                elem.classList.add("invalid-required");
        }, true);

        bodyElem.addEventListener("change", (event: Event) => {
            const elem = event.target as HTMLElement;
            elem.classList.remove("invalid");
            elem.classList.remove("invalid-required");
        });

        this.__queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (context.app.model.antiforgery && options.method !== "GET" && options.method && this.__current)
                    options.headers[context.app.model.antiforgery.headerName] = this.__current.model.validationToken;
            }
        });

        return next();
    }

    async navigate(context: NavigateContext<WebsiteApplication, WebsiteNavigateData>, next: MiddlewareNext) {
        if (!this.__queue)
            throw new Error('Website is not initialized.');

        const current = context.data.current = this.__current;

        const navSequence = this.__incNavSequence();
        this.__showNavigationProgress();
        this.__queue.reset(true);

        if (context.external || !allowHistory) {
            this.__forceNav(context);
            return;
        }

        if (current && (current.hash || context.hash) && current.url.toLowerCase() === context.url.toLowerCase()) {
            const isHashEqual = current.hash?.toLowerCase() === context.hash?.toLowerCase();

            if (!isHashEqual && !context.data.popstate) {
                this.__hideNavigationProgress();

                const newHash = context.hash ? "#" + context.hash : "";
                console.log(`nav to hash: ${newHash}`);
                location.hash = newHash;
                return;
            }

            if (current.hash && !context.hash)
                console.log(`remove hash: ${current.hash}`);
            else if (!current.hash && context.hash)
                console.log(`add hash: ${context.hash}`);
            else if (!isHashEqual)
                console.log(`change hash: ${current.hash} > ${context.hash}`);
            else
                console.log(`no change hash: ${current.hash} == ${context.hash}`);

            current.hash = context.hash;

            try {
                await current.page.__changedHash(context.hash, current.hash);

                await next();
            }
            finally {
                this.__hideNavigationProgress();
            }

            return;
        }

        const isFirst = context.source === "first";

        try {
            let navModel: NavigationModel;
            let navContent: DocumentFragment | null = null;

            if (isFirst) {
                // first navigation

                const navScriptElement = <HTMLScriptElement>document.getElementById(navDataElemId);
                if (!navScriptElement)
                    throw new Error('Not found first navigation data.');

                navModel = JSON.parse(navScriptElement.text);
                navScriptElement.remove();
            }
            else {
                // continue navigation

                const response: AjaxResponse = await minWaitAsync(this.__queue.enque({
                    method: "GET", url: context.url, query: { "_": navSequence.toString() },
                    headers: { "page-nav": current?.model.state || "" },
                    disableCache: true
                }), this.options.navMinTime);

                if (this.__isNavOutdated(navSequence))
                    return;

                if (response.status != 200 && response.type != "html") {
                    console.warn(`Nav request response status ${response.status}`);
                    this.__forceNav(context);
                    return;
                }

                if (response.headers.has(pageReloadHeader)) {
                    this.__forceNav(context);
                    return;
                }

                if (this.__precessPageResponse(context, response))
                    return;

                if (response.type != "html")
                    throw new Error('Nav response is not html.');

                navContent = document.createDocumentFragment();
                const fixElem = DOM.tag("div");
                navContent.append(fixElem);
                fixElem.insertAdjacentHTML("beforebegin", response.data);
                fixElem.remove();

                const navJsonElem = <HTMLScriptElement>navContent.getElementById(navDataElemId);
                navModel = JSON.parse(navJsonElem.text);
                navJsonElem.remove();

                if (current && current.model.isAuthenticated !== navModel.isAuthenticated) {
                    this.__forceNav(context);
                    return;
                }
            }

            await this.__renderPage(context, current, navModel, navSequence, navContent);

            await next();
        }
        catch (reason) {
            if (!isFirst && !this.__isNavOutdated(navSequence)) {
                this.__forceNav(context);
                return;
            }

            throw reason;
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    async submit(context: SubmitContext<WebsiteApplication>, next: MiddlewareNext) {
        if (!this.__current)
            throw new Error('Unable to submit.');

        const { url, form } = context;
        const method = (context.method.toUpperCase() as AJAXMethod);

        const current = context.data.current = this.__current;

        const navSequence = this.__incNavSequence();
        this.__showNavigationProgress();
        current.page.queue.reset(true);

        try {
            var query: { [key: string]: string | string[]; } = {};
            for (var key in current.model.query)
                query[key] = current.model.query[key];

            const response: AjaxResponse = await minWaitAsync(current.page.queue.enque({
                method, url, query,
                headers: { "page-nav": current.model.state || "", "page-submit": "true" },
                data: new FormData(form)
            }), this.options.submitMinTime);

            if (this.__isNavOutdated(navSequence))
                return;

            switch (response.status) {
                case 200:
                case 201:
                    break;
                default:
                    throw new Error(`Submit request response status ${response.status}`);
            }

            if (this.__precessPageResponse(context, response))
                return;

            if (response.type == "html") {
                if (!response.data)
                    throw new Error('Submit response not have html.');

                const contentFragment = document.createDocumentFragment();
                const fixElem = DOM.tag("div");
                contentFragment.append(fixElem);
                fixElem.insertAdjacentHTML("beforebegin", response.data);
                fixElem.remove();

                await this.__renderPage(context, current, null, navSequence, contentFragment);
            }
            else
                await current.page.__submitted(response);

            await next();
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    stop(context: StopContext<WebsiteApplication>, next: MiddlewareNext) {
        context.data.current = this.__current;

        return next();
    }

    // WebsiteMiddleware members

    private __precessPageResponse(context: NavigateContext, response: AjaxResponse): boolean {
        const pageAction = response.headers.get(pageActionHeader);
        if (pageAction) {
            switch (pageAction) {
                case "reset":
                case "reload": {
                    BROWSER.default.location.reload();
                    return true;
                }
                default:
                    throw "Неизвестный тип действия для страницы.";
            }
        }

        const redirectUrl = response.headers.get(pageLocationHeader);
        if (redirectUrl) {
            const replace = response.headers.has(pageReplaceHeader);

            if (response.headers.has(pageReloadHeader)) {
                if (replace)
                    BROWSER.default.location.replace(redirectUrl);
                else
                    BROWSER.default.location.assign(redirectUrl);
            }
            else
                context.app.nav({ url: redirectUrl, replace });

            return true;
        }

        return false;
    }

    private __forceNav(context: NavigateContext) {
        if (context.replace && !context.external)
            BROWSER.default.location.replace(context.url);
        else
            BROWSER.default.location.assign(context.url);
    }

    private async __renderPage(context: NavigateContext<WebsiteApplication>, current: NavigationEntry | undefined, newNav: NavigationModel | null, navSequence: number, newContent: DocumentFragment | null) {
        const nav = newNav || current?.model;
        if (!nav)
            throw new Error('Not set nav.');

        let pageTypeName: string | null = nav.page.type;
        if (!pageTypeName && this.options.defaultType)
            pageTypeName = this.options.defaultType;

        let pageDefinition: ScriptDefinition | null = null;

        if (pageTypeName) {
            pageDefinition = this.options && this.options.pageTypes ? this.options.pageTypes[pageTypeName] : null;
            if (!pageDefinition)
                throw `Not found page definition "${pageTypeName}".`;
        }
        else
            pageDefinition = { factory: () => Promise.resolve({ default: Page }) };

        const pageType: { default: typeof Page } = await pageDefinition.factory();

        if (this.__isNavOutdated(navSequence))
            return;

        let currentPageElem: HTMLElement | null;
        let newPageElem: HTMLElement;
        if (newContent !== null) {
            // replace page content

            currentPageElem = current?.page.element || null;

            newPageElem = <HTMLElement>newContent.getElementById(pageElemId);
            if (!newPageElem)
                throw new Error("Not found page element.");
        }
        else {
            currentPageElem = null;

            const elem = document.getElementById(pageElemId);
            if (!elem)
                throw new Error("Not found page element.");
            newPageElem = elem;
        }

        if (current?.page)
            current.page.destroy();

        let page: Page | undefined;
        try {
            page = new pageType.default(context.app, nav);
            await page.__render(newPageElem, current?.hash);

            if (this.__isNavOutdated(navSequence))
                throw new Error('Page is outdated.');

            this.__refreshPageScripts(page);

            if (newNav)
                this.__setNavigation(context, current, newNav, page);
            else if (current)
                current.page = page;
        }
        catch (reason) {
            if (page)
                page.destroy();

            throw reason;
        }

        if (currentPageElem) {
            currentPageElem.replaceWith(newPageElem);
            currentPageElem.remove();

            scriptReplace(newPageElem);
        }

        return page;
    }

    private __refreshPageScripts(page: Page) {
        if (!page.element)
            return;

        DOM.queryElements(page.element, "[data-content-script]").forEach(elem => {
            if (UIElement.hasElement(elem))
                return;

            const scriptName = elem.getAttribute("data-content-script");
            if (!scriptName)
                return;

            const script = this.__getScript(scriptName);
            if (script) {
                script.then((t) => {
                    const uiElem: UIElement = new t.default(elem, this);
                    page.onDestroy(uiElem);
                });
            }
        });
    }

    private __getScript(name: string): Promise<{ default: UIElement | any }> | null {
        if (!this.options.scripts)
            return null;

        const scriptFunc = this.options.scripts[name];
        if (!scriptFunc)
            return null;

        return scriptFunc.factory();
    }

    private __setNavigation(context: NavigateContext, current: NavigationEntry | undefined, newNav: NavigationModel, page: Page) {
        let navUrl = context.url;

        const isFirst = context.source == "first";
        const fromPopstate = !!context.data.popstate;
        const title = newNav.title || "";

        if (!isFirst) {
            let metaDescription = document.getElementById("page-meta-description");
            if (newNav.description) {
                if (!metaDescription)
                    document.head.appendChild(metaDescription = DOM.tag("meta", { id: "page-meta-description", name: "description", content: "" }));

                metaDescription.setAttribute("content", newNav.description);
            }
            else if (metaDescription)
                metaDescription.remove();

            let metaKeywords = document.getElementById("page-meta-keywords");
            if (newNav.keywords) {
                if (!metaKeywords)
                    document.head.appendChild(metaKeywords = DOM.tag("meta", { id: "page-meta-keywords", name: "keywords", content: "" }));

                metaKeywords.setAttribute("content", newNav.keywords);
            }
            else if (metaKeywords)
                metaKeywords.remove();

            let linkCanonical = document.getElementById("page-link-canonical");
            if (newNav.canonicalLink) {
                if (!linkCanonical)
                    document.head.appendChild(linkCanonical = DOM.tag("link", { id: "page-link-canonical", rel: "canonical", href: "" }));

                linkCanonical.setAttribute("href", newNav.canonicalLink);
            }
            else if (linkCanonical)
                linkCanonical.remove();

            this.__setOpenGraphProperty("type", newNav.openGraph?.type);
            this.__setOpenGraphProperty("title", newNav.openGraph?.title);
            this.__setOpenGraphProperty("image", newNav.openGraph?.image);
            this.__setOpenGraphProperty("url", newNav.openGraph?.url);
            this.__setOpenGraphProperty("site_name", newNav.openGraph?.siteName);
            this.__setOpenGraphProperty("description", newNav.openGraph?.description);

            if (current && current.model.bodyClass)
                document.body.classList.remove(current.model.bodyClass);

            if (newNav.bodyClass)
                document.body.classList.add(newNav.bodyClass);
        }

        this.__current = {
            context,
            url: context.url,
            hash: context.hash,
            model: newNav,
            page
        };

        let replace = context.replace;
        if (isFirst || navUrl === location.href)
            replace = true;

        if (!isFirst && !fromPopstate) {
            if (!context.hash) {
                if (replace)
                    window.history.replaceState(window.history.state, title, navUrl);
                else
                    window.history.pushState(window.history.state, title, navUrl);
            }

            if (context.hash)
                location.hash = "#" + context.hash;

            document.title = title;

            if (!replace)
                window.scrollTo({ left: 0, top: 0, behavior: "auto" });
        }
    }

    private __setOpenGraphProperty(name: string, value: string | null | undefined) {
        let metaTagElem = document.getElementById(`og-${name}`);
        if (value) {
            if (!metaTagElem)
                document.head.appendChild(metaTagElem = DOM.tag("meta", { id: `og-${name}`, property: name, content: value }));

            metaTagElem.setAttribute("content", value);
        }
        else if (metaTagElem)
            metaTagElem.remove();
    }

    private __onPopState(context: StartContext, event: PopStateEvent) {
        event.preventDefault();

        console.log(`popstate: ${location.href}`);

        context.app.nav({ data: { popstate: true } });
    }

    private __incNavSequence() {
        this.__navCounter++;
        return this.__navCounter;
    }
    private __isNavOutdated(navSequence: number) {
        return this.__navCounter !== navSequence;
    }

    private __loaderElem: HTMLElement | null = null;
    private __progressInterval: number = 0;
    private __progressTimeout: number = 0;
    private __progressStart: number = 0;
    private __showNavigationProgress() {
        window.clearTimeout(this.__progressTimeout);
        window.clearTimeout(this.__progressInterval);

        if (!this.__loaderElem)
            return;

        this.__loaderElem.classList.remove("show", "show2", "finish");
        this.__loaderElem.style.width = "0%";

        window.setTimeout(() => {
            if (!this.__loaderElem)
                return;

            this.__loaderElem.classList.add("show");
            this.__loaderElem.style.width = "70%";
        }, 10);

        this.__progressTimeout = window.setTimeout(() => {
            if (!this.__loaderElem)
                return;

            this.__loaderElem.classList.add("show");
            this.__loaderElem.style.width = "100%";
        }, 1700);

        this.__progressStart = Date.now();
    }

    private __hideNavigationProgress() {
        let d = 500 - (Date.now() - this.__progressStart);
        if (d < 0)
            d = 0;

        window.clearTimeout(this.__progressTimeout);
        this.__progressTimeout = window.setTimeout(() => {
            window.clearTimeout(this.__progressInterval);

            if (!this.__loaderElem)
                return;

            this.__loaderElem.classList.add("finish");
            this.__loaderElem.style.width = "100%";

            this.__progressInterval = window.setTimeout(() => {
                if (!this.__loaderElem)
                    return;

                this.__loaderElem.classList.remove("show", "finish");
                this.__loaderElem.style.width = "0%";
            }, 180);
        }, d);
    }
}

export interface PagesOptions {
    defaultType?: string;
    pageTypes?: { [key: string]: PageDefinition };
    scripts?: { [key: string]: ScriptDefinition };
    navMinTime?: number;
    submitMinTime?: number;
}

export interface PageDefinition {
    factory: () => Promise<{ default: typeof Page } | any>;
    preload?: boolean;
}

export interface ScriptDefinition {
    factory: () => Promise<{ default: typeof UIElement } | any>;
    preload?: boolean;
}