import { DOM, ajaxRequest, Utility, AjaxRequest, AjaxResponse, AJAXMethod, AjaxQueue } from "brandup-ui";
import { Middleware, ApplicationModel, NavigateContext, NavigationOptions, StartContext, LoadContext, StopContext, NavigatingContext, SubmitContext } from "brandup-ui-app";
import { NavigationModel, AntiforgeryOptions } from "../common";
import { Page, Website } from "../pages/base";
import minWait from "../utilities/wait";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "Page-Reload";
const pageActionHeader = "Page-Action";
const pageLocationHeader = "Page-Location";
const pageReplaceHeader = "Page-Replace";

export class WebsiteMiddleware extends Middleware<ApplicationModel> implements Website {
    readonly options: WebsiteOptions;
    readonly antiforgery: AntiforgeryOptions;
    private __contentBodyElem: HTMLElement;
    private __page: Page = null;
    private __navCounter = 0;
    private __currentUrl: UrlParsed = null;
    private __navigation: NavigationModel;
    private __loadingPage = false;
    readonly queue: AjaxQueue;

    get id(): string { return this.app.model.websiteId; }
    get visitorId(): string { return this.app.model.visitorId; }

    constructor(nav: NavigationModel, options: WebsiteOptions, antiforgery: AntiforgeryOptions) {
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

                if (this.antiforgery && options.method !== "GET" && options.method)
                    options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;
            }
        });

        this.setNavigation(nav, location.hash ? location.hash.substr(1) : null, false);
    }

    start(context: StartContext, next: () => void) {
        context.items["nav"] = this.__navigation;

        const bodyElem = document.body;

        bodyElem.appendChild(this.__loaderElem = DOM.tag("div", { class: "bp-page-loader" }));

        this.__contentBodyElem = document.getElementById("page-content");
        if (!this.__contentBodyElem)
            throw "Not found page content element.";

        if (allowHistory) {
            window.addEventListener("popstate", Utility.createDelegate(this, this.__onPopState));
        }

        bodyElem.addEventListener("invalid", (event: Event) => {
            event.preventDefault();

            const elem = event.target as HTMLInputElement;
            elem.classList.add("invalid");

            if (elem.hasAttribute("data-val-required")) {
                elem.classList.add("invalid-required");
            }
        }, true);
        bodyElem.addEventListener("change", (event: Event) => {
            const elem = event.target as HTMLInputElement;
            elem.classList.remove("invalid");

            if (elem.hasAttribute("data-val-required")) {
                elem.classList.remove("invalid-required");
            }
        });

        this.__renderPage(context.items, this.__navCounter, null, next);
    }
    loaded(context: LoadContext, next: () => void) {
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;

        next();
    }
    navigating(context: NavigatingContext, next) {
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;

        next();
    }
    navigate(context: NavigateContext, next: () => void) {
        if (!allowHistory) {
            location.href = context.fullUrl ? context.fullUrl : location.href;
            return;
        }

        this.__showNavigationProgress();

        context.items["prevNav"] = this.__navigation;

        this.__loadingPage = true;
        const navSequence = this.__incNavSequence();

        this.queue.reset(true);
        this.queue.push({
            url: context.url,
            method: "POST",
            urlParams: { _nav: "" },
            type: "TEXT",
            data: this.__navigation.state ? this.__navigation.state : "",
            success: (response: AjaxResponse<NavigationModel>) => {
                if (this.__isNavOutdated(navSequence))
                    return;

                switch (response.status) {
                    case 200: {
                        if (this.__precessPageResponse(response))
                            return;

                        context.items["nav"] = response.data;

                        this.setNavigation(response.data, context.hash, context.replace);

                        if (response.xhr.getResponseHeader(pageReloadHeader) === "true") {
                            location.reload();
                            return;
                        }

                        this.__loadContent(context.items, navSequence, next);

                        break;
                    }
                    default: {
                        if (context.replace)
                            location.replace(context.fullUrl);
                        else
                            location.assign(context.fullUrl);
                        break;
                    }
                }
            }
        });
    }
    submit(context: SubmitContext, next: () => void) {
        const { form } = context;
        const url = form.action;
        const method = form.method ? (form.method.toUpperCase() as AJAXMethod) : "POST";

        const submitButton = DOM.queryElement(form, "[type=submit]");
        if (submitButton)
            submitButton.classList.add("loading");
        form.classList.add("loading");

        const navSequence = this.__incNavSequence();
        const submitCallback = (response: AjaxResponse) => {
            if (!this.__isNavOutdated(navSequence)) {
                console.log(`form submitted: ${method} ${url} ${response.status}`);

                switch (response.status) {
                    case 200:
                    case 201: {
                        if (this.__precessPageResponse(response))
                            return;

                        this.updateHtml(response.data);

                        break;
                    }
                    default: {
                        break;
                    }
                }

                if (submitButton)
                    submitButton.classList.remove("loading");
                form.classList.remove("loading");
            }

            next();

            this.__hideNavigationProgress();
        };

        this.__showNavigationProgress();

        console.log(`form submitting: ${method} ${url}`);

        const handler = form.getAttribute("data-form-handler");

        this.queue.reset(true);
        this.queue.push({
            url,
            urlParams: { _content: "", handler },
            method,
            data: new FormData(form),
            success: submitButton ? minWait(submitCallback) : submitCallback
        });
    }
    stop(context: StopContext, next) {
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;

        next();
    }

    request(options: AjaxRequest, includeAntiforgery = true) {
        if (!options.headers)
            options.headers = {};

        if (includeAntiforgery && this.antiforgery && options.method !== "GET")
            options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;

        ajaxRequest(options);
    }
    updateHtml(html: string) {
        const navSequence = this.__incNavSequence();

        this.__renderPage({}, navSequence, html, () => { return; });
    }
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string {
        return this.app.uri(path, queryParams);
    }
    nav(options: NavigationOptions) {
        this.app.nav(options);
    }
    getScript(name: string): Promise<{ default: any }> {
        const scriptFunc = this.options.scripts[name];
        if (!scriptFunc)
            return;
        return scriptFunc();
    }

    private setNavigation(data: NavigationModel, hash: string, replace: boolean) {
        let navUrl = data.url;
        if (hash)
            navUrl += "#" + hash;

        const prevNav = this.__navigation;

        if (prevNav) {
            if (prevNav.isAuthenticated !== data.isAuthenticated) {
                location.href = navUrl;
                return;
            }

            document.title = data.title ? data.title : "";

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
        }

        this.__navigation = data;
        this.__currentUrl = { url: data.url, hash };

        if (prevNav && prevNav.bodyClass) {
            document.body.classList.remove(prevNav.bodyClass);
        }

        if (this.__navigation.bodyClass) {
            document.body.classList.add(this.__navigation.bodyClass);
        }

        if (navUrl === location.href)
            replace = true;

        if (!hash) {
            if (allowHistory) {
                if (!replace)
                    window.history.pushState(window.history.state, data.title, navUrl);
                else
                    window.history.replaceState(window.history.state, data.title, navUrl);
            }
        }

        if (!replace)
            window.scrollTo({ left: 0, top: 0, behavior: "auto" });
    }
    private __setOpenGraphProperty(name: string, value: string) {
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

    private __loadContent(items: { [key: string]: any }, navSequence: number, next: () => void) {
        if (this.__isNavOutdated(navSequence))
            return;

        this.queue.push({
            url: this.__navigation.url,
            urlParams: { _content: "" },
            disableCache: true,
            success: (response: AjaxResponse) => {
                if (this.__isNavOutdated(navSequence))
                    return;

                switch (response.status) {
                    case 200: {
                        if (this.__precessPageResponse(response))
                            return;

                        this.__renderPage(items, navSequence, response.data ? response.data : "", next);

                        break;
                    }
                    case 404:
                    case 500:
                    case 401: {
                        location.reload();
                        break;
                    }
                    default:
                        throw new Error();
                }
            }
        });
    }
    private __renderPage(items: { [key: string]: any }, navSequence: number, contentHtml: string, next: () => void) {
        if (this.__isNavOutdated(navSequence))
            return;

        let pageTypeName = this.__navigation.page.type;
        if (!pageTypeName)
            pageTypeName = this.options.defaultType;

        if (pageTypeName) {
            const pageTypeFactory = this.options.pageTypes[pageTypeName];
            if (!pageTypeFactory)
                throw `Not found page type "${pageTypeName}".`;

            pageTypeFactory()
                .then((pageType) => { this.__createPage(items, navSequence, pageType.default, contentHtml, next); })
                .catch(() => { throw `Error loading page type "${pageTypeName}".`; });
        }
        else {
            this.__createPage(items, navSequence, Page, contentHtml, next);
        }
    }
    private __createPage(items: { [key: string]: any }, navSequence: number, pageType: new (...p) => Page, contentHtml: string, next: () => void) {
        if (this.__isNavOutdated(navSequence))
            return;

        if (this.__page) {
            this.__page.destroy();
            this.__page = null;
        }

        if (contentHtml !== null) {
            DOM.empty(this.__contentBodyElem);
            this.__contentBodyElem.insertAdjacentHTML("afterbegin", contentHtml);
            WebsiteMiddleware.nodeScriptReplace(this.__contentBodyElem);
        }

        this.__page = new pageType(this, this.__navigation, this.__contentBodyElem);
        this.__page.render(this.__currentUrl.hash);

        items["page"] = this.__page;

        this.__loadingPage = false;

        next();

        this.__hideNavigationProgress();
    }

    private __precessPageResponse(response: AjaxResponse): boolean {
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
            this.nav({
                url: redirectLocation,
                replace: response.xhr.getResponseHeader(pageReplaceHeader) === "true"
            });
            return true;
        }

        return false;
    }

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        const url = location.href;
        const oldUrl = this.__currentUrl;
        const newUrl = this.__extractHashFromUrl(url);

        if (oldUrl.hash && !newUrl.hash) {
            console.log(`remove hash: ${oldUrl.hash}`);
        }
        else if (!oldUrl.hash && newUrl.hash) {
            console.log(`add hash: ${newUrl.hash}`);
        }
        else if (oldUrl.hash && newUrl.hash) {
            console.log(`change hash: ${newUrl.hash}`);
        }

        this.__currentUrl = newUrl;

        if ((oldUrl.hash || newUrl.hash) && oldUrl.url.toLowerCase() === newUrl.url.toLowerCase()) {
            this.__page.changedHash(newUrl.hash, oldUrl.hash);
            return;
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
                url: url.substr(0, hashIndex),
                hash: url.substr(hashIndex + 1)
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

    private __loaderElem: HTMLElement;
    private __progressInterval: number;
    private __progressTimeout: number;
    private __progressStart: number;
    private __showNavigationProgress() {
        window.clearTimeout(this.__progressTimeout);
        window.clearTimeout(this.__progressInterval);

        this.__loaderElem.classList.remove("show", "show2", "finish");
        this.__loaderElem.style.width = "0%";
        this.__loaderElem.classList.add("show");

        this.__loaderElem.style.width = "50%";
        this.__progressTimeout = window.setTimeout(() => {
            this.__loaderElem.classList.add("show2");
            this.__loaderElem.style.width = "70%";
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

            this.__loaderElem.classList.add("finish");
            this.__loaderElem.style.width = "100%";

            this.__progressInterval = window.setTimeout(() => {
                this.__loaderElem.classList.remove("show", "show2", "finish");
                this.__loaderElem.style.width = "0%";
            }, 180);
        }, d);
    }

    static nodeScriptReplace(node: Node) {
        if ((node as Element).tagName === "SCRIPT") {
            const script = document.createElement("script");
            script.text = (node as Element).innerHTML;
            for (let i = (node as Element).attributes.length - 1; i >= 0; i--)
                script.setAttribute((node as Element).attributes[i].name, (node as Element).attributes[i].value);
            node.parentNode.replaceChild(script, node);
        }
        else {
            let i = 0;
            const children = node.childNodes;
            while (i < children.length)
                WebsiteMiddleware.nodeScriptReplace(children[i++]);
        }

        return node;
    }
}

interface UrlParsed {
    url: string;
    hash: string;
}

export interface WebsiteOptions {
    defaultType?: string;
    pageTypes?: { [key: string]: () => Promise<any> };
    scripts?: { [key: string]: () => Promise<any> };
}