import { AjaxQueue, AjaxRequest } from "brandup-ui-ajax";
import { Application, ContextData, NavigateContext, NavigationOptions } from "brandup-ui-app";
import { Page } from "./page";

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
    title: string | null;
    canonicalLink: string | null;
    description: string | null;
    keywords: string | null;
    isAuthenticated: boolean;
    bodyClass: string | null;
    openGraph: PageOpenGraph | null;
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
    get current(): NavigationEntry | undefined;
    request(options: AjaxRequest, includeAntiforgery?: boolean): void;
    buildUrl(path?: string, queryParams?: { [key: string]: string }): string;
    nav(options: NavigationOptions): void;
    getScript(name: string): Promise<{ default: any }> | null;
}

export interface WebsiteNavigateData extends ContextData {
    website?: WebsiteContext;
    current?: NavigationEntry;
    new?: NavigationEntry;
}

export interface NavigationEntry {
    context: NavigateContext;
    url: string;
    hash: string | null;
    model: NavigationModel;
    page: Page;
}