import { DOM } from "@brandup/ui";
import { WEBSITE, NavigationModel, WebsiteApplication } from "../source/index";

const buildNav = (over: Partial<NavigationModel>): NavigationModel => ({
    url: "http://localhost/", path: "/", query: {}, validationToken: "tok", state: "state",
    title: "Home", canonicalLink: null, description: null, keywords: null,
    isAuthenticated: false, bodyClass: null, openGraph: null, page: { type: null }, ...over
});

const pageHtml = (label: string, navOver: Partial<NavigationModel>) =>
    `<script id="nav-data" type="application/json">${JSON.stringify(buildNav(navOver))}</script>` +
    `<div id="page-content">${label}</div>`;

let app: WebsiteApplication;
let fetchMock: jest.Mock;

const waitFor = async (predicate: () => boolean, timeout = 2000) => {
    const start = Date.now();
    while (Date.now() - start < timeout) {
        if (predicate()) return;
        await new Promise(r => setTimeout(r, 20));
    }
    throw new Error("Condition not met within timeout.");
};

beforeAll(async () => {
    window.scrollTo = (() => { }) as typeof window.scrollTo;

    const appDataScript = <HTMLScriptElement>DOM.tag("script", { id: "app-data", type: "application/json" });
    appDataScript.text = '{ "env": { "basePath": "/" }, "model": { "websiteId": "id" } }';
    document.head.insertAdjacentElement("beforeend", appDataScript);

    const navDataScript = <HTMLScriptElement>DOM.tag("script", { id: "nav-data", type: "application/json" });
    navDataScript.text = JSON.stringify(buildNav({}));
    document.body.insertAdjacentElement("beforeend", navDataScript);
    document.body.insertAdjacentElement("beforeend", DOM.tag("div", { id: "page-content" }, "HOME"));

    fetchMock = jest.fn((input: RequestInfo | URL) => {
        const url = typeof input === "string" ? input : input.toString();
        if (url.includes("/bad"))
            return Promise.resolve(new Response("error", { status: 500, headers: { "content-type": "text/html" } }));
        if (url.includes("/old"))
            return Promise.resolve(new Response("", { status: 200, headers: { "content-type": "text/html", "page-location": "/target" } }));
        if (url.includes("/target"))
            return Promise.resolve(new Response(pageHtml("TARGET", { url: "http://localhost/target", path: "/target", title: "Target" }), { status: 200, headers: { "content-type": "text/html" } }));
        return Promise.resolve(new Response("{}", { status: 200, headers: { "content-type": "application/json" } }));
    });
    global.fetch = fetchMock as unknown as typeof fetch;

    const context = await WEBSITE.run({ navMinTime: 0, pages: {} }, () => { });
    app = context.app;
});

it("does not re-render when the nav response is not a 200 html page", async () => {
    await app.nav({ url: "/bad" }).catch(() => { });

    expect(fetchMock.mock.calls.some(c => String(c[0]).includes("/bad"))).toBe(true);
    // failed nav must keep the current page content intact
    expect(document.getElementById("page-content")?.textContent).toEqual("HOME");
});

it("follows a page-location redirect to the target page", async () => {
    app.nav({ url: "/old" }).catch(() => { });

    await waitFor(() => document.getElementById("page-content")?.textContent === "TARGET");

    expect(document.getElementById("page-content")?.textContent).toEqual("TARGET");
    expect(fetchMock.mock.calls.some(c => String(c[0]).includes("/target"))).toBe(true);
});
