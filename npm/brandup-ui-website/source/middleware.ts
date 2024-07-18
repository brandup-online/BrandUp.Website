import { AJAXMethod, AjaxQueue, AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { Middleware, ApplicationModel, NavigateContext, NavigationOptions, StartContext, StopContext, SubmitContext, InvokeContext, Application } from "brandup-ui-app";
import { DOM } from "brandup-ui-dom";
import { NavigationModel, AntiforgeryOptions, WebsiteContext } from "./common";
import { Page } from "./page";
import { minWait } from "./utilities/wait";
import { scriptReplace } from "./utilities/script";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "page-reload";
const pageActionHeader = "page-action";
const pageLocationHeader = "page-location";
const pageReplaceHeader = "page-replace";
const navDataElemId = "nav-data";
const pageElemId = "page-content";

export class WebsiteMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> implements WebsiteContext {
    readonly options: WebsiteOptions;
    readonly antiforgery: AntiforgeryOptions;
    private __page: Page | null = null;
    private __navCounter = 0;
    private __currentUrl: UrlParsed | null = null;
    private __navigation: NavigationModel | null = null;
    readonly queue: AjaxQueue;

    get id(): string { return this.app.model.websiteId; }
    get validationToken(): string | null { return this.__navigation?.validationToken || null; }

    constructor(options: WebsiteOptions, antiforgery: AntiforgeryOptions) {
        super();

        this.options = options;
        this.antiforgery = antiforgery;

        if (!this.options.pageTypes)
            this.options.pageTypes = {};
        if (!this.options.scripts)
            this.options.scripts = {};

        this.queue = new AjaxQueue({
            canRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (this.antiforgery && options.method !== "GET" && options.method && this.__navigation)
                    options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;
            }
        });
    }

    // Middleware members

    start(context: StartContext, next: VoidFunction) {
        context.data["website"] = this;

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
        context.data["website"] = this;

        next();
    }

    async navigate(context: NavigateContext) {
        context.data["website"] = this;

        this.__showNavigationProgress();
        this.queue.reset(true);
        const navSequence = this.__incNavSequence();

        if (context.external || !allowHistory) {
            this.__forceNav(context);
            return false;
        }

        try {
            let navModel: NavigationModel;
            let navContent: DocumentFragment | null = null;

            if (context.source === "first") {
                // first navigation

                const navScriptElement = <HTMLScriptElement>document.getElementById(navDataElemId);
                if (!navScriptElement)
                    throw 'Not found first navigation data.';

                navModel = JSON.parse(navScriptElement.text);
                navScriptElement.remove();
            }
            else {
                // continue navigation

                navContent = await this.__requestNav(context, navSequence, this.__navigation?.state);
                if (!navContent)
                    return;

                const navJsonElem = <HTMLScriptElement>navContent.getElementById(navDataElemId);
                navModel = JSON.parse(navJsonElem.text);
                navJsonElem.remove();
            }

            await this.__renderPage(context, navModel, navSequence, navContent);
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    async submit(context: SubmitContext) {
        if (!this.__navigation || !this.__page)
            throw 'Unable to submit.';

        context.data["website"] = this;

        this.__showNavigationProgress();
        this.queue.reset(true);

        const navSequence = this.__incNavSequence();
        const currentNav = this.__navigation;
        const currentPage = this.__page;

        try {
            const response = await this.__requestSubmit(context, navSequence, currentNav);
            if (!response)
                return false;

            switch (response.type) {
                case "html":
                    if (!response.data)
                        throw 'Submit response not have html.';

                    await this.__updateHtml(context, navSequence, response.data);
                    break;
                default:
                    currentPage.formSubmitted(response);
                    break;
            }
        }
        finally {
            this.__hideNavigationProgress();
        }
    }

    stop(context: StopContext, next: VoidFunction) {
        context.data["website"] = this;
        context.data["nav"] = this.__navigation;
        context.data["page"] = this.__page;

        next();
    }

    // WebsiteContext members

    request(options: AjaxRequest, includeAntiforgery = true) {
        if (!options.headers)
            options.headers = {};

        if (includeAntiforgery && this.antiforgery && options.method !== "GET" && this.__navigation)
            options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;

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

    private __requestNav(context: NavigateContext, navSequence: number, state: string | null | undefined) {
        return new Promise<DocumentFragment | null>((resolve, reject) => {
            this.queue.push({
                method: "GET",
                url: context.url,
                headers: { "page-nav": state || "" },
                disableCache: true,
                success: (response: AjaxResponse<string>) => {
                    if (this.__isNavOutdated(navSequence)) {
                        reject("Nav request is outdated.");
                        resolve(null);
                        return;
                    }

                    switch (response.status) {
                        case 0:
                            break;
                        case 200: {
                            if (response.headers.has(pageReloadHeader)) {
                                this.__forceNav(context);
                                resolve(null);
                                return;
                            }

                            if (this.__precessPageResponse("nav", response, () => resolve(null)))
                                return;

                            if (!response.data) {
                                this.__forceNav(context);
                                reject('Nav response is empty.');
                                return;
                            }

                            const contentFragment = document.createDocumentFragment();
                            const fixElem = DOM.tag("div");
                            contentFragment.append(fixElem);
                            fixElem.insertAdjacentHTML("beforebegin", response.data);
                            fixElem.remove();

                            resolve(contentFragment);
                            break;
                        }
                        default:
                            this.__forceNav(context);
                            reject(`Nav request response status ${response.status}`);
                            break;
                    }
                },
                error: () => { resolve(null); }
            });
        });
    }

    private __requestSubmit(context: SubmitContext, navSequence: number, currentNav: NavigationModel) {
        const { url, form } = context;
        const method = (context.method.toUpperCase() as AJAXMethod);

        return new Promise<AjaxResponse | null>((resolve, reject) => {
            var urlParams: { [key: string]: string | string[]; } = {};
            for (var key in currentNav.query)
                urlParams[key] = currentNav.query[key];

            this.queue.push({
                method,
                url,
                query: urlParams,
                headers: {
                    "page-nav": currentNav.state || "",
                    "page-submit": "true"
                },
                data: new FormData(form),
                success: minWait((response: AjaxResponse) => {
                    if (this.__isNavOutdated(navSequence)) {
                        reject("Submit request is outdated.");
                        return;
                    }

                    switch (response.status) {
                        case 200:
                        case 201: {
                            if (this.__precessPageResponse("submit", response, () => { resolve(null) }))
                                break;

                            resolve(response);
                            break;
                        }
                        default:
                            reject(`Submit request response status ${response.status}`);
                            break;
                    }
                }),
                error: (reason) => { reject(`Submit request is error: ${reason}`); }
            });
        });
    }

    private async __updateHtml(context: SubmitContext, navSequence: number, html: string) {
        if (!this.__navigation)
            throw "Can't update page html.";

        const contentFragment = document.createDocumentFragment();
        const fixElem = DOM.tag("div");
        contentFragment.append(fixElem);
        fixElem.insertAdjacentHTML("beforebegin", html);
        fixElem.remove();

        context.data["submit"] = true;

        await this.__renderPage(<any>context, this.__navigation, navSequence, contentFragment);
    }

    private async __renderPage(context: NavigateContext, nav: NavigationModel, navSequence: number, newContent: DocumentFragment | null) {
        if (this.__isNavOutdated(navSequence))
            return false;

        const isSubmit = !!context.data["submit"];
        const prevNav = this.__navigation;

        context.data["prevNav"] = prevNav;
        context.data["prevPage"] = this.__page;

        try {
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

            if (this.__page) {
                // destroy current page

                this.__page.destroy();
                this.__page = null;
            }

            const pageType = await pageFactory();

            if (this.__isNavOutdated(navSequence))
                return false;

            const page = await this.__createPage(pageType.default, nav, newContent);
            if (!isSubmit)
                this.__setNavigation(nav, context.hash, context.replace);

            this.__page = page;
            page.render(this.__currentUrl ? this.__currentUrl.hash : null);

            context.data["nav"] = nav;
            context.data["page"] = page;

            return true;
        }
        catch (reason) {
            if (prevNav && !isSubmit)
                this.__forceNav(context);

            throw reason;
        }
    }

    private __createPage(pageType: typeof Page, nav: NavigationModel, newContent: DocumentFragment | null): Promise<Page> {
        return new Promise<Page>(resolve => {
            let pageElem = document.getElementById(pageElemId);
            if (!pageElem)
                throw "Not found page element.";

            if (newContent !== null) {
                const newPageElem = <HTMLElement>newContent.getElementById(pageElemId);
                if (!newPageElem)
                    throw "Not found page element.";

                pageElem.replaceWith(newPageElem);
                pageElem.remove();

                pageElem = newPageElem;

                scriptReplace(pageElem);
            }

            const page = new pageType(this, nav, pageElem);

            resolve(page);
        });
    }

    private __forceNav(context: NavigateContext) {
        if (context.replace && !context.external)
            location.replace(context.url);
        else
            location.assign(context.url);
    }

    private __setNavigation(data: NavigationModel, hash: string | null, replace: boolean) {
        let navUrl = data.url;
        if (hash)
            navUrl += "#" + hash;

        const prevNav = this.__navigation;

        if (prevNav) {
            if (prevNav.isAuthenticated !== data.isAuthenticated) {
                location.href = navUrl;
                return;
            }

            document.title = data.title || "";

            let metaDescription = document.getElementById("page-meta-description");
            if (data.description) {
                if (!metaDescription) {
                    document.head.appendChild(metaDescription = DOM.tag("meta", { id: "page-meta-description", name: "description", content: "" }));
                }

                metaDescription.setAttribute("content", data.description);
            }
            else if (metaDescription)
                metaDescription.remove();

            let metaKeywords = document.getElementById("page-meta-keywords");
            if (data.keywords) {
                if (!metaKeywords) {
                    document.head.appendChild(metaKeywords = DOM.tag("meta", { id: "page-meta-keywords", name: "keywords", content: "" }));
                }

                metaKeywords.setAttribute("content", data.keywords);
            }
            else if (metaKeywords)
                metaKeywords.remove();

            let linkCanonical = document.getElementById("page-link-canonical");
            if (data.canonicalLink) {
                if (!linkCanonical) {
                    document.head.appendChild(linkCanonical = DOM.tag("link", { id: "page-link-canonical", rel: "canonical", href: "" }));
                }

                linkCanonical.setAttribute("href", data.canonicalLink);
            }
            else if (linkCanonical)
                linkCanonical.remove();

            this.__setOpenGraphProperty("type", data.openGraph ? data.openGraph.type : null);
            this.__setOpenGraphProperty("title", data.openGraph ? data.openGraph.title : null);
            this.__setOpenGraphProperty("image", data.openGraph ? data.openGraph.image : null);
            this.__setOpenGraphProperty("url", data.openGraph ? data.openGraph.url : null);
            this.__setOpenGraphProperty("site_name", data.openGraph ? data.openGraph.siteName : null);
            this.__setOpenGraphProperty("description", data.openGraph ? data.openGraph.description : null);

            document.body.classList.remove(prevNav.bodyClass);
        }

        this.__navigation = data;
        this.__currentUrl = { url: data.url, hash };

        if (this.__navigation.bodyClass)
            document.body.classList.add(this.__navigation.bodyClass);

        if (navUrl === location.href)
            replace = true;

        if (!hash) {
            if (!replace)
                window.history.pushState(window.history.state, data.title, navUrl);
            else
                window.history.replaceState(window.history.state, data.title, navUrl);
        }

        if (!replace)
            window.scrollTo({ left: 0, top: 0, behavior: "auto" });
    }

    private __setOpenGraphProperty(name: string, value: string | null) {
        let metaTagElem = document.getElementById(`og-${name}`);
        if (value) {
            if (!metaTagElem) {
                document.head.appendChild(metaTagElem = DOM.tag("meta", { id: `og-${name}`, property: name, content: value }));
            }

            metaTagElem.setAttribute("content", value);
        }
        else if (metaTagElem)
            metaTagElem.remove();
    }

    private __precessPageResponse(source: "nav" | "submit", response: AjaxResponse, end: () => void): boolean {
        const pageAction = response.headers.get(pageActionHeader);
        if (pageAction) {
            switch (pageAction) {
                case "reset":
                case "reload": {
                    end();
                    location.reload();
                    return true;
                }
                default:
                    throw "Неизвестный тип действия для страницы.";
            }
        }

        const redirectLocation = response.headers.get(pageLocationHeader);
        if (redirectLocation) {
            const replace = response.headers.has(pageReplaceHeader);

            if (response.headers.has(pageReloadHeader)) {
                if (replace)
                    location.replace(redirectLocation);
                else
                    location.assign(redirectLocation);
            }
            else {
                this.nav({
                    url: redirectLocation,
                    replace,
                    context: { source },
                    callback: () => { end(); }
                });
            }

            return true;
        }

        return false;
    }

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        const url = location.href;
        const oldUrl = this.__currentUrl;
        const newUrl = this.__currentUrl = this.__extractHashFromUrl(url);

        if (oldUrl) {
            if (oldUrl.hash && !newUrl.hash) {
                console.log(`remove hash: ${oldUrl.hash}`);
            }
            else if (!oldUrl.hash && newUrl.hash) {
                console.log(`add hash: ${newUrl.hash}`);
            }
            else if (oldUrl.hash && newUrl.hash) {
                console.log(`change hash: ${newUrl.hash}`);
            }

            if ((oldUrl.hash || newUrl.hash) && oldUrl.url.toLowerCase() === newUrl.url.toLowerCase()) {
                this.__page?.changedHash(newUrl.hash, oldUrl.hash);
                return;
            }
        }

        console.log("PopState: " + url);

        this.app.nav({
            url: url,
            replace: true,
            context: {
                state: event.state
            }
        });
    }
    private __extractHashFromUrl(url: string): UrlParsed {
        const hashIndex = url.lastIndexOf("#");
        if (hashIndex > 0)
            return {
                url: url.substring(0, hashIndex),
                hash: url.substring(hashIndex + 1)
            };
        return { url, hash: null };
    }

    private __incNavSequence() {
        this.__navCounter++;
        return this.__navCounter;
    }
    private __isNavOutdated(navSequence: number) {
        return navSequence !== this.__navCounter;
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

interface UrlParsed {
    url: string;
    hash: string | null;
}

export interface WebsiteOptions {
    defaultType?: string;
    pageTypes?: { [key: string]: () => Promise<{ default: typeof Page } | any> };
    scripts?: { [key: string]: () => Promise<{ default: typeof Page } | any> };
}