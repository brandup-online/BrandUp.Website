import { PageModel } from "@brandup/ui-website";
import PageBase from "../base";
import "./navigation.less";

class ExampleNavigationPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.NavigationPage" }

    protected async onRenderContent() {
        await super.onRenderContent();

        if (this.nav.query["test"]) {
            console.log("begin redirect from render");
            await this.website.nav({ url: "/about" });
        }
    }
}

export default ExampleNavigationPage;