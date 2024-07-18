import { Application, ApplicationModel } from "brandup-ui-app";

export class WebsiteApplication<TModel extends ApplicationModel = {}> extends Application<TModel> {
    async pageHandler() {
        await this.invoker.invoke("pageHandler", { data: {} });
    }
}