import { Page, PageModel } from "@brandup/ui-website";
import "./form.less";

class FormPage extends Page<PageModel> {
    get typeName(): string { return "FormPage" }
}

export default FormPage;