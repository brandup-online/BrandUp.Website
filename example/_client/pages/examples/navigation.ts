import { PageHashAction, PageHashChangedEvent, PageModel, PAGE_HASHCHANGED_EVENT } from "@brandup/ui-website";
import PageBase from "../base";
import "./navigation.less";

class ExampleNavigationPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.NavigationPage" }

    protected async onRenderContent() {
        await super.onRenderContent();

        console.log("render page");

        if (this.context.query.has("test")) {
            console.log("begin redirect from render");
            await this.context.redirect("/about");
        }

        if (this.context.query.has("error"))
            throw new Error("page error");

        this.on(PAGE_HASHCHANGED_EVENT, (e: PageHashChangedEvent) => {
            console.log(`${e.action} hash: ${e.prev} -> ${e.new}`);
        });
    }
}

export default ExampleNavigationPage;