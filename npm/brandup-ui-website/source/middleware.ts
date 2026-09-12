import { DOM, UIElement } from "@brandup/ui";
import { type AJAXMethod, AjaxQueue, type AjaxRequest, type AjaxResponse } from "@brandup/ui-ajax";
import type { NavigateContext, StartContext, StopContext, SubmitContext, MiddlewareNext } from "@brandup/ui-app";
import { FuncHelper } from "@brandup/ui-helpers";
import type { NavigationModel, NavigationEntry, WebsiteMiddleware, WebsiteNavigateData, WebsiteOptions, PageDefinition, ComponentScript, PageScript, HistoryState } from "./types";
import { WebsiteApplication } from "./app";
import { Page } from "./page";
import { NavigationLoader } from "./loader";
import { NavigationScroll } from "./scroll";
import * as ScriptHelper from "./helpers/script";
import * as MetaHelper from "./helpers/meta";
import { WEBSITE_MIDDLEWARE_NAME } from "./constants";

const allowHistory = !!window.history && !!window.history.pushState;
const pageReloadHeader = "page-reload";
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
    private __loader?: NavigationLoader;
    private __scroll?: NavigationScroll;

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

        this.__loader = new NavigationLoader(bodyElem);
        this.__loader.begin();

        this.__prepareRequest = (request) => {
            if (!request.headers)
                request.headers = {};

            if (context.app.model.antiforgery && request.method && request.method !== "GET" && this.__current?.model.validationToken)
                request.headers[context.app.model.antiforgery.headerName] = this.__current.model.validationToken;
        };

        this.__scroll = new NavigationScroll(() => this.__current?.context.id);

        return next();
    }

    async navigate(context: NavigateContext<WebsiteApplication, WebsiteNavigateData>, next: MiddlewareNext) {
        if (!this.__queue)
            throw new Error('Website is not initialized.');

        // Уходим со страницы — дописываем её позицию прокрутки в состояние истории: во время
        // набора текста запись отложена, а иначе возврат назад восстановил бы устаревшую.
        this.__scroll?.save();

        if (context.external || !allowHistory) {
            this.__forceNav(context);
            return;
        }
        else if (context.action === "hash") {
            if (this.__current) {
                this.__setNavigation(context, this.__current, this.__current.model, this.__current.page);
                await this.__current.page.__changedHash(context);
            }

            await next();
            return;
        }

        const progressToken = this.__loader?.begin();
        this.__queue.reset(true);

        const isFirst = context.source === "first";
        const current: NavigationEntry | undefined = context.data.current = this.__current;

        try {
            let navModel: NavigationModel;
            let navContent: DocumentFragment | null = null;

            if (isFirst || !this.__current) {
                // first navigation

                const navScriptElement = <HTMLScriptElement>document.getElementById(navDataElemId);
                if (!navScriptElement)
                    throw new Error('Not found first navigation data.');

                navModel = JSON.parse(navScriptElement.text);
                navScriptElement.remove();
            }
            else {
                // continue navigation

                const response: AjaxResponse = await FuncHelper.minWaitAsync(() => this.__queue.enqueue({
                    method: "GET", url: context.url, query: { "_": new Date().getTime().toString() },
                    headers: { "page-nav": current?.model.state || "" },
                    disableCache: true
                }, context.abort), this.options.navMinTime, context.abort);

                if (response.status != 200) {
                    console.warn(`Nav request response status ${response.status}`);
                    this.__forceNav(context);
                    return;
                }

                if (response.headers.has(pageReloadHeader)) {
                    this.__forceNav(context);
                    return;
                }

                // Редирект (page-location) приходит со статусом 200, но без html-тела,
                // поэтому обрабатываем его до проверки response.type на html.
                if (await this.__processPageResponse(context, response))
                    return;

                if (response.type != "html")
                    throw new Error('Nav response is not html.');

                navContent = document.createDocumentFragment();
                const fixElem = DOM.tag("div");
                navContent.append(fixElem);
                fixElem.insertAdjacentHTML("beforebegin", response.data);
                fixElem.remove();

                const navJsonElem = <HTMLScriptElement | null>navContent.getElementById(navDataElemId);
                if (!navJsonElem)
                    throw new Error('Not found navigation data.');
                navModel = JSON.parse(navJsonElem.text);
                navJsonElem.remove();

                if (current && current.model.isAuthenticated !== navModel.isAuthenticated) {
                    this.__forceNav(context);
                    return;
                }
            }

            context.data.page = await this.__renderPage(context, current, navModel, navContent);

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
            this.__loader?.end(progressToken);
        }
    }

    async submit(context: SubmitContext<WebsiteApplication>, next: MiddlewareNext) {
        if (!this.__current)
            throw new Error('Unable to submit.');

        const { url, form } = context;
        const method = (context.method.toUpperCase() as AJAXMethod);

        const current = context.data.current = this.__current;

        const progressToken = this.__loader?.begin();
        current.page.queue.reset(true);

        try {
            var query: { [key: string]: string | string[]; } = {};
            for (var key in current.model.query)
                query[key] = current.model.query[key];

            const response: AjaxResponse = await FuncHelper.minWaitAsync(() => current.page.queue.enqueue({
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

            if (await this.__processPageResponse(context, response))
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
            this.__loader?.end(progressToken);
        }
    }

    stop(context: StopContext<WebsiteApplication>, next: MiddlewareNext) {
        context.data.current = this.__current;

        this.__scroll?.destroy();
        this.__scroll = undefined;

        this.__loader?.destroy();
        this.__loader = undefined;

        return next();
    }

    async renderComponents(container: Page) {
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

                const component: UIElement = new scriptType.default(elem, container);
                container.on("destroy", () => component.destroy());
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

    private async __processPageResponse(context: NavigateContext, response: AjaxResponse): Promise<boolean> {
        const isReload = response.headers.has(pageReloadHeader);
        const redirectUrl = response.headers.get(pageLocationHeader);
        if (redirectUrl) {
            const replace = response.headers.has(pageReplaceHeader);

            if (isReload) {
                this.__loader?.keep();

                if (replace)
                    window.location.replace(redirectUrl);
                else
                    window.location.assign(redirectUrl);
            }
            else
                await context.redirect({ url: redirectUrl, replace, data: context.data });

            return true;
        }
        else if (isReload) {
            this.__loader?.keep();
            window.location.reload();
        }

        return false;
    }

    private __forceNav(context: NavigateContext) {
        this.__loader?.keep();

        if (context.replace && !context.external)
            window.location.replace(context.url);
        else
            window.location.assign(context.url);
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

            if (newNav)
                this.__setNavigation(context, current, newNav, page);
            else if (current) {
                if (current.page !== page)
                    current.page.destroy();
                current.page = page;
            }
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

        try {
            await this.renderComponents(page);
            await page.__rendered();
        }
        finally {
            if (context.data.popstate)
                this.__scroll?.restore(context.data.popstate);
        }

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
        if (context.hash)
            navUrl += "#" + context.hash;

        const isFirst = context.action == "first";
        const title = newNav.title || "";
        const changedPage = current?.page !== page;

        if (!isFirst && changedPage) {
            MetaHelper.setMetadata("description", newNav.description);
            MetaHelper.setMetadata("keywords", newNav.keywords);
            MetaHelper.setCanonical(newNav.canonicalLink);
            MetaHelper.setOpenGraph(newNav.openGraph);

            if (current?.model.bodyClass)
                document.body.classList.remove(current.model.bodyClass);

            if (newNav.bodyClass)
                document.body.classList.add(newNav.bodyClass);
        }

        if (current?.page && changedPage)
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
        if (replace && (changedScope || context.current?.action === "first")) {
            // Если изменилась область навигации или предыдущая бала первой, то 
            // не нужно перезаписывать текущую страницу
            replace = false;

            if (changedScope)
                forceSkipScroll = true; // принудительно пропускаем прокрутку
        }

        let state: HistoryState | null = window.history.state;
        if (!state)
            state = {};
        if (!state.brandup_website)
            state.brandup_website = { id: context.id };
        else
            state.brandup_website.id = context.id;

        if (!isFirst) {
            const isHashChanged = context.action === "hash";
            const isNoChangeUrl = context.action === "url-no-change";

            if (context.data.popstate !== undefined || isNoChangeUrl) {
                /*
                    1. Если навигация из события popstate, то принулительно перезаписываем состояние.
                    2. Если url навигации не поменялся, то перезаписываем состояние.
                */

                replace = true;
            }

            if (replace)
                window.history.replaceState(state, "", navUrl);
            else {
                // Новая запись истории заводится от состояния предыдущей, поэтому позицию прокрутки
                // нужно убрать: иначе страница унаследует чужую и восстановит её при возврате.
                delete state.brandup_website.scroll;
                window.history.pushState(state, "", navUrl);
            }

            if (changedPage && !isHashChanged)
                document.title = title;

            if ((!replace && !forceSkipScroll && !isHashChanged) || (isNoChangeUrl && context.data.clickElem))
                window.scrollTo({ left: 0, top: 0, behavior: "auto" });
        }
        else
            window.history.replaceState(state, "", navUrl);
    }
}