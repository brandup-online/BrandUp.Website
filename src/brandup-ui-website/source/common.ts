export interface AntiforgeryOptions {
    headerName: string;
    formFieldName: string;
}

export interface PageNavState {
    isBrandUp: boolean;
    url: string;
    title: string;
    path: string;
    hash: string;
    params: { [key: string]: string };
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
    page: PageModel;
    [key: string]: any;
}

export interface PageModel {
    type: string;
    [key: string]: any;
}