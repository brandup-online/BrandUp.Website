import { DOM } from "@brandup/ui-dom";
import { WEBSITE, NavigationModel, Page } from "../source/index";

it('Success with default page type', async () => {
    const appDataScript = <HTMLScriptElement>DOM.tag("script", { id: "app-data", type: "application/json" });
    appDataScript.text = '{ "env": { "basePath": "/" }, "model": { "websiteId": "id" } }';
    document.head.insertAdjacentElement("beforeend", appDataScript);

    const firstNav: NavigationModel = {
        url: "http://localhost/",
        path: "/",
        query: {},
        validationToken: "valid-token",
        state: "state",
        title: "title",
        canonicalLink: null,
        description: null,
        keywords: null,
        isAuthenticated: false,
        bodyClass: null,
        openGraph: null,
        page: { type: null }
    };
    const navDataScript = <HTMLScriptElement>DOM.tag("script", { id: "nav-data", type: "application/json" });
    navDataScript.text = JSON.stringify(firstNav);
    document.body.insertAdjacentElement("beforeend", navDataScript);
    document.body.insertAdjacentElement("beforeend", DOM.tag("div", { id: "page-content" }));

    const context = await WEBSITE.run({
        pages: {
            "test": { factory: () => Promise.resolve({ default: Page }) }
        }
    }, builder => { }, { test: "test" });

    expect(context).not.toBeNull();
    expect(context.url).toEqual(firstNav.url);
    expect(context.source).toEqual("first");
})