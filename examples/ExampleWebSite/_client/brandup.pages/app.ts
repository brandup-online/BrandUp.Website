import { AppClientModel, NavigationModel, PageClientModel, PageNavState, IApplication, NavigationOptions } from "./typings/website";
import Page from "./pages/page";
import { UIElement, DOM, Utility, ajaxRequest, AjaxRequestOptions } from "brandup-ui";
import "./app.less";

const allowHistory = !!window.history && !!window.history.pushState;

export class ApplicationBuilder {
    private __pageTypes: { [key: string]: () => Promise<any> } = {};
    private __scripts: { [key: string]: () => Promise<any> } = {};

    addPageType(name: string, importFunc: () => Promise<any>) {
        this.__pageTypes[name.toLowerCase()] = importFunc;
    }
    getPageType(name: string): Promise<{ default: any }> {
        const f = this.__pageTypes[name.toLowerCase()];
        return f();
    }

    addScript(name: string, importFunc: () => Promise<any>) {
        this.__scripts[name.toLowerCase()] = importFunc;
    }
    getScript(name: string): Promise<{ default: any }> {
        const f = this.__scripts[name.toLowerCase()];
        return f();
    }
}

export class Application<TModel extends AppClientModel> extends UIElement implements IApplication {
    private __navCounter = 0;
    private __navigation: NavigationModel;
    private __contentBodyElem: HTMLElement;
    readonly model: TModel;
    private options: AppSetupOptions;
    page: Page<PageClientModel> = null;
    private linkClickFunc: () => void;
    private keyDownUpFunc: () => void;
    private __builder: ApplicationBuilder;
    private __requestVerificationToken: HTMLInputElement;
    private __progressElem: HTMLElement;
    private __progressInterval: number;
    private __progressTimeout: number;
    private __progressStart: number;

    protected constructor(model: TModel, options: AppSetupOptions) {
        super();

        this.model = model;
        this.options = options ? options : { defaultPageScript: "page" };

        if (!this.options.defaultPageScript)
            this.options.defaultPageScript = "page";

        this.__builder = new ApplicationBuilder();
        this.__builder.addPageType("page", () => import("./pages/page"));

        if (options.configure)
            options.configure(this.__builder);

        this.setElement(document.body);

        this.defineEvent("pageNavigating", { cancelable: true, bubbles: true });
        this.defineEvent("pageNavigated", { cancelable: false, bubbles: true });
        this.defineEvent("pageLoading", { cancelable: false, bubbles: true });
        this.defineEvent("pageLoaded", { cancelable: false, bubbles: true });
        this.defineEvent("pageContentLoaded", { cancelable: false, bubbles: true });

        this.linkClickFunc = Utility.createDelegate(this, this.__onClickAppLink);
        this.keyDownUpFunc = Utility.createDelegate(this, this.__onKeyDownUp);
    }

    get typeName(): string { return "Application" }
    get navigation(): NavigationModel { return this.__navigation; }

    init() {
        this.__contentBodyElem = document.getElementById("page-content");
        if (!this.__contentBodyElem)
            throw "Не найден элемент контента страницы.";

        this.__requestVerificationToken = DOM.getElementByName("__RequestVerificationToken") as HTMLInputElement;
        if (this.__requestVerificationToken === null)
            throw `Не найден элемент с именем __RequestVerificationToken.`;

        const initNav = this.model.nav;
        const pageState: PageNavState = {
            url: initNav.url,
            title: initNav.page.title,
            path: initNav.path,
            params: initNav.query,
            hash: location.hash ? location.hash.substr(1) : null
        };

        this.__navigation = initNav;

        if (allowHistory) {
            window.addEventListener("popstate", Utility.createDelegate(this, this.__onPopState));
            window.addEventListener("hashchange", Utility.createDelegate(this, this.__onHashChange));
        }

        window.addEventListener("click", this.linkClickFunc, false);
        window.addEventListener("keydown", this.keyDownUpFunc, false);
        window.addEventListener("keyup", this.keyDownUpFunc, false);
        document.body.appendChild(this.__progressElem = DOM.tag("div", { class: "bp-page-loader" }));

        this.__renderPage(pageState, initNav.page, false, false, false);
    }
    load() { console.log("app loaded"); }
    destroy() {
        document.body.removeEventListener("click", this.linkClickFunc, false);
        document.body.removeEventListener("keydown", this.keyDownUpFunc, false);
        document.body.removeEventListener("keyup", this.keyDownUpFunc, false);
    }

    request(options: AjaxRequestOptions) {
        if (!options.headers)
            options.headers = {};

        if (this.model.antiforgery && options.method !== "GET")
            options.headers[this.model.antiforgery.headerName] = this.__navigation.validationToken;

        ajaxRequest(options);
    }
    uri(path?: string, queryParams?: { [key: string]: string }): string {
        let url = this.model.baseUrl;
        if (path)
        //    url += "/";
        //else
        {
            if (path.substr(0, 1) === "/")
                path = path.substr(1);
            url += path;
        }

        if (queryParams) {
            let query = "";
            let i = 0;
            for (const key in queryParams) {
                const value = queryParams[key];
                if (value === null || typeof value === "undefined")
                    continue;

                if (i > 0)
                    query += "&";

                query += key;

                if (value)
                    query += "=" + value;

                i++;
            }

            if (query)
                url += "?" + query;
        }

        return url;
    }

    reload() {
        this.nav({ url: null, pushState: false });
    }
    navigate(target: string | HTMLElement) {
        if (!target)
            throw new Error("target not set");

        let url: string = null;
        let pushState = true;
        if (Utility.isString(target))
            url = target as string;
        else {
            const targetElem = target as HTMLElement;
            if (targetElem.tagName === "A")
                url = targetElem.getAttribute("href");
            else if (targetElem.hasAttribute("data-href"))
                url = targetElem.getAttribute("data-href");
            else
                throw "Не удалось получить Url адрес для перехода.";

            if (targetElem.hasAttribute("data-url-replace"))
                pushState = false;
        }

        if (!url)
            url = location.href;

        this.nav({ url: url, pushState: pushState, scrollToTop: pushState });
    }
    nav(options: NavigationOptions) {
        const isCancelled = this.raiseEvent("pageNavigating", options);
        if (!isCancelled) {
            console.log("cancelled navigation");
            return;
        }

        if (!allowHistory) {
            location.href = options.url ? options.url : location.href;
            return;
        }

        let { url, hash, pushState } = options;
        const { notRenderPage, scrollToTop } = options;
        if (!url) {
            url = location.href;
            const hashIndex = url.lastIndexOf("#");
            if (hashIndex > 0) {
                url = url.substr(0, hashIndex);
                hash = url.substr(hashIndex + 1);
            }
        }

        if (!hash)
            hash = null;
        else {
            if (hash.startsWith("#"))
                hash = hash.substr(1);
        }

        this.__navCounter++;
        const navSequence = this.__navCounter;

        this.__beginLoading();

        this.request({
            url: url,
            method: "POST",
            urlParams: { _nav: "" },
            type: "TEXT",
            data: this.navigation.state ? this.navigation.state : "",
            success: (data: NavigationModel, status: number, xhr: XMLHttpRequest) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        const pageAction = xhr.getResponseHeader("Page-Action");
                        if (pageAction) {
                            switch (pageAction) {
                                case "reset": {
                                    location.href = url;
                                    return;
                                }
                                default:
                                    throw "Неизвестный тип действия для страницы.";
                            }
                        }

                        const redirectLocation = xhr.getResponseHeader("Page-Location");
                        if (redirectLocation) {
                            if (redirectLocation.startsWith("/"))
                                this.navigate(redirectLocation);
                            else
                                location.href = redirectLocation;
                            return;
                        }

                        let navUrl = data.url;
                        if (hash)
                            navUrl += "#" + hash;

                        if (this.__navigation.isAuthenticated !== data.isAuthenticated) {
                            location.href = navUrl;
                            return;
                        }

                        this.__navigation = data;

                        const pageModel = data.page;
                        const navState: PageNavState = {
                            url: data.url,
                            title: pageModel.title,
                            path: data.path,
                            params: data.query,
                            hash: hash
                        };

                        if (navUrl === location.href)
                            pushState = false;

                        this.raiseEvent("pageNavigated", navState);

                        if (options.success)
                            options.success();

                        if (!notRenderPage)
                            this.__renderPage(navState, pageModel, true, scrollToTop, pushState);
                        else {
                            this.setNav(navState, pageModel, pushState);

                            this.page.update(navState, pageModel);

                            this.__endLoading();
                        }

                        break;
                    }
                    default:
                        location.href = url;
                }
            }
        });
    }
    private setNav(navState: PageNavState, pageModel: PageClientModel, pushState: boolean) {
        const navUrl = navState.hash ? navState.url + "#" + navState.hash : navState.url;

        if (pushState)
            window.history.pushState(navState, navState.title, navUrl);
        else
            window.history.replaceState(navState, navState.title, navUrl);

        document.title = pageModel.title ? pageModel.title : "";
    }

    script(name: string): Promise<{ default: object }> {
        const scriptPromise = this.__builder.getScript(name);
        if (!scriptPromise)
            return;
        return scriptPromise;
    }
    renderPage(html: string) {
        const navState = this.page.nav;
        const pageModel = this.page.model;

        this.page.destroy();
        this.page = null;

        this.__loadPageScript(navState, pageModel, html ? html : "", false, null);
    }

    private __renderPage(navState: PageNavState, pageModel: PageClientModel, needLoadContent: boolean, scrollToTop: boolean, pushState: boolean) {
        this.raiseEvent("pageLoading", navState);

        if (needLoadContent)
            this.__loadContent(navState, pageModel, scrollToTop, pushState);
        else
            this.__loadPageScript(navState, pageModel, null, scrollToTop, pushState);
    }
    private __loadContent(navState: PageNavState, pageModel: PageClientModel, scrollToTop: boolean, pushState: boolean) {
        const navSequence = this.__navCounter;

        this.request({
            url: navState.url,
            urlParams: { _content: "" },
            disableCache: true,
            success: (data: string, status: number) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        this.__loadPageScript(navState, pageModel, data ? data : "", scrollToTop, pushState);

                        this.raiseEvent("pageContentLoaded");

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
    private __loadPageScript(navState: PageNavState, pageModel: PageClientModel, contentHtml: string, scrollToTop: boolean, pushState: boolean) {
        let pageScript = pageModel.scriptName;
        if (!pageScript)
            pageScript = this.options.defaultPageScript;

        this.__builder.getPageType(pageScript)
            .then((pageType) => {
                if (this.page) {
                    this.page.destroy();
                    this.page = null;
                }

                if (pushState !== null)
                    this.setNav(navState, pageModel, pushState);

                if (scrollToTop)
                    window.scrollTo({ left: 0, top: 0, behavior: "auto" });

                if (contentHtml !== null) {
                    DOM.empty(this.__contentBodyElem);
                    this.__contentBodyElem.insertAdjacentHTML("afterbegin", contentHtml);
                    Application.nodeScriptReplace(this.__contentBodyElem);
                }

                this.__createPage(pageType.default, navState, pageModel);
            })
            .catch(() => {
                throw "Ошибка загрузки скрипта страницы.";
            });
    }
    private __createPage(pageType: new (...p) => Page<PageClientModel>, navState: PageNavState, pageModel: PageClientModel) {
        this.page = new pageType(this, navState, pageModel, this.__contentBodyElem);

        this.__endLoading();

        this.raiseEvent("pageLoaded", navState);
    }

    private __beginLoading() {
        window.clearTimeout(this.__progressTimeout);
        window.clearTimeout(this.__progressInterval);

        this.__progressElem.classList.remove("show", "show2", "finish");
        this.__progressElem.style.width = "0%";
        this.__progressElem.classList.add("show");

        this.element.classList.remove("bp-state-loaded");
        this.element.classList.add("bp-state-loading");

        this.__progressElem.style.width = "50%";
        this.__progressTimeout = window.setTimeout(() => {
            this.__progressElem.classList.add("show2");
            this.__progressElem.style.width = "70%";
        }, 1700);

        this.__progressStart = Date.now();
    }
    private __endLoading() {
        document.body.classList.remove("bp-state-loading");
        document.body.classList.add("bp-state-loaded");

        let d = 400 - (Date.now() - this.__progressStart);
        if (d < 0)
            d = 0;

        this.__progressTimeout = window.setTimeout(() => {
            window.clearTimeout(this.__progressInterval);

            this.__progressElem.classList.add("finish");
            this.__progressElem.style.width = "100%";

            this.__progressInterval = window.setTimeout(() => {
                this.__progressElem.classList.remove("show", "show2", "finish");
                this.__progressElem.style.width = "0%";
            }, 180);
        }, d);
    }

    private _ctrlPressed = false;

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        const url = location.href;
        console.log("PopState: " + url);

        if (url.lastIndexOf("#") > 0) {
            const t = url.lastIndexOf("#");
            const urlHash = url.substr(t + 1);
            const urlWithoutHash = url.substr(0, t);

            if (!event.state) {
                console.log("PopState hash: " + urlHash);

                const pageState: PageNavState = {
                    url: url.substr(0, t),
                    title: this.__navigation.page.title,
                    path: this.__navigation.path,
                    params: this.__navigation.query,
                    hash: urlHash
                };

                window.history.replaceState(pageState, pageState.title, location.href);

                return;
            }
            else {
                if (urlWithoutHash.toLowerCase() === this.__navigation.url.toLowerCase())
                    return;
            }
        }

        if (event.state) {
            const state = event.state as PageNavState;
            this.nav({ url: state.url, hash: state.hash, pushState: false, scrollToTop: false });

            return;
        }
    }
    private __onHashChange(e: HashChangeEvent) {
        console.log("HashChange to " + e.newURL);
    }
    private __onClickAppLink(e: MouseEvent) {
        let elem = e.target as HTMLElement;
        let ignore = false;
        while (elem) {
            if (elem.hasAttribute("data-nav-ignore")) {
                ignore = true;
                break;
            }

            if (elem.classList && elem.classList.contains("applink"))
                break;
            if (elem === e.currentTarget)
                return;

            if (typeof elem.parentElement === "undefined")
                elem = elem.parentNode as HTMLElement;
            else
                elem = elem.parentElement;

            if (!elem)
                return true;
        }

        if (this._ctrlPressed)
            return true;

        if (elem.hasAttribute("target")) {
            if (elem.getAttribute("target") === "_blank")
                return true;
        }

        e.preventDefault();
        e.stopPropagation();
        e.returnValue = false;

        if (ignore)
            return false;

        this.navigate(elem);

        return false;
    }
    private __onKeyDownUp(e: KeyboardEvent) {
        this._ctrlPressed = e.ctrlKey;
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
                Application.nodeScriptReplace(children[i++]);
        }

        return node;
    }
    static setup<TModel extends AppClientModel>(options: AppSetupOptions, init?: (app: Application<TModel>) => void) {
        if (window["app"]) {
            const app = window["app"] as Application<TModel>;
            app.destroy();

            delete window["app"];
        }

        if (window["appInitOptions"]) {
            const appModel = window["appInitOptions"] as TModel;
            const app = new Application<TModel>(appModel, options);
            
            setTimeout(() => {
                let isInitiated = false;
                const appInitFunc = () => {
                    if (isInitiated)
                        return;
                    isInitiated = true;
                    app.init();
                    if (init)
                        init(app);
                };
                window["app"] = app;

                if (document.readyState === "loading") {
                    document.addEventListener("readystatechange", () => {
                        if (document.readyState !== "loading")
                            appInitFunc();
                    });
                }
                else
                    appInitFunc();

                window.addEventListener("load", () => {
                    appInitFunc();
                    app.load();
                });
            }, 0);

            return app;
        }

        return null;
    }
}

export interface AppSetupOptions {
    defaultPageScript?: string;
    configure?: (builder: ApplicationBuilder) => void;
}