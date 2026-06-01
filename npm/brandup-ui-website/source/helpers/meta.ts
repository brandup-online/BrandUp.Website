import { DOM } from "@brandup/ui";

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
            document.head.appendChild(elem = DOM.tag("meta", { id: elemId, property: `og:${name}`, content: value }));

        elem.setAttribute("content", value);
    }
    else if (elem)
        elem.remove();
}

const setOpenGraph = (props: { [name: string]: string } | null | undefined) => {
    // Удаляем только управляемые нами og-теги (по id-префиксу),
    // не трогая og:-теги, добавленные сторонним кодом или сервером без id.
    document.head.querySelectorAll("meta[id^=\"og-\"]").forEach(elem => elem.remove());

    if (!props)
        return;

    for (const name in props) {
        const value = props[name];
        if (!value)
            continue;

        document.head.appendChild(DOM.tag("meta", { id: `og-${name}`, property: `og:${name}`, content: value }));
    }
}

export {
    setMetadata,
    setCanonical,
    setOG,
    setOpenGraph
}