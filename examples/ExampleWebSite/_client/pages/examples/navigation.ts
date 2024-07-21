import { Page, PageModel } from "@brandup/ui-website";
import "./navigation.less";

class ExampleNavigationPage extends Page<PageModel> {
    get typeName(): string { return "ExampleNavigationPage" }
}

export default ExampleNavigationPage;