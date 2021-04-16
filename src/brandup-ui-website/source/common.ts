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