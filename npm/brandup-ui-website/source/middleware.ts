import { DOM } from "@brandup/ui-dom";
import { UIElement } from "@brandup/ui";
import { AJAXMethod, AjaxQueue, AjaxRequest, AjaxResponse } from "@brandup/ui-ajax";
import { NavigateContext, StartContext, StopContext, SubmitContext, MiddlewareNext, BROWSER, Application } from "@brandup/ui-app";
import { FuncHelper } from "@brandup/ui-helpers";
import { NavigationModel, NavigationEntry, WebsiteMiddleware, WebsiteNavigateData, WebsiteOptions, PageDefinition, ComponentScript, PageScript } from "./types";
import { WebsiteApplication } from "./app";
import { Page } from "./page";
import * as ScriptHelper from "./helpers/script";
import * as MetaHelper from "./helpers/meta";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "page-reload";
const pageActionHeader = "page-action";
const pageLocationHeader = "page-location";
const pageReplaceHeader = "page-replace";
const navDataElemId = "nav-data";
const pageElemId = "page-content";

export class WebsiteMiddlewareImpl implements WebsiteMiddleware {
    readonly name: string = WEBSITE_MIDDLEWARE_NAME;
    readonly options: WebsiteOptions;
    private __queue: AjaxQueue;
    private __current?: NavigationEntry;
    private __prepareRequest?: (request: AjaxRequest) => void;

    constructor(options: WebsiteOptions) {
        this.options = options;

        if (this.options.defaultPage && (!this.options.pages || !this.options.pages[this.options.defaultPage]))
            throw new Error(`Default page type is not registered.`);

        this.__queue = new AjaxQueue({
            canRequest: (request) => this.prepareRequest(request)
        });
    }

    get current(): NavigationEntry | undefined { return this.__current; }
    get validationToken(): string | null { return this.__current?.model.validationToken || null; }

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

        this.__prepareRequest = (request) => {
            if (!request.headers)
                request.headers = {};

            if (context.app.model.antiforgery && request.method && request.method !== "GET" && this.__current?.model.validationToken)
                request.headers[context.app.model.antiforgery.headerName] = this.__current.model.validationToken;
        };

        return next();
    }

    async navigate(context: NavigateContext<WebsiteApplication, WebsiteNavigateData>, next: MiddlewareNext) {
        if (!this.__queue)
            throw new Error('Website is not initialized.');

        const current = context.data.current = this.__current;

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

                const response: AjaxResponse = await FuncHelper.minWaitAsync(() => this.__queue.enque({
                    method: "GET", url: context.url, query: { "_": new Date().getTime().toString() },
                    headers: { "page-nav": current?.model.state || "" },
                    disableCache: true
                }, context.abort), this.options.navMinTime, context.abort);

                if (response.status != 200 && response.type != "html") {
                    console.warn(`Nav request response status ${response.status}`);
                    this.__forceNav(context);
                    return;
                }

                if (response.headers.has(pageReloadHeader)) {
                    this.__forceNav(context);
                    return;
                }

                if (await this.__precessPageResponse(context, response))
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

            const page = await this.__renderPage(context, current, navModel, navContent);
            if (page)
                await next();
        }
        catch (reason) {
            if (!isFirst && !context.abort.aborted) {
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

        this.__showNavigationProgress();
        current.page.queue.reset(true);

        try {
            var query: { [key: string]: string | string[]; } = {};
            for (var key in current.model.query)
                query[key] = current.model.query[key];

            const response: AjaxResponse = await FuncHelper.minWaitAsync(() => current.page.queue.enque({
                method, url, query,
                headers: { "page-nav": current.model.state || "", "page-submit": "true" },
                data: new FormData(form)
            }, context.abort), this.options.submitMinTime, context.abort);

            switch (response.status) {
                case 200:
                case 201:
                    break;
                default:
                    throw new Error(`Submit request response status ${response.status}`);
            }

            if (await this.__precessPageResponse(context, response))
                return;

            if (response.type == "html") {
                if (!response.data)
                    throw new Error('Submit response not have html.');

                const contentFragment = document.createDocumentFragment();
                const fixElem = DOM.tag("div");
                contentFragment.append(fixElem);
                fixElem.insertAdjacentHTML("beforebegin", response.data);
                fixElem.remove();

                await this.__renderPage(context, current, null, contentFragment);
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

    async renderComponents(container: UIElement) {
        if (!container.element)
            throw new Error(`Container ${container.typeName} is not set element.`);

        const defineScripts = DOM.queryElements(container.element, "[data-content-script]");
        for (let i = 0; i < defineScripts.length; i++) {
            const elem = defineScripts.item(i);
            if (UIElement.hasElement(elem))
                continue;

            const componentName = elem.getAttribute("data-content-script");
            if (!componentName)
                continue;

            const componentType = this.findComponent(componentName);
            if (componentType) {
                const scriptType = await componentType();
                if (!scriptType.default)
                    throw new Error(`Component ${componentName} is not set default export.`);

                const component: UIElement = new scriptType.default(elem, this);
                container.onDestroy(component);
            }
        }
    }

    findComponent(name: string): (() => Promise<ComponentScript>) | null {
        if (!this.options.components)
            return null;

        const scriptFunc = this.options.components[name];
        if (!scriptFunc)
            return null;

        return scriptFunc.factory;
    }

    prepareRequest(request: AjaxRequest) {
        if (!this.__prepareRequest)
            throw new Error("Application is not started.");

        this.__prepareRequest(request);
    }

    // WebsiteMiddleware members

    private async __precessPageResponse(context: NavigateContext, response: AjaxResponse): Promise<boolean> {
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
                await context.redirect({ url: redirectUrl, replace, data: context.data });

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

    private async __renderPage(context: NavigateContext<WebsiteApplication>, current: NavigationEntry | undefined, newNav: NavigationModel | null, newContent: DocumentFragment | null) {
        const nav = newNav || current?.model;
        if (!nav)
            throw new Error('Not set nav.');

        let pageTypeName: string | null = nav.page.type;
        if (!pageTypeName && this.options.defaultPage)
            pageTypeName = this.options.defaultPage;

        let pageDefinition: PageDefinition | null = null;

        if (pageTypeName) {
            pageDefinition = this.options.pages ? this.options.pages[pageTypeName] : null;
            if (!pageDefinition)
                throw new Error(`Not found page definition "${pageTypeName}".`);
        }
        else
            pageDefinition = { factory: () => Promise.resolve({ default: Page }) };

        const pageType: PageScript = await pageDefinition.factory();
        context.abort.throwIfAborted();

        let currentPageElem: HTMLElement | null;
        let newPageElem: HTMLElement;
        if (newContent !== null) {
            // replace page content

            currentPageElem = current?.page.element || null;
            if (!currentPageElem)
                currentPageElem = this.__getPageElem();

            const elem = newContent.getElementById(pageElemId);
            if (!elem)
                throw new Error("Not found page element.");
            newPageElem = elem;
        }
        else {
            // render first nav

            currentPageElem = null;
            newPageElem = this.__getPageElem();
        }

        let page: Page | undefined;
        try {
            page = <Page>new pageType.default(context, nav);
            await page.__render(newPageElem);

            context.abort.throwIfAborted();

            await this.renderComponents(page);

            context.abort.throwIfAborted();

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

            ScriptHelper.scriptReplace(newPageElem);
        }

        await page.__rendered();

        return page;
    }

    private __getPageElem() {
        const elem = document.getElementById(pageElemId);
        if (!elem)
            throw new Error("Not found page element.");
        return elem;
    }

    private __setNavigation(context: NavigateContext, current: NavigationEntry | undefined, newNav: NavigationModel, page: Page) {
        let navUrl = context.url;

        const isFirst = context.source == "first";
        const fromPopstate = !!context.data.popstate;
        const title = newNav.title || "";

        if (!isFirst) {
            MetaHelper.setMetadata("description", newNav.description);
            MetaHelper.setMetadata("keywords", newNav.keywords);
            MetaHelper.setCanonical(newNav.canonicalLink);
            MetaHelper.setOG("type", newNav.openGraph?.type);
            MetaHelper.setOG("title", newNav.openGraph?.title);
            MetaHelper.setOG("image", newNav.openGraph?.image);
            MetaHelper.setOG("url", newNav.openGraph?.url);
            MetaHelper.setOG("site_name", newNav.openGraph?.siteName);
            MetaHelper.setOG("description", newNav.openGraph?.description);

            if (current?.model.bodyClass)
                document.body.classList.remove(current.model.bodyClass);

            if (newNav.bodyClass)
                document.body.classList.add(newNav.bodyClass);
        }

        if (current?.page)
            current.page.destroy();

        this.__current = {
            context,
            url: context.url,
            hash: context.hash,
            model: newNav,
            page
        };

        let replace = context.replace;
        let forceSkipScroll = false;
        const changedScope = context.current?.scope != context.scope;
		if (replace && (changedScope || context.current?.source === "first")) {
			// Если изменилась область навигации или предыдущая бала первой, то 
			// не нужно перезаписывать текущую страницу
			replace = false;

            if (changedScope)
                forceSkipScroll = true; // принудительно пропускаем прокрутку
		}

        if (isFirst || navUrl === location.href)
            replace = true;

        if (!isFirst && !fromPopstate) {
            if (!context.hash) {
                if (replace)
                    window.history.replaceState(window.history.state, title, navUrl);
                else
                    window.history.pushState(window.history.state, title, navUrl);
            }
            else
                location.hash = "#" + context.hash;

            document.title = title;

            if (!replace && !forceSkipScroll)
                window.scrollTo({ left: 0, top: 0, behavior: "auto" });
        }
    }

    private __onPopState(context: StartContext, event: PopStateEvent) {
        event.preventDefault();

        console.log(`popstate: ${location.href}`);

        context.app.nav({ data: { popstate: true } });
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

        this.__loaderElem.classList.remove("show", "finish");
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