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
    [key: string]: unknown;
}

/** Свойства Open Graph: плоский словарь "имя свойства og:* → значение" (включая пользовательские). */
export interface PageOpenGraph {
    [name: string]: string;
}

export interface PageModel {
    type: string | null;
    [key: string]: unknown;
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

    renderComponents(container: Page): Promise<void>;
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

export interface PreloadingDefinition<T = { default: unknown }> {
    factory: () => Promise<T>;
    preload?: boolean;
}

export type PageScript = { default: new (...args: any[]) => Page };
export type ComponentScript = { default: new (...args: any[]) => UIElement };

/** Состояние навигации в window.history.state */
export interface HistoryState {
    [key: string]: unknown;
    brandup_website?: {
        id: string;
        scroll?: { x: number; y: number; }
    };
}