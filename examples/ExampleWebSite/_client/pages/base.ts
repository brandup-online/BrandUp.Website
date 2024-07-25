import { Page, PageModel } from "@brandup/ui-website";
import "./base.less";

export default class PageBase<TModel extends PageModel = PageModel> extends Page<TModel> {
    get typeName(): string { return "Example.PageBase"; }
}