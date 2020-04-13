import { PageClientModel } from "../brandup.pages/typings/website";
import { DOM } from "brandup-ui";
import MirSnaPage from "./base";
import minWait from "../utilities/wait";

export class FormPage<TModel extends PageClientModel> extends MirSnaPage<TModel> {
    private _isSubmitting: boolean;

    get typeName(): string { return "FormPage" }

    protected onRenderContent() {
        super.onRenderContent();

        const forms = DOM.queryElements(this.element, "form");
        forms.forEach((form: HTMLFormElement) => {
            form.addEventListener("submit", (e: Event) => {
                e.preventDefault();
                this.__submit(form);
            });
        });

        this.registerCommand("form-submit", (elem: HTMLButtonElement) => {
            const form = elem.closest("form") as HTMLFormElement;
            if (!form)
                return;
            this.__submit(form);
        });
    }

    private __submit(form: HTMLFormElement, url = "", handler: string = null) {
        if (!form.checkValidity())
            return;

        if (this._isSubmitting)
            return;
        this._isSubmitting = true;

        const submitButton = DOM.queryElement(form, "[type=submit]");
        if (submitButton)
            submitButton.classList.add("loading");
        form.classList.add("loading");

        if (!url)
            url = form.action;

        if (!handler)
            handler = form.getAttribute("data-form-handler");

        this._onFormSubmiting(form);

        const f = (data, status: number, xhr: XMLHttpRequest) => {
            if (status === 200 || status === 201) {
                this.onSubmitted(data, handler, status);

                const pageLocation = xhr.getResponseHeader("Page-Location");
                if (pageLocation) {
                    this.app.navigate(pageLocation);
                    return;
                }

                if (!this._onFormSubmited(form, data, status, xhr))
                    this.app.renderPage(data);
            }
            else if (status === 400) {
                this._isSubmitting = false;

                if (submitButton)
                    submitButton.classList.remove("loading");
                form.classList.remove("loading");

                this._onFormSubmitBadRequest(form, data);
            }
            else
                throw "";
        };

        this.queue.request({
            url: url,
            urlParams: { _content: "", handler: handler },
            method: "POST",
            type: "FORM",
            data: new FormData(form),
            success: submitButton ? minWait(f) : f
        });
    }

    protected onSubmitted(data: any, handler: string, status: number) { }

    protected _onFormSubmiting(form: HTMLFormElement) { }
    protected _onFormSubmited(form: HTMLFormElement, data: any, status: number, xhr: XMLHttpRequest): boolean { return false; }
    protected _onFormSubmitBadRequest(_form: HTMLFormElement, _response: any) { }
}

export default FormPage;