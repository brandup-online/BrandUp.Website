import { UIElement } from "@brandup/ui";

class TestElem extends UIElement {
    get typeName(): string { return "Components.Test"; }

    constructor(elem: HTMLElement) {
        super();

        this.setElement(elem);

        elem.innerHTML = "test component";
    }

    destroy() {
        super.destroy();
    }
}

export default TestElem;