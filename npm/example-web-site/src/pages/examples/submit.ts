import { PageModel } from "@brandup/ui-website";
import FormPage from "../form";
import "./navigation.less";

class ExampleNavigationPage extends FormPage<PageModel> {
    get typeName(): string { return "Example.SubmitPage" }

    protected async onRenderContent() {
        await super.onRenderContent();

        this.registerCommand("reload", () => {
            this.context.app.reload();
        });

        this.registerCommand("test", () => {
            this.submit();
        })
    }
}

export default ExampleNavigationPage;