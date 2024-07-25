import { PageModel } from "@brandup/ui-website";
import PageBase from "../base";
import "./navigation.less";

class ExampleNavigationPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.NavigationPage" }
}

export default ExampleNavigationPage;