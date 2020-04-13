import Page from "../brandup.pages/pages/page";
import { PageClientModel } from "../brandup.pages/typings/website";

class ExamplePage<TModel extends PageClientModel> extends Page<TModel> {
    get typeName(): string { return "ExampleWebSite.Page" }

    protected onRenderContent() {
    }
}

export default ExamplePage;
