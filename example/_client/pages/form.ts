import { PageModel } from "@brandup/ui-website";
import PageBase from "./base";
import "./form.less";

class FormPage extends PageBase<PageModel> {
    get typeName(): string { return "Example.FormPage" }
}

export default FormPage;