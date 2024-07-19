import { AJAXMethod, AjaxQueue, AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { Middleware, ApplicationModel, NavigateContext, NavigationOptions, StartContext, StopContext, SubmitContext, InvokeContext, Application } from "brandup-ui-app";
import { DOM } from "brandup-ui-dom";
import { NavigationModel, AntiforgeryOptions, WebsiteContext, NavigationEntry, WebsiteNavigateData } from "./common";
import { Page } from "./page";
import { scriptReplace } from "./helpers/script";
import { extractHashFromUrl } from "./helpers/url";
import { minWaitAsync } from "helpers/wait";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "page-reload";
const pageActionHeader = "page-action";
const pageLocationHeader = "page-location";
const pageReplaceHeader = "page-replace";
const navDataElemId = "nav-data";
const pageElemId = "page-content";

const DEFAULT_OPTIONS: WebsiteOptions = {
    navMinTime: 0,
    submitMinTime: 500
};

export class WebsiteMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> implements WebsiteContext {
    readonly options: WebsiteOptions;
    readonly antiforgery: AntiforgeryOptions;
    readonly queue: AjaxQueue;
    private __current?: NavigationEntry;
    private __navCounter = 0;

    constructor(options: WebsiteOptions, antiforgery: AntiforgeryOptions) {
        super();

        this.options = Object.assign(options, DEFAULT_OPTIONS);
        this.antiforgery = antiforgery;

        if (!this.options.pageTypes)
            this.options.pageTypes = {};
        if (!this.options.scripts)
            this.options.scripts = {};

        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (this.antiforgery && options.method !== "GET" && options.method && this.__current)
                    options.headers[this.antiforgery.headerName] = this.__current.model.validationToken;
            }
        });
    }

    // Middleware members

    start(context: StartContext, next: VoidFunction) {
        context.data.website = this;

        const bodyElem = document.body;

        bodyElem.appendChild(this.__loaderElem = DOM.tag("div", { class: "bp-page-loader" }));
        this.__showNavigationProgress();

        if (allowHistory)
            window.addEventListener("popstate", (e: PopStateEvent) => this.__onPopState(e));

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

        next();
    }

    loaded(context: StartContext, next: VoidFunction) {
        context.data.website = this;

        next();
    }

    async navigate(context: NavigateContext<WebsiteNavigateData>) {
        context.data.website = this;
        const current = context.data.current = this.__current;

        this.__showNavigationProgress();
        this.queue.reset(true);
        const navSequence = this.__incNavSequence();

        if (context.external || !allowHistory) {
            this.__forceNav(context);
            return false;
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

                const response: AjaxResponse = await minWaitAsync(this.queue.enque({
                    method: "GET", url: context.url, query: { "_": navSequence.toString() },
                    headers: { "page-nav": current?.model.state || "" },
                    disableCache: true
                }), this.options.navMinTime);

                if (this.__isNavOutdated(navSequence))
                    return false;

                if (response.status != 200) {
                    console.warn(`Nav request response status ${response.status}`);
                    this.__forceNav(context);
                    return false;
                }

                if (response.headers.has(pageReloadHeader)) {
                    this.__forceNav(context);
                    return false;
                }

                if (this.__precessPageResponse(context, response))
                    return false;

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
                    return false;
                }
            }

            await this.__renderPage(context, current, navModel, navSequence, navContent);
        }
        catch (reason) {
            if (!isFirst) {
                this.__forceNav(context);
                return false;
            }

            throw reason;
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    async submit(context: SubmitContext) {
        if (!this.__current)
            throw new Error('Unable to submit.');

        const { url, form } = context;
        const method = (context.method.toUpperCase() as AJAXMethod);

        context.data.website = this;
        const current = context.data.current = this.__current;

        this.__showNavigationProgress();
        this.queue.reset(true);
        const navSequence = this.__incNavSequence();

        try {
            var query: { [key: string]: string | string[]; } = {};
            for (var key in current.model.query)
                query[key] = current.model.query[key];

            const response: AjaxResponse = await minWaitAsync(this.queue.enque({
                method, url, query,
                headers: { "page-nav": current.model.state || "", "page-submit": "true" },
                data: new FormData(form)
            }), this.options.submitMinTime);

            if (this.__isNavOutdated(navSequence))
                return false;

            switch (response.status) {
                case 200:
                case 201:
                    break;
                default:
                    throw new Error(`Submit request response status ${response.status}`);
            }

            if (this.__precessPageResponse(context, response))
                return false;

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
                await current.page.formSubmitted(response);
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    stop(context: StopContext, next: VoidFunction) {
        context.data.website = this;
        context.data.current = this.__current;

        next();
    }

    // WebsiteContext members

    get id(): string { return this.app.model.websiteId; }
    get validationToken(): string | null { return this.__current?.model.validationToken || null; }
    get current(): NavigationEntry | undefined { return this.__current; }

    request(options: AjaxRequest, includeAntiforgery = true) {
        if (!options.headers)
            options.headers = {};

        if (includeAntiforgery && this.antiforgery && options.method !== "GET" && this.__current)
            options.headers[this.antiforgery.headerName] = this.__current.model.validationToken;

        this.queue.push(options);
    }

    buildUrl(path?: string, queryParams?: { [key: string]: string }): string {
        return this.app.uri(path, queryParams);
    }

    nav(options: NavigationOptions) {
        this.app.nav(options);
    }

    getScript(name: string): Promise<{ default: any }> | null {
        if (!this.options.scripts)
            return null;
        const scriptFunc = this.options.scripts[name];
        if (!scriptFunc)
            return null;
        return scriptFunc();
    }

    // WebsiteMiddleware members

    private __precessPageResponse(context: NavigateContext, response: AjaxResponse): boolean {
        const pageAction = response.headers.get(pageActionHeader);
        if (pageAction) {
            switch (pageAction) {
                case "reset":
                case "reload": {
                    location.reload();
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
                    location.replace(redirectUrl);
                else
                    location.assign(redirectUrl);
            }
            else
                this.nav({ url: redirectUrl, replace });

            return true;
        }

        return false;
    }

    private __forceNav(context: NavigateContext) {
        if (context.replace && !context.external)
            location.replace(context.url);
        else
            location.assign(context.url);
    }

    private async __renderPage(context: NavigateContext, current: NavigationEntry | undefined, newNav: NavigationModel | null, navSequence: number, newContent: DocumentFragment | null) {
        const nav = newNav || current?.model;
        if (!nav)
            throw new Error('Not set nav.');

        let pageTypeName: string | null = nav.page.type;
        if (!pageTypeName && this.options.defaultType)
            pageTypeName = this.options.defaultType;

        let pageFactory: (() => Promise<{ default: typeof Page } | any>) | null = null;

        if (pageTypeName) {
            pageFactory = this.options && this.options.pageTypes ? this.options.pageTypes[pageTypeName] : null;
            if (!pageFactory)
                throw `Not found page type "${pageTypeName}".`;
        }
        else
            pageFactory = () => new Promise<{ default: typeof Page } | any>(resolve => { resolve({ default: Page }); });

        const pageType: { default: typeof Page } = await pageFactory();

        if (this.__isNavOutdated(navSequence))
            return null;

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
            page = new pageType.default(this, nav);
            await page.render(newPageElem, current ? current.hash : null);

            if (this.__isNavOutdated(navSequence))
                throw new Error('');

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

    private __setNavigation(context: NavigateContext, current: NavigationEntry | undefined, newNav: NavigationModel, page: Page) {
        let navUrl = newNav.url;
        if (context.hash)
            navUrl += "#" + context.hash;

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
            url: newNav.url,
            hash: context.hash,
            model: newNav,
            page
        };

        let replace = context.replace;
        if (isFirst || navUrl === location.href)
            replace = true;

        if (!isFirst && !fromPopstate) {
            if (!replace)
                window.history.pushState(window.history.state, title, navUrl);
            else
                window.history.replaceState(window.history.state, title, navUrl);

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

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        const url = location.href;
        const current = this.__current;
        const newUrl = extractHashFromUrl(url);

        if (current) {
            if (current.hash && !newUrl.hash)
                console.log(`remove hash: ${current.hash}`);
            else if (!current.hash && newUrl.hash)
                console.log(`add hash: ${newUrl.hash}`);
            else if (current.hash && newUrl.hash)
                console.log(`change hash: ${newUrl.hash}`);

            if ((current.hash || newUrl.hash) && current.url.toLowerCase() === newUrl.url.toLowerCase()) {
                current.page.changedHash(newUrl.hash, current.hash);
                return;
            }
        }

        console.log(`popstate: ${url}`);

        this.app.nav({ url: url, data: { popstate: event.state } });
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

export interface WebsiteOptions {
    defaultType?: string;
    pageTypes?: { [key: string]: () => Promise<{ default: typeof Page } | any> };
    scripts?: { [key: string]: () => Promise<{ default: typeof Page } | any> };
    navMinTime?: number;
    submitMinTime?: number;
}