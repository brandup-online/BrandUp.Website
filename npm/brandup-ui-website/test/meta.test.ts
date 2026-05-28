import { setMetadata, setCanonical, setOG } from "../source/helpers/meta";

beforeEach(() => {
    document.head.innerHTML = "";
});

describe("setMetadata", () => {
    it("creates meta when value provided", () => {
        setMetadata("description", "hello");

        const elem = document.getElementById("page-meta-description");
        expect(elem).not.toBeNull();
        expect(elem!.getAttribute("name")).toEqual("description");
        expect(elem!.getAttribute("content")).toEqual("hello");
    });

    it("updates existing meta content", () => {
        setMetadata("keywords", "a");
        setMetadata("keywords", "b");

        const elems = document.querySelectorAll("#page-meta-keywords");
        expect(elems.length).toEqual(1);
        expect(elems[0].getAttribute("content")).toEqual("b");
    });

    it("removes meta when value is empty", () => {
        setMetadata("description", "hello");
        setMetadata("description", null);

        expect(document.getElementById("page-meta-description")).toBeNull();
    });
});

describe("setCanonical", () => {
    it("creates and removes canonical link", () => {
        setCanonical("https://example.org/");
        const elem = document.getElementById("page-link-canonical");
        expect(elem).not.toBeNull();
        expect(elem!.getAttribute("rel")).toEqual("canonical");
        expect(elem!.getAttribute("href")).toEqual("https://example.org/");

        setCanonical(null);
        expect(document.getElementById("page-link-canonical")).toBeNull();
    });
});

describe("setOG", () => {
    it("creates og meta with og: property prefix", () => {
        setOG("title", "Title");

        const elem = document.getElementById("og-title");
        expect(elem).not.toBeNull();
        expect(elem!.getAttribute("property")).toEqual("og:title");
        expect(elem!.getAttribute("content")).toEqual("Title");
    });

    it("updates server-rendered og meta without duplicating", () => {
        const existing = document.createElement("meta");
        existing.id = "og-type";
        existing.setAttribute("property", "og:type");
        existing.setAttribute("content", "website");
        document.head.appendChild(existing);

        setOG("type", "article");

        const elems = document.querySelectorAll("#og-type");
        expect(elems.length).toEqual(1);
        expect(elems[0].getAttribute("content")).toEqual("article");
        expect(elems[0].getAttribute("property")).toEqual("og:type");
    });

    it("removes og meta when value is empty", () => {
        setOG("image", "https://example.org/og.jpg");
        setOG("image", null);

        expect(document.getElementById("og-image")).toBeNull();
    });
});
