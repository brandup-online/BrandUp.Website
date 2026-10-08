import { DOM } from "@brandup/ui";
import { WEBSITE, NavigationModel, NavigationScroll, Page } from "../source/index";

const buildNav = (): NavigationModel => ({
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
    page: { type: null }
});

let scrollMock: jest.Mock;

beforeAll(async () => {
    window.scrollTo = (() => { }) as typeof window.scrollTo;
    scrollMock = jest.fn();
    window.scroll = scrollMock as unknown as typeof window.scroll;

    // Состояние записи, пережившее перезагрузку: позицию записал pagehide прошлого документа.
    window.history.replaceState({ brandup_website: { id: "prev", scroll: { x: 0, y: 500 } } }, "");

    const appDataScript = <HTMLScriptElement>DOM.tag("script", { id: "app-data", type: "application/json" });
    appDataScript.text = '{ "env": { "basePath": "/" }, "model": { "websiteId": "id" } }';
    document.head.insertAdjacentElement("beforeend", appDataScript);

    const navDataScript = <HTMLScriptElement>DOM.tag("script", { id: "nav-data", type: "application/json" });
    navDataScript.text = JSON.stringify(buildNav());
    document.body.insertAdjacentElement("beforeend", navDataScript);
    document.body.insertAdjacentElement("beforeend", DOM.tag("div", { id: "page-content" }, "HOME"));

    await WEBSITE.run({
        pages: { "test": { factory: () => Promise.resolve({ default: Page }) } }
    }, () => { });
});

it("turns off the browser's own scroll restoration", () => {
    expect(window.history.scrollRestoration).toEqual("manual");
});

it("restores the saved position on the first navigation", () => {
    expect(scrollMock).toHaveBeenCalledWith({ top: 500, left: 0, behavior: "instant" });
});

it("stops restoring once the user scrolls on their own", () => {
    // Документ в jsdom не растёт, поэтому восстановление первой навигации всё ещё повторяется.
    jest.useFakeTimers();
    try {
        window.dispatchEvent(new Event("wheel"));
        scrollMock.mockClear();

        jest.advanceTimersByTime(1000);
        expect(scrollMock).not.toHaveBeenCalled();
    }
    finally {
        jest.useRealTimers();
    }
});

it("restores the position of a hash-only entry on back and forward", async () => {
    window.history.pushState({ brandup_website: { id: "hash-entry", scroll: { x: 0, y: 700 } } }, "", "/#section");
    scrollMock.mockClear();

    window.dispatchEvent(new PopStateEvent("popstate", { state: window.history.state }));
    await new Promise(resolve => setTimeout(resolve, 50));

    expect(scrollMock).toHaveBeenCalledWith({ top: 700, left: 0, behavior: "instant" });
});

describe("restore on a document still growing", () => {
    let maxY = 0;
    const originalScroll = window.scroll;

    beforeEach(() => {
        jest.useFakeTimers();
        maxY = 1500;
        Object.defineProperty(window, "scrollY", { configurable: true, writable: true, value: 0 });
        // Окно упирается в текущий низ документа, как в браузере, пока он не дорос.
        window.scroll = ((options: ScrollToOptions) => {
            (window as { scrollY: number }).scrollY = Math.min(options.top ?? 0, maxY);
        }) as typeof window.scroll;
        window.history.replaceState({ brandup_website: { id: "growing", scroll: { x: 0, y: 3000 } } }, "");
    });

    afterEach(() => {
        jest.useRealTimers();
        window.scroll = originalScroll;
    });

    it("keeps applying the position until the document reaches it", () => {
        const scroll = new NavigationScroll(() => "growing");
        try {
            scroll.restore(window.history.state);
            expect(window.scrollY).toEqual(1500);

            maxY = 5000;
            jest.advanceTimersByTime(100);
            expect(window.scrollY).toEqual(3000);
        }
        finally {
            scroll.destroy();
        }
    });

    it("keeps the saved position when the page is left before the document reaches it", () => {
        const scroll = new NavigationScroll(() => "growing");
        try {
            scroll.restore(window.history.state);
            window.dispatchEvent(new Event("scroll"));
            jest.advanceTimersByTime(200);

            scroll.save();
            expect(window.history.state.brandup_website.scroll).toEqual({ x: 0, y: 3000 });

            // Восстановление снято: дальше позиция пишется как обычно.
            maxY = 5000;
            jest.advanceTimersByTime(1000);
            expect(window.scrollY).toEqual(1500);
        }
        finally {
            scroll.destroy();
        }
    });

    it("drops a write scheduled just before the timeout, while the document was still short", () => {
        const scroll = new NavigationScroll(() => "growing");
        try {
            scroll.restore(window.history.state);

            // Документ подрос, но не до конца: окно сдвинулось, и прокрутка поставила отложенную запись.
            jest.advanceTimersByTime(4850);
            maxY = 2000;
            jest.advanceTimersByTime(100);
            window.dispatchEvent(new Event("scroll"));

            // Таймаут истекает раньше, чем отложенная запись.
            jest.advanceTimersByTime(1000);
            expect(window.scrollY).toEqual(2000);
            expect(window.history.state.brandup_website.scroll).toEqual({ x: 0, y: 3000 });
        }
        finally {
            scroll.destroy();
        }
    });

    it("stops on user input that a component keeps from bubbling", () => {
        const popup = document.body.appendChild(DOM.tag("div"));
        popup.addEventListener("wheel", e => e.stopPropagation());

        const scroll = new NavigationScroll(() => "growing");
        try {
            scroll.restore(window.history.state);
            popup.dispatchEvent(new Event("wheel", { bubbles: true }));

            maxY = 5000;
            jest.advanceTimersByTime(1000);
            expect(window.scrollY).toEqual(1500);
        }
        finally {
            scroll.destroy();
            popup.remove();
        }
    });

    it("gives up after the timeout and leaves the saved position in place", () => {
        const scroll = new NavigationScroll(() => "growing");
        try {
            scroll.restore(window.history.state);
            jest.advanceTimersByTime(6000);

            maxY = 5000;
            jest.advanceTimersByTime(1000);
            expect(window.scrollY).toEqual(1500);
            expect(window.history.state.brandup_website.scroll).toEqual({ x: 0, y: 3000 });
        }
        finally {
            scroll.destroy();
        }
    });
});

it("gives the previous scroll restoration mode back on destroy", () => {
    window.history.scrollRestoration = "auto";

    const scroll = new NavigationScroll(() => undefined);
    expect(window.history.scrollRestoration).toEqual("manual");

    scroll.destroy();
    expect(window.history.scrollRestoration).toEqual("auto");

    window.history.scrollRestoration = "manual";
});
