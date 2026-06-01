import { DOM } from "@brandup/ui";
import { WEBSITE, NavigationModel, Page } from "../source/index";

const setupDom = () => {
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
};

it('Success with default page type', async () => {
    setupDom();

    const context = await WEBSITE.run({
        submitMinTime: 1234,
        pages: {
            "test": { factory: () => Promise.resolve({ default: Page }) }
        }
    }, () => { }, { test: "test" });

    expect(context).not.toBeNull();
    expect(context.data.test).toEqual("test");

    // user-provided option survives the default merge (regression for Object.assign order)
    expect(context.app.options.submitMinTime).toEqual(1234);
    // default applied for not-provided option
    expect(context.app.options.navMinTime).toEqual(0);

    // current is a live getter returning the running application, not a stale null snapshot
    expect(WEBSITE.current).not.toBeNull();
    expect(WEBSITE.current).toBe(context.app);
})

it('Second run is rejected', async () => {
    await expect(WEBSITE.run({}, () => { })).rejects.toBeDefined();
})
