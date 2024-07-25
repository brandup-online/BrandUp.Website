import { PageModel } from "@brandup/ui-website";
import PageBase from "./base";

class SignInPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.SignInPage" }

    async onRenderContent() {
        this.registerCommand("test", async (context) => {
            await this.submit();
        });

        return super.onRenderContent();
    }
}

export default SignInPage;