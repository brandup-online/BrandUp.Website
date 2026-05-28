import { DOM } from "@brandup/ui-dom";
import { WEBSITE, NavigationModel, Page, WebsiteApplication } from "../source/index";

const buildNav = (over: Partial<NavigationModel>): NavigationModel => ({
    url: "http://localhost/",
    path: "/",
    query: {},
    validationToken: "tok1",
    state: "state1",
    title: "Home",
    canonicalLink: null,
    description: null,
    keywords: null,
    isAuthenticated: false,
    bodyClass: null,
    openGraph: null,
    page: { type: null },
    ...over
});

const page2Html =
    `<script id="nav-data" type="application/json">${JSON.stringify(buildNav({ url: "http://localhost/page2", path: "/page2", validationToken: "tok2", state: "state2", title: "Page 2" }))}</script>` +
    `<div id="page-content">PAGE2</div>`;

let app: WebsiteApplication;
let fetchMock: jest.Mock;

beforeAll(async () => {
    window.scrollTo = (() => { }) as typeof window.scrollTo;

    const appDataScript = <HTMLScriptElement>DOM.tag("script", { id: "app-data", type: "application/json" });
    appDataScript.text = '{ "env": { "basePath": "/" }, "model": { "websiteId": "id", "antiforgery": { "headerName": "X-CSRF", "formFieldName": "__rvt" } } }';
    document.head.insertAdjacentElement("beforeend", appDataScript);

    const navDataScript = <HTMLScriptElement>DOM.tag("script", { id: "nav-data", type: "application/json" });
    navDataScript.text = JSON.stringify(buildNav({}));
    document.body.insertAdjacentElement("beforeend", navDataScript);
    document.body.insertAdjacentElement("beforeend", DOM.tag("div", { id: "page-content" }, "HOME"));

    fetchMock = jest.fn((input: RequestInfo | URL) => {
        const url = typeof input === "string" ? input : input.toString();
        if (url.includes("/page2"))
            return Promise.resolve(new Response(page2Html, { status: 200, headers: { "content-type": "text/html" } }));
        return Promise.resolve(new Response("{}", { status: 200, headers: { "content-type": "application/json" } }));
    });
    global.fetch = fetchMock as unknown as typeof fetch;

    const context = await WEBSITE.run({
        pages: { "test": { factory: () => Promise.resolve({ default: Page }) } }
    }, () => { });

    app = context.app;
});

it("renders a new page from an http navigation", async () => {
    const pushSpy = jest.spyOn(window.history, "pushState");

    await app.nav({ url: "/page2" });

    expect(document.getElementById("page-content")?.textContent).toEqual("PAGE2");
    expect(document.title).toEqual("Page 2");
    expect(fetchMock.mock.calls.some(c => String(c[0]).includes("/page2"))).toBe(true);
    expect(pushSpy.mock.calls.some(c => String(c[2]).includes("/page2"))).toBe(true);

    pushSpy.mockRestore();
});

it("adds the antiforgery header to non-GET requests", async () => {
    fetchMock.mockClear();

    await app.request({ method: "POST", url: "/api/x" });

    const postCall = fetchMock.mock.calls.find(c => (c[1]?.method ?? "").toUpperCase() === "POST");
    expect(postCall).toBeDefined();

    const headers = postCall![1]!.headers as any;
    const headerValue = headers instanceof Headers ? headers.get("X-CSRF") : headers["X-CSRF"];
    expect(headerValue).toEqual("tok2");
});
