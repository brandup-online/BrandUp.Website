import { Application } from "./brandup.pages/app";
import { AppClientModel, PageNavState } from "./brandup.pages/typings/website";
import { DOM, ajaxRequest } from "brandup-ui";
import "./styles.less";

export const appManager = Application.setup<WebsiteClientModel>({
    configure: (builder) => {
        builder.addPageType("page", () => import("./pages/base"));
        builder.addPageType("form", () => import("./pages/form"));
    }
}, (app) => {
});

interface WebsiteClientModel extends AppClientModel {
    city: CityClientModel;
    cities: Array<CityClientModel>;
    customerId: string;
}

interface CityClientModel {
    title: string;
    url: string;
}