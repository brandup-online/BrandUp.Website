import { Page, PageModel } from "brandup-ui-website";

class SignInPage extends Page<PageModel> {
    get typeName(): string { return "SignInPage" }

    onRenderContent() {
        this.registerCommand("test", () => {
            this.submit();
        });

        super.onRenderContent();
    }
}

export default SignInPage;