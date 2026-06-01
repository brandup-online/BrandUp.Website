import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import "./base.less";

export default class PageBase<TModel extends PageModel = PageModel> extends Page<WebsiteApplication, TModel> {
    override get typeName(): string { return "Example.PageBase"; }

    protected override async onRenderedContent() {
        console.log("page rendered");
    }
}