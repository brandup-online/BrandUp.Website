interface PageCollectionModel {
    id: string;
    createdDate: string;
    pageId?: string;
    title: string;
    pageType: string;
    sort: "FirstOld" | "FirstNew";
    customSorting: boolean;
    pageUrl: string;
}

interface PageModel {
    id: string;
    createdDate: string;
    title: string;
    status: "Draft" | "Published";
    url: string;
}

interface PageTypeModel {
    name: string;
    title: string;
}

interface ContentTypeModel {
    name: string;
    title: string;
}

interface ContentModel {
    title: string;
    type: ContentTypeModel;
}

interface PageEditorModel {
    id: string;
    email: string;
}

interface Result {
    succeeded: boolean;
    errors: Array<string>;
}

interface ValidationProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
    instance: string;
    errors: { [key: string]: Array<string> };
}