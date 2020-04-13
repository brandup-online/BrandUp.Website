import { AjaxRequestOptions } from "brandup-ui";

export interface PageNavState {
    url: string;
    title: string;
    path: string;
    hash: string;
    params: { [key: string]: string; };
}

export interface AppClientModel {
    baseUrl: string;
    nav: NavigationModel;
    antiforgery: AntiforgeryModel;
}

export interface AntiforgeryModel {
    headerName: string;
    formFieldName: string;
}

export interface NavigationModel {
    enableAdministration: boolean;
    isAuthenticated: boolean,
    url: string;
    path: string;
    query: { [key: string]: string; };
    validationToken: string;
    state: string;
    page: PageClientModel;
}

export interface PageClientModel {
    title: string;
    cssClass: string;
    scriptName: string;
    canonicalLink: string;
    description: string;
    keywords: string;
}

export interface IApplication {
    model: any;
    navigation: NavigationModel;
    request(options: AjaxRequestOptions)
    uri(path?: string, queryParams?: { [key: string]: string; }): string;
    reload();
    navigate(target: any);
    nav(options: NavigationOptions);
    script(name: string): Promise<{ default: any }>;
    renderPage(html: string);
}

export interface NavigationOptions {
    url: string;
    hash?: string;
    pushState: boolean;
    notRenderPage?: boolean;
    scrollToTop?: boolean;
    success?: () => void;
}

export interface IPage {
    app: IApplication;

    update(nav: PageNavState, model: any)
    refreshScripts();
}