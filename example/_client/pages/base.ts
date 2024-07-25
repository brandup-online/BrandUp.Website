import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import "./base.less";

export default class PageBase<TModel extends PageModel = PageModel> extends Page<WebsiteApplication, TModel> {
    get typeName(): string { return "Example.PageBase"; }
}