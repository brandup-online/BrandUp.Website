import { ApplicationModel, ContextData, NavigateContext } from "@brandup/ui-app";
import { Page } from "./page";

export interface WebsiteApplicationModel extends ApplicationModel {
    websiteId: string;
    antiforgery: AntiforgeryOptions;
}

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

export interface WebsiteNavigateData extends ContextData {
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