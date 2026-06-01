import { PageModel } from "@brandup/ui-website";
import PageBase from "./base";

class SignInPage extends PageBase<PageModel> {
    override get typeName(): string { return "Example.SignInPage" }

    override async onRenderContent() {
        this.registerCommand("test", async (_context) => {
            await this.submit();
        });

        return super.onRenderContent();
    }
}

export default SignInPage;
