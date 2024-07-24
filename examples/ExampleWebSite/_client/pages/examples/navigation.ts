import { Page, PageModel } from "@brandup/ui-website";
import { UIElement } from "@brandup/ui";
import "./navigation.less";

class ExampleNavigationPage extends Page<PageModel> {
    get typeName(): string { return "ExampleNavigationPage" }
}

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


export default ExampleNavigationPage;