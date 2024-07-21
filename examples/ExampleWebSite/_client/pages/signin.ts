import { Page, PageModel } from "@brandup/ui-website";

class SignInPage extends Page<PageModel> {
    get typeName(): string { return "SignInPage" }

    async onRenderContent() {
        this.registerAsyncCommand("test", (context) => {
            this.submit().finally(() => context.complate());
        });

        return super.onRenderContent();
    }
}

export default SignInPage;