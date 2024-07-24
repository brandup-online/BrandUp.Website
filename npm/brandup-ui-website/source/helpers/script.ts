import { PreloadingDefinition } from "types";

const scriptReplace = (node: Node) => {
    if ((node as Element).tagName === "SCRIPT") {
        const source = <HTMLScriptElement>node;
        if (source.type.toLowerCase().includes("json"))
            return;

        const script = document.createElement("script");
        script.text = source.innerHTML;
        for (let i = source.attributes.length - 1; i >= 0; i--)
            script.setAttribute(source.attributes[i].name, source.attributes[i].value);
        node.parentNode?.replaceChild(script, node);
    }
    else {
        let i = 0;
        const children = node.childNodes;
        while (i < children.length)
            scriptReplace(children[i++]);
    }

    return node;
};

const preloadDefinitions = (defs?: { [name: string]: PreloadingDefinition }) => {
    if (!defs)
        return;

    const loadings: Promise<any>[] = [];
    for (let name in defs) {
        const def = defs[name];
        if (def.preload)
            loadings.push(def.factory());
    }

    if (loadings.length) {
        Promise.allSettled(loadings)
            .then(() => console.log(`preloaded`));
    }
}

export {
    scriptReplace,
    preloadDefinitions
};