import { scriptReplace } from "../source/helpers/script";

describe("scriptReplace", () => {
    it("leaves application/json scripts untouched", () => {
        const parent = document.createElement("div");
        const json = document.createElement("script");
        json.type = "application/json";
        json.text = '{"a":1}';
        parent.appendChild(json);

        scriptReplace(parent);

        expect(parent.firstChild).toBe(json);
    });

    it("recreates executable scripts preserving attributes and content", () => {
        const parent = document.createElement("div");
        const js = document.createElement("script");
        js.type = "text/javascript";
        js.setAttribute("data-x", "1");
        js.text = "/* code */";
        parent.appendChild(js);

        scriptReplace(parent);

        const replaced = parent.querySelector("script");
        expect(replaced).not.toBeNull();
        expect(replaced).not.toBe(js);
        expect(replaced!.getAttribute("data-x")).toEqual("1");
        expect(replaced!.textContent).toEqual("/* code */");
    });

    it("recurses into nested elements", () => {
        const root = document.createElement("div");
        const wrapper = document.createElement("section");
        const js = document.createElement("script");
        js.type = "text/javascript";
        js.text = "1;";
        wrapper.appendChild(js);
        root.appendChild(wrapper);

        scriptReplace(root);

        const replaced = root.querySelector("script");
        expect(replaced).not.toBeNull();
        expect(replaced).not.toBe(js);
    });
});
