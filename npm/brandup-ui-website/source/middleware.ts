import { AJAXMethod, AjaxQueue, AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { Middleware, ApplicationModel, NavigateContext, NavigationOptions, StartContext, LoadContext, StopContext, SubmitContext, InvokeContext, Application } from "brandup-ui-app";
import { DOM } from "brandup-ui-dom";
import { NavigationModel, AntiforgeryOptions, WebsiteContext } from "./common";
import { Page } from "./page";
import { minWait } from "./utilities/wait";
import { scriptReplace } from "./utilities/script";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "Page-Reload";
const pageActionHeader = "Page-Action";
const pageLocationHeader = "Page-Location";
const pageReplaceHeader = "Page-Replace";
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
            preRequest: (options) => {
                if (!options.headers)
                    options.headers = {};

                if (this.antiforgery && options.method !== "GET" && options.method && this.__navigation)
                    options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;
            }
        });
    }

    // Middleware members

    start(context: StartContext, next: () => void, end: () => void) {
        context["website"] = this;

        const bodyElem = document.body;

        bodyElem.appendChild(this.__loaderElem = DOM.tag("div", { class: "bp-page-loader" }));
        this.__showNavigationProgress();

        if (allowHistory)
            window.addEventListener("popstate", (e: PopStateEvent) => this.__onPopState(e));

        bodyElem.addEventListener("invalid", (event: Event) => {
            event.preventDefault();

            const elem = event.target as HTMLElement;
            elem.classList.add("invalid");

            if (elem.hasAttribute("data-val-required")) {
                elem.classList.add("invalid-required");
            }
        }, true);

        bodyElem.addEventListener("change", (event: Event) => {
            const elem = event.target as HTMLElement;
            elem.classList.remove("invalid");

            if (elem.hasAttribute("data-val-required")) {
                elem.classList.remove("invalid-required");
            }
        });

        super.start(context, next, end);
    }

    loaded(context: LoadContext, next: () => void, end: () => void) {
        context["website"] = this;

        super.loaded(context, next, end);
    }

    navigate(context: NavigateContext, next: () => void, end: () => void) {
        if (context.external) {
            if (context.context["source"] == "submit")
                this.__forceNav(context);
            else {
                this.__forceNav(context);
                //const linkElem = <HTMLLinkElement>DOM.tag("a", { href: context.url, target: "_blank" });
                //linkElem.click();
                //linkElem.remove();
            }
            
            end();
            return;
        }

        if (!allowHistory) {
            this.__forceNav(context);

            end();
            return;
        }

        context["website"] = this;

        this.__showNavigationProgress();
        this.queue.reset(true);

        const navSequence = this.__incNavSequence();

        if (!this.__navigation) {
            // first navigation

            const navScriptElement = <HTMLScriptElement>document.getElementById(navDataElemId);
            if (!navScriptElement)
                throw 'Not found first navigation data.';

            const navModel: NavigationModel = JSON.parse(navScriptElement.text);
            navScriptElement.remove();

            this.__renderPage(context, navModel, this.__navCounter, null, next, end);
        }
        else {
            // continue navigation

            this.queue.push({
                url: context.url,
                method: "GET",
                headers: { "Page-Nav": this.__navigation?.state ? this.__navigation.state : "" },
                disableCache: true,
                success: (response: AjaxResponse<string>) => {
                    if (this.__isNavOutdated(navSequence)) {
                        end();
                        return;
                    }

                    switch (response.status) {
                        case 200: {
                            if (response.xhr.getResponseHeader(pageReloadHeader) === "true") {
                                this.__forceNav(context);
                                end();
                                return;
                            }

                            if (this.__precessPageResponse("nav", response, end))
                            {
                                end();
                                return;
                            }

                            if (!response.data) {
                                this.__forceNav(context);
                                end();
                                return;
                            }

                            const contentFragment = document.createDocumentFragment();
                            const fixElem = DOM.tag("div");
                            contentFragment.append(fixElem);
                            fixElem.insertAdjacentHTML("beforebegin", response.data);
                            fixElem.remove();

                            const navJsonElem = <HTMLScriptElement>contentFragment.getElementById(navDataElemId);
                            const navModel = JSON.parse(navJsonElem.text);
                            navJsonElem.remove();

                            this.__renderPage(context, navModel, navSequence, contentFragment, next, end);

                            break;
                        }
                        default: {
                            this.__forceNav(context);
                            end();
                            break;
                        }
                    }
                }
            });
        }
    }

    submit(context: SubmitContext, next: () => void, end: () => void) {
        const { url, form } = context;
        const method = (context.method.toUpperCase() as AJAXMethod);

        if (!this.__navigation || !this.__page)
            throw 'Unable to submit.';

        context["website"] = this;

        this.queue.reset(true);
        this.__showNavigationProgress();

        const navSequence = this.__incNavSequence();
        const currentNav = this.__navigation;
        const currentPage = this.__page;

        const submitCallback = (response: AjaxResponse) => {
            if (this.__isNavOutdated(navSequence)) {
                end();
                return;
            }

            console.log(`form submited: ${method} ${url} ${response.status}`);

            switch (response.status) {
                case 200:
                case 201: {
                    if (this.__precessPageResponse("submit", response, end)) {
                        end();
                        break;
                    }

                    const contentType = response.xhr.getResponseHeader("content-type");
                    if (contentType && contentType.startsWith("text/html"))
                        this.__updateHtml(context, response.data, next, end);
                    else
                        currentPage.callbackHandler(response.data);

                    break;
                }
                default:
                    console.warn(`Submit response status ${response.status}`);
                    next();
                    break;
            }

            this.__hideNavigationProgress();
        };

        var urlParams: { [key: string]: string; } = {};
        for (var key in currentNav.query)
            urlParams[key] = currentNav.query[key];

        this.queue.push({
            url,
            urlParams,
            headers: {
                "Page-Nav": currentNav.state || "",
                "Page-Submit": "true"
            },
            method,
            data: new FormData(form),
            success: minWait(submitCallback)
        });
    }

    stop(context: StopContext, next) {
        context["website"] = this;
        context["nav"] = this.__navigation;
        context["page"] = this.__page;

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

    private __updateHtml(context: SubmitContext, html: string, next: () => void, end: () => void) {
        if (!this.__navigation)
            throw "Can't update page html.";

        const navSequence = this.__incNavSequence();

        const contentFragment = document.createDocumentFragment();
        const fixElem = DOM.tag("div");
        contentFragment.append(fixElem);
        fixElem.insertAdjacentHTML("beforebegin", html);
        fixElem.remove();

        context["submit"] = true;

        this.__renderPage(<any>context, this.__navigation, navSequence, contentFragment, next, end);
    }

    private __renderPage(context: NavigateContext, nav: NavigationModel, navSequence: number, newContent: DocumentFragment | null, next: () => void, end: () => void) {
        if (this.__isNavOutdated(navSequence)) {
            end();
            return;
        }

        const isSubmit = !!context["submit"];
        const prevNav = this.__navigation;

        context["prevNav"] = prevNav;
        context["prevPage"] = this.__page;

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

        pageFactory()
            .then((pageType: { default: typeof Page }) => {
                if (this.__isNavOutdated(navSequence))
                    throw '';

                return this.__createPage(pageType.default, nav, newContent);
            })
            .then(page => {
                if (!isSubmit)
                    this.__setNavigation(nav, context.hash, context.replace);

                this.__page = page;
                page.render(this.__currentUrl ? this.__currentUrl.hash : null);

                context["nav"] = nav;
                context["page"] = page;

                next();

                return page;
            })
            .catch((reason) => {
                console.error(`Error page ${context.url}: ${reason}`);

                if (prevNav && !isSubmit)
                    this.__forceNav(context);

                end();
            })
            .finally(() => this.__hideNavigationProgress());
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
        const pageAction = response.xhr.getResponseHeader(pageActionHeader);
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

        const redirectLocation = response.xhr.getResponseHeader(pageLocationHeader);
        if (redirectLocation) {
            end();

            this.nav({
                url: redirectLocation,
                replace: response.xhr.getResponseHeader(pageReplaceHeader) === "true",
                context: { source }
            });
            
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
    defaultType?: string | null;
    pageTypes?: { [key: string]: () => Promise<{ default: typeof Page } | any> } | null;
    scripts?: { [key: string]: () => Promise<any> } | null;
}