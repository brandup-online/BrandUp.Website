import { DOM } from "@brandup/ui";
import { WEBSITE, NavigationModel } from "../source/index";

const pagePath = "/consent?returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dx&tab=2";

const firstNav: NavigationModel = {
    url: "http://localhost" + pagePath, path: "/consent", query: { returnUrl: "/connect/authorize?client_id=x", tab: "2" },
    validationToken: "tok1", state: "state1",
    title: "Consent", canonicalLink: null, description: null, keywords: null,
    isAuthenticated: false, bodyClass: null, openGraph: null, page: { type: null }
};

let fetchMock: jest.Mock;

const waitFor = async (predicate: () => boolean, timeout = 2000) => {
    const start = Date.now();
    while (Date.now() - start < timeout) {
        if (predicate()) return;
        await new Promise(r => setTimeout(r, 20));
    }
    throw new Error("Condition not met within timeout.");
};

const submitUrls = () => fetchMock.mock.calls.map(c => new URL(String(c[0]), "http://localhost")).filter(it => it.pathname === "/consent" && it.searchParams.has("returnUrl"));

beforeAll(async () => {
    window.scrollTo = (() => { }) as typeof window.scrollTo;
    window.history.replaceState(null, "", pagePath);

    // Same shim as in submit.test.ts: the test environment FormData cannot be built from a form.
    const BaseFormData = global.FormData;
    class FormDataFromForm extends BaseFormData {
        constructor(form?: HTMLFormElement) {
            super();
            if (form) {
                for (const element of Array.from(form.elements) as HTMLInputElement[]) {
                    if (element.name)
                        this.append(element.name, element.value ?? "");
                }
            }
        }
    }
    global.FormData = FormDataFromForm as unknown as typeof FormData;

    const appDataScript = <HTMLScriptElement>DOM.tag("script", { id: "app-data", type: "application/json" });
    appDataScript.text = '{ "env": { "basePath": "/" }, "model": { "websiteId": "id", "antiforgery": { "headerName": "X-CSRF", "formFieldName": "__rvt" } } }';
    document.head.insertAdjacentElement("beforeend", appDataScript);

    const navDataScript = <HTMLScriptElement>DOM.tag("script", { id: "nav-data", type: "application/json" });
    navDataScript.text = JSON.stringify(firstNav);
    document.body.insertAdjacentElement("beforeend", navDataScript);

    const pageContent = DOM.tag("div", { id: "page-content" });
    pageContent.innerHTML = '<form class="appform no-action" method="post"><input name="a" value="1"></form>'
        + '<form class="appform no-query" method="post" action="/consent"><input name="a" value="1"></form>';
    document.body.insertAdjacentElement("beforeend", pageContent);

    fetchMock = jest.fn(() => Promise.resolve(new Response('<div id="page-content">SUBMITTED</div>', { status: 200, headers: { "content-type": "text/html" } })));
    global.fetch = fetchMock as unknown as typeof fetch;

    await WEBSITE.run({ submitMinTime: 0, pages: {} }, () => { });
});

it("does not repeat the page query when the form posts to the page address", async () => {
    const form = document.querySelector("form.no-action") as HTMLFormElement;
    form.dispatchEvent(new SubmitEvent("submit", { bubbles: true, cancelable: true }));

    await waitFor(() => submitUrls().length === 1);

    const url = submitUrls()[0];
    expect(url.searchParams.getAll("returnUrl")).toEqual(["/connect/authorize?client_id=x"]);
    expect(url.searchParams.getAll("tab")).toEqual(["2"]);
});

it("keeps the page query when the form action has none", async () => {
    document.getElementById("page-content")!.innerHTML = '<form class="appform no-query" method="post" action="/consent"><input name="a" value="1"></form>';
    fetchMock.mockClear();

    const form = document.querySelector("form.no-query") as HTMLFormElement;
    form.dispatchEvent(new SubmitEvent("submit", { bubbles: true, cancelable: true }));

    await waitFor(() => submitUrls().length === 1);

    const url = submitUrls()[0];
    expect(url.searchParams.getAll("returnUrl")).toEqual(["/connect/authorize?client_id=x"]);
    expect(url.searchParams.getAll("tab")).toEqual(["2"]);
});
