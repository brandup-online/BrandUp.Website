import { AjaxQueue, AjaxRequest } from "brandup-ui-ajax";
import { Application, NavigationOptions } from "brandup-ui-app";

export interface AntiforgeryOptions {
    headerName: string;
    formFieldName: string;
}

export interface NavigationModel {
    url: string;
    path: string;
    query: { [key: string]: string; };
    validationToken: string;
    state: string;
    title: string;
    canonicalLink: string;
    description: string;
    keywords: string;
    isAuthenticated: boolean;
    bodyClass: string;
    openGraph: PageOpenGraph;
    page: PageModel;
    [key: string]: any;
}

export interface PageOpenGraph {
    type: string;
    image: string;
    title: string;
    url: string;
    siteName: string;
    description: string;
}

export interface PageModel {
    type: string;
    [key: string]: any;
}

export interface WebsiteContext {
    readonly app: Application;
    readonly antiforgery: AntiforgeryOptions;
    readonly queue: AjaxQueue;
    get id(): string;
    get validationToken(): string | null;
    request(options: AjaxRequest, includeAntiforgery?: boolean);
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string;
    nav(options: NavigationOptions);
    getScript(name: string): Promise<{ default: any }> | null;
}