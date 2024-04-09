var nodeScriptReplace = function (node) {
    if (node.tagName === "SCRIPT") {
        var source = node;
        if (source.type.toLowerCase().includes("json"))
            return;
        var script = document.createElement("script");
        script.text = source.innerHTML;
        for (var i = source.attributes.length - 1; i >= 0; i--)
            script.setAttribute(source.attributes[i].name, source.attributes[i].value);
        node.parentNode.replaceChild(script, node);
    }
    else {
        var i = 0;
        var children = node.childNodes;
        while (i < children.length)
            nodeScriptReplace(children[i++]);
    }
    return node;
};
export default nodeScriptReplace;
