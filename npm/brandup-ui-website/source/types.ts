import { ApplicationModel, ContextData, Middleware, NavigateContext } from "@brandup/ui-app";
import { Page } from "./page";
import { UIElement } from "@brandup/ui";
import { AjaxRequest } from "@brandup/ui-ajax";

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
    type: string | null;
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

export interface WebsiteMiddleware extends Middleware {
    get current(): Readonly<NavigationEntry | undefined>;
    get validationToken(): string | null;

    renderComponents(container: UIElement): Promise<void>;
    findComponent(name: string): (() => Promise<ComponentScript>) | null;
    prepareRequest(request: AjaxRequest): void;
}

export interface WebsiteOptions {
    defaultPage?: string;
    pages?: { [key: string]: PageDefinition };
    components?: { [key: string]: ComponentDefinition };
    navMinTime?: number;
    submitMinTime?: number;
}

export interface PageDefinition extends PreloadingDefinition<PageScript> {
}

export interface ComponentDefinition extends PreloadingDefinition<ComponentScript> {
}

export interface PreloadingDefinition<T = { default: any }> {
    factory: () => Promise<T>;
    preload?: boolean;
}

export type PageScript = { default: typeof Page | any };
export type ComponentScript = { default: typeof UIElement | any };