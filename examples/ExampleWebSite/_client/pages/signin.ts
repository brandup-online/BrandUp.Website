import { Page, PageModel } from "@brandup/ui-website";

class SignInPage extends Page<PageModel> {
    get typeName(): string { return "SignInPage" }

    async onRenderContent() {
        this.registerCommand("test", async (context) => {
            await this.submit();
        });

        return super.onRenderContent();
    }
}

export default SignInPage;