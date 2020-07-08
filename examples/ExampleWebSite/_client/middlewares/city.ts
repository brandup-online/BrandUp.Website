import { Middleware, NavigateContext, StartContext, LoadContext, NavigatingContext } from "brandup-ui-app";
import { DOM } from "brandup-ui";

const clickRet = (clickElem: HTMLElement, scopeSelector: string, cssClass: string, on: () => void, off?: () => void) => {
    const scopeElem = clickElem.closest(scopeSelector);

    const clickFunc = (e: MouseEvent) => {
        console.log("clickFunc");

        const target = e.target as HTMLElement;
        if (!target.closest(scopeSelector)) {
            scopeElem.classList.remove(cssClass);
            off();

            document.body.removeEventListener("click", clickFunc, { capture: false });
        }
    };

    if (!scopeElem) {
        document.body.removeEventListener("click", clickFunc, { capture: false });
        return;
    }

    if (scopeElem.classList.toggle(cssClass)) {
        on();

        clickElem["___clickRet"] = clickFunc;
        document.body.addEventListener("click", clickFunc, { capture: false });
    }
    else {
        off();
        document.body.removeEventListener("click", clickElem["___clickRet"], { capture: false });
    }
};

export class CityMiddleware extends Middleware {
    start(_context: StartContext, next) {
        next();

        this.app.registerCommand("toggle-city", (elem) => {
            clickRet(elem, ".city", "expanded", () => {
                let cityList: HTMLElement;
                const cityPopup = DOM.tag("div", { class: "cities" }, cityList = DOM.tag("ul"));

                cityList.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "" }, "Москва")));
                cityList.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "" }, "Новосибирск")));

                elem.insertAdjacentElement("afterend", cityPopup);
            }, () => { elem.nextElementSibling.remove(); });
        });
    }

    loaded(_context: LoadContext, next) {
        next();
    }

    navigating(_context: NavigatingContext, next) {
        next();
    }

    navigate(_context: NavigateContext, next) {
        next();
    }
}