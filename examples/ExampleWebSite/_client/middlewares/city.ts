import { ajaxRequest, AjaxResponse } from "@brandup/ui-ajax";
import { Middleware, MiddlewareNext, NavigateContext, StartContext, SubmitContext } from "@brandup/ui-app";
import { DOM } from "@brandup/ui-dom";
import { WebsiteContext } from "@brandup/ui-website";

const clickRet = (clickElem: HTMLElement, scopeSelector: string, cssClass: string, on: () => void, off?: () => void) => {
    const scopeElem = clickElem.closest(scopeSelector);

    const clickFunc = (e: MouseEvent) => {
        const target = e.target as HTMLElement;
        if (!target.closest(scopeSelector)) {
            scopeElem?.classList.remove(cssClass);
            if (off)
                off();

            document.body.removeEventListener("click", (<any>clickElem)["___clickRet"], { capture: false });
        }
    };

    if (!scopeElem) {
        document.body.removeEventListener("click", clickFunc, { capture: false });
        return;
    }

    if (scopeElem.classList.toggle(cssClass)) {
        on();

        (<any>clickElem)["___clickRet"] = clickFunc;
        document.body.addEventListener("click", clickFunc, { capture: false });
    }
    else {
        if (off)
            off();
        document.body.removeEventListener("click", (<any>clickElem)["___clickRet"], { capture: false });
    }
};

export class CityMiddleware implements Middleware {
    readonly name: string = "city";

    async start(context: StartContext, next: MiddlewareNext) {
        await next();

        context.app.registerAsyncCommand("toggle-city", (ctx) => {
            clickRet(ctx.target, ".city", "expanded", () => {
                const request = ajaxRequest({
                    url: context.app.buildUrl("api/city"),
                    success: (response: AjaxResponse<Array<CityModel>>) => {
                        if (response.status !== 200)
                            return;

                        let cityList: HTMLElement;
                        const cityPopup = DOM.tag("div", { class: "cities" }, cityList = DOM.tag("ul"));

                        response.data?.forEach((it) => {
                            cityList.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-name": it.name, "data-command": "select-city" }, it.title)));
                        });

                        ctx.target.insertAdjacentElement("afterend", cityPopup);

                        ctx.complate();
                    }
                });

                ctx.timeoutCallback = () => {
                    request.abort();
                };
            }, () => {
                ctx.target.nextElementSibling?.remove();
                ctx.complate();
            });
        });

        context.app.registerCommand("select-city", (el) => {
            const cityName = el.getAttribute("data-name");
            alert(cityName);
        });
    }
}