import { DOM } from "@brandup/ui-dom";

const setMetadata = (name: "description" | "keywords", value: string | null | undefined) => {
    const elemId = `page-meta-${name}`;
    let elem = DOM.getById(elemId);
    if (value) {
        if (!elem)
            document.head.appendChild(elem = DOM.tag("meta", { id: elemId, name: name, content: "" }));

        elem.setAttribute("content", value);
    }
    else if (elem)
        elem.remove();
}

const setCanonical = (value: string | null | undefined) => {
    let elem = DOM.getById("page-link-canonical");
    if (value) {
        if (!elem)
            document.head.appendChild(elem = DOM.tag("link", { id: "page-link-canonical", rel: "canonical", href: "" }));

        elem.setAttribute("href", value);
    }
    else if (elem)
        elem.remove();
}

const setOG = (name: "type" | "title" | "image" | "url" | "site_name" | "description" | string, value: string | null | undefined) => {
    const elemId = `og-${name}`;
    let elem = DOM.getById(elemId);
    if (value) {
        if (!elem)
            document.head.appendChild(elem = DOM.tag("meta", { id: elemId, property: name, content: value }));

        elem.setAttribute("content", value);
    }
    else if (elem)
        elem.remove();
}

export {
    setMetadata,
    setCanonical,
    setOG
}