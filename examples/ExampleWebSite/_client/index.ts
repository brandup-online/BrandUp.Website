import { Application } from "./brandup.pages/app";
import { AppClientModel } from "./brandup.pages/typings/website";
import "./styles.less";
import { DOM, ajaxRequest } from "brandup-ui";

const initApp = (app: Application<WebsiteClientModel>) => {
    let isRenderCities = false;
    app.registerCommand("toggle-city", (elem) => {
        if (!isRenderCities) {
            isRenderCities = true;
            elem.insertAdjacentElement("afterend", DOM.tag("div", { class: "cities" }, DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-city", "data-city-id": "1" }, "Москва")),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-city", "data-city-id": "2" }, "Новосибирск")),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-city", "data-city-id": "3" }, "Омск")),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-city", "data-city-id": "4" }, "Томск"))
            ])));
        }

        document.body.classList.toggle("show-cities");
    });

    app.registerCommand("select-city", (elem) => {
        const cityId = elem.getAttribute("data-city-id");
        if (cityId) {
            ajaxRequest({
                url: app.uri("api/app/changeCity"),
                urlParams: { id: cityId },
                method: "POST",
                success: () => {

                }
            });
        }
        document.body.classList.remove("show-cities");
    });
}

export const appManager = Application.setup<WebsiteClientModel>({
    configure: (builder) => {
        builder.addPageType("page", () => import("./pages/base"));
        builder.addPageType("form", () => import("./pages/form"));
    }
}, initApp);

interface WebsiteClientModel extends AppClientModel {
    city: CityClientModel;
    cities: Array<CityClientModel>;
    customerId: string;
}

interface CityClientModel {
    title: string;
    url: string;
}