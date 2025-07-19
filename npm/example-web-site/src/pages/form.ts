import { PageModel } from "@brandup/ui-website";
import PageBase from "./base";
import "./form.less";

class FormPage<T extends PageModel> extends PageBase<T> {
    get typeName(): string { return "Example.FormPage" }
}

export default FormPage;