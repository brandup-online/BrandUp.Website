import { PageModel } from "@brandup/ui-website";
import PageBase from "../base";
import "./navigation.less";

class ExampleNavigationPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.NavigationPage" }

    protected async onRenderContent() {
        await super.onRenderContent();

        if (this.context.query.has("test")) {
            console.log("begin redirect from render");
            await this.website.nav({ url: "/about" });
        }

        if (this.context.query.has("error"))
            throw new Error("page error")
    }
}

export default ExampleNavigationPage;