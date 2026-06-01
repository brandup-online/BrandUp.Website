import { DOM } from "@brandup/ui";
import { WEBSITE, NavigationModel } from "../source/index";

const firstNav: NavigationModel = {
    url: "http://localhost/", path: "/", query: {}, validationToken: "tok1", state: "state1",
    title: "Home", canonicalLink: null, description: null, keywords: null,
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

beforeAll(async () => {
    window.scrollTo = (() => { }) as typeof window.scrollTo;
    // The node FormData provided by the test environment cannot be constructed from a form
    // element (a browser-only feature the middleware relies on), so shim it for the test.
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
    pageContent.innerHTML = '<form class="appform" method="post" action="/submit"><input name="a" value="1"></form>';
    document.body.insertAdjacentElement("beforeend", pageContent);

    fetchMock = jest.fn((input: RequestInfo | URL) => {
        const url = typeof input === "string" ? input : input.toString();
        if (url.includes("/submit"))
            return Promise.resolve(new Response('<div id="page-content">SUBMITTED</div>', { status: 200, headers: { "content-type": "text/html" } }));
        return Promise.resolve(new Response("{}", { status: 200, headers: { "content-type": "application/json" } }));
    });
    global.fetch = fetchMock as unknown as typeof fetch;

    await WEBSITE.run({ submitMinTime: 0, pages: {} }, () => { });
});

it("re-renders page content from an html submit response", async () => {
    const form = document.querySelector("form.appform") as HTMLFormElement;
    form.dispatchEvent(new SubmitEvent("submit", { bubbles: true, cancelable: true }));

    await waitFor(() => document.getElementById("page-content")?.textContent === "SUBMITTED");

    expect(document.getElementById("page-content")?.textContent).toEqual("SUBMITTED");
});

it("sends the antiforgery header with the submit request", () => {
    const submitCall = fetchMock.mock.calls.find(c => String(c[0]).includes("/submit"));
    expect(submitCall).toBeDefined();

    const headers = submitCall![1]!.headers as any;
    const headerValue = headers instanceof Headers ? headers.get("X-CSRF") : headers["X-CSRF"];
    expect(headerValue).toEqual("tok1");
});
