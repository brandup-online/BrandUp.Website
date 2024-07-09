const scriptReplace = (node: Node) => {
    if ((node as Element).tagName === "SCRIPT") {
        const source = <HTMLScriptElement>node;
        if (source.type.toLowerCase().includes("json"))
            return;

        const script = document.createElement("script");
        script.text = source.innerHTML;
        for (let i = source.attributes.length - 1; i >= 0; i--)
            script.setAttribute(source.attributes[i].name, source.attributes[i].value);
        node.parentNode.replaceChild(script, node);
    }
    else {
        let i = 0;
        const children = node.childNodes;
        while (i < children.length)
            scriptReplace(children[i++]);
    }

    return node;
};

export {
    scriptReplace
};