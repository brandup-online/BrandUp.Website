var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import { AjaxQueue, ajaxRequest } from "brandup-ui-ajax";
import { Middleware } from "brandup-ui-app";
import { DOM } from "brandup-ui-dom";
import { Page } from "../pages/base";
import { minWait } from "../utilities/wait";
var allowHistory = !!window.history && !!window.history.pushState;
var pageReloadHeader = "Page-Reload";
var pageActionHeader = "Page-Action";
var pageLocationHeader = "Page-Location";
var pageReplaceHeader = "Page-Replace";
var WebsiteMiddleware = /** @class */ (function (_super) {
    __extends(WebsiteMiddleware, _super);
    function WebsiteMiddleware(nav, options, antiforgery) {
        var _this = _super.call(this) || this;
        _this.__page = null;
        _this.__navCounter = 0;
        _this.__currentUrl = null;
        _this.__loadingPage = false;
        _this.options = options;
        _this.antiforgery = antiforgery;
        if (!_this.options.pageTypes)
            _this.options.pageTypes = {};
        if (!_this.options.scripts)
            _this.options.scripts = {};
        _this.queue = new AjaxQueue({
            preRequest: function (options) {
                if (!options.headers)
                    options.headers = {};
                if (_this.antiforgery && options.method !== "GET" && options.method)
                    options.headers[_this.antiforgery.headerName] = _this.__navigation.validationToken;
            }
        });
        _this.setNavigation(nav, location.hash ? location.hash.substr(1) : null, false);
        return _this;
    }
    Object.defineProperty(WebsiteMiddleware.prototype, "id", {
        get: function () { return this.app.model.websiteId; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(WebsiteMiddleware.prototype, "visitorId", {
        get: function () { return this.app.model.visitorId; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(WebsiteMiddleware.prototype, "validationToken", {
        get: function () { return this.__navigation ? this.__navigation.validationToken : null; },
        enumerable: false,
        configurable: true
    });
    WebsiteMiddleware.prototype.start = function (context, next) {
        var _this = this;
        context.items["website"] = this;
        context.items["nav"] = this.__navigation;
        var bodyElem = document.body;
        bodyElem.appendChild(this.__loaderElem = DOM.tag("div", { class: "bp-page-loader" }));
        this.__showNavigationProgress();
        this.__contentBodyElem = document.getElementById("page-content");
        if (!this.__contentBodyElem)
            throw "Not found page content element.";
        if (allowHistory) {
            window.addEventListener("popstate", function (e) { return _this.__onPopState(e); });
        }
        bodyElem.addEventListener("invalid", function (event) {
            event.preventDefault();
            var elem = event.target;
            elem.classList.add("invalid");
            if (elem.hasAttribute("data-val-required")) {
                elem.classList.add("invalid-required");
            }
        }, true);
        bodyElem.addEventListener("change", function (event) {
            var elem = event.target;
            elem.classList.remove("invalid");
            if (elem.hasAttribute("data-val-required")) {
                elem.classList.remove("invalid-required");
            }
        });
        this.__renderPage(context.items, this.__navCounter, null, next);
    };
    WebsiteMiddleware.prototype.loaded = function (context, next) {
        context.items["website"] = this;
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;
        next();
        this.__hideNavigationProgress();
    };
    WebsiteMiddleware.prototype.navigating = function (context, next) {
        context.items["website"] = this;
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;
        this.__showNavigationProgress();
        next();
    };
    WebsiteMiddleware.prototype.navigate = function (context, next) {
        var _this = this;
        if (!allowHistory) {
            location.href = context.fullUrl ? context.fullUrl : location.href;
            return;
        }
        context.items["website"] = this;
        context.items["prevNav"] = this.__navigation;
        this.__loadingPage = true;
        var navSequence = this.__incNavSequence();
        this.queue.reset(true);
        this.queue.push({
            url: context.url,
            method: "POST",
            urlParams: { _nav: "" },
            type: "TEXT",
            data: this.__navigation.state ? this.__navigation.state : "",
            success: function (response) {
                if (_this.__isNavOutdated(navSequence))
                    return;
                switch (response.status) {
                    case 200: {
                        if (_this.__precessPageResponse(response))
                            return;
                        context.items["nav"] = response.data;
                        _this.setNavigation(response.data, context.hash, context.replace);
                        if (response.xhr.getResponseHeader(pageReloadHeader) === "true") {
                            location.reload();
                            return;
                        }
                        _this.__loadContent(context.items, navSequence, next);
                        break;
                    }
                    default: {
                        if (context.replace)
                            location.replace(context.fullUrl);
                        else
                            location.assign(context.fullUrl);
                        break;
                    }
                }
            }
        });
    };
    WebsiteMiddleware.prototype.submit = function (context, next) {
        var _this = this;
        var form = context.form;
        var url = form.action;
        var method = form.method ? form.method.toUpperCase() : "POST";
        context.items["website"] = this;
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;
        var submitButton = DOM.queryElement(form, "[type=submit]");
        if (submitButton)
            submitButton.classList.add("loading");
        form.classList.add("loading");
        var navSequence = this.__incNavSequence();
        var submitCallback = function (response) {
            if (!_this.__isNavOutdated(navSequence)) {
                console.log("form submitted: ".concat(method, " ").concat(url, " ").concat(response.status));
                switch (response.status) {
                    case 200:
                    case 201: {
                        if (!_this.__precessPageResponse(response))
                            _this.updateHtml(response.data);
                        break;
                    }
                    default: {
                        break;
                    }
                }
                if (submitButton)
                    submitButton.classList.remove("loading");
                form.classList.remove("loading");
            }
            next();
            _this.__hideNavigationProgress();
        };
        this.__showNavigationProgress();
        console.log("form submitting: ".concat(method, " ").concat(url));
        var handler = form.getAttribute("data-form-handler");
        this.queue.reset(true);
        this.queue.push({
            url: url,
            urlParams: { _content: "", handler: handler },
            method: method,
            data: new FormData(form),
            success: submitButton ? minWait(submitCallback) : submitCallback
        });
    };
    WebsiteMiddleware.prototype.stop = function (context, next) {
        context.items["website"] = this;
        context.items["nav"] = this.__navigation;
        context.items["page"] = this.__page;
        next();
    };
    WebsiteMiddleware.prototype.request = function (options, includeAntiforgery) {
        if (includeAntiforgery === void 0) { includeAntiforgery = true; }
        if (!options.headers)
            options.headers = {};
        if (includeAntiforgery && this.antiforgery && options.method !== "GET")
            options.headers[this.antiforgery.headerName] = this.__navigation.validationToken;
        ajaxRequest(options);
    };
    WebsiteMiddleware.prototype.updateHtml = function (html) {
        var navSequence = this.__incNavSequence();
        this.__renderPage({}, navSequence, html, function () { return; });
    };
    WebsiteMiddleware.prototype.buildUrl = function (path, queryParams) {
        return this.app.uri(path, queryParams);
    };
    WebsiteMiddleware.prototype.nav = function (options) {
        this.app.nav(options);
    };
    WebsiteMiddleware.prototype.getScript = function (name) {
        var scriptFunc = this.options.scripts[name];
        if (!scriptFunc)
            return;
        return scriptFunc();
    };
    WebsiteMiddleware.prototype.setNavigation = function (data, hash, replace) {
        var navUrl = data.url;
        if (hash)
            navUrl += "#" + hash;
        var prevNav = this.__navigation;
        if (prevNav) {
            if (prevNav.isAuthenticated !== data.isAuthenticated) {
                location.href = navUrl;
                return;
            }
            document.title = data.title ? data.title : "";
            var metaDescription = document.getElementById("page-meta-description");
            if (data.description) {
                if (!metaDescription) {
                    document.head.appendChild(metaDescription = DOM.tag("meta", { id: "page-meta-description", name: "description", content: "" }));
                }
                metaDescription.setAttribute("content", data.description);
            }
            else if (metaDescription)
                metaDescription.remove();
            var metaKeywords = document.getElementById("page-meta-keywords");
            if (data.keywords) {
                if (!metaKeywords) {
                    document.head.appendChild(metaKeywords = DOM.tag("meta", { id: "page-meta-keywords", name: "keywords", content: "" }));
                }
                metaKeywords.setAttribute("content", data.keywords);
            }
            else if (metaKeywords)
                metaKeywords.remove();
            var linkCanonical = document.getElementById("page-link-canonical");
            if (data.canonicalLink) {
                if (!linkCanonical) {
                    document.head.appendChild(linkCanonical = DOM.tag("link", { id: "page-link-canonical", rel: "canonical", href: "" }));
                }
                linkCanonical.setAttribute("href", data.canonicalLink);
            }
            else if (linkCanonical)
                linkCanonical.remove();
            this.__setOpenGraphProperty("type", data.openGraph ? data.openGraph.type : null);
            this.__setOpenGraphProperty("title", data.openGraph ? data.openGraph.title : null);
            this.__setOpenGraphProperty("image", data.openGraph ? data.openGraph.image : null);
            this.__setOpenGraphProperty("url", data.openGraph ? data.openGraph.url : null);
            this.__setOpenGraphProperty("site_name", data.openGraph ? data.openGraph.siteName : null);
            this.__setOpenGraphProperty("description", data.openGraph ? data.openGraph.description : null);
        }
        this.__navigation = data;
        this.__currentUrl = { url: data.url, hash: hash };
        if (prevNav && prevNav.bodyClass) {
            document.body.classList.remove(prevNav.bodyClass);
        }
        if (this.__navigation.bodyClass) {
            document.body.classList.add(this.__navigation.bodyClass);
        }
        if (navUrl === location.href)
            replace = true;
        if (!hash) {
            if (allowHistory) {
                if (!replace)
                    window.history.pushState(window.history.state, data.title, navUrl);
                else
                    window.history.replaceState(window.history.state, data.title, navUrl);
            }
        }
        if (!replace)
            window.scrollTo({ left: 0, top: 0, behavior: "auto" });
    };
    WebsiteMiddleware.prototype.__setOpenGraphProperty = function (name, value) {
        var metaTagElem = document.getElementById("og-".concat(name));
        if (value) {
            if (!metaTagElem) {
                document.head.appendChild(metaTagElem = DOM.tag("meta", { id: "og-".concat(name), property: name, content: value }));
            }
            metaTagElem.setAttribute("content", value);
        }
        else if (metaTagElem)
            metaTagElem.remove();
    };
    WebsiteMiddleware.prototype.__loadContent = function (items, navSequence, next) {
        var _this = this;
        if (this.__isNavOutdated(navSequence))
            return;
        this.queue.push({
            url: this.__navigation.url,
            urlParams: { _content: "" },
            disableCache: true,
            success: function (response) {
                if (_this.__isNavOutdated(navSequence))
                    return;
                switch (response.status) {
                    case 200: {
                        if (_this.__precessPageResponse(response))
                            return;
                        _this.__renderPage(items, navSequence, response.data ? response.data : "", next);
                        break;
                    }
                    case 404:
                    case 500:
                    case 401: {
                        location.reload();
                        break;
                    }
                    default:
                        throw new Error();
                }
            }
        });
    };
    WebsiteMiddleware.prototype.__renderPage = function (items, navSequence, contentHtml, next) {
        var _this = this;
        if (this.__isNavOutdated(navSequence))
            return;
        var pageTypeName = this.__navigation.page.type;
        if (!pageTypeName)
            pageTypeName = this.options.defaultType;
        if (pageTypeName) {
            var pageTypeFactory = this.options.pageTypes[pageTypeName];
            if (!pageTypeFactory)
                throw "Not found page type \"".concat(pageTypeName, "\".");
            pageTypeFactory()
                .then(function (pageType) { _this.__createPage(items, navSequence, pageType.default, contentHtml, next); })
                .catch(function () { throw "Error loading page type \"".concat(pageTypeName, "\"."); });
        }
        else {
            this.__createPage(items, navSequence, Page, contentHtml, next);
        }
    };
    WebsiteMiddleware.prototype.__createPage = function (items, navSequence, pageType, contentHtml, next) {
        if (this.__isNavOutdated(navSequence))
            return;
        if (this.__page) {
            this.__page.destroy();
            this.__page = null;
        }
        if (contentHtml !== null) {
            DOM.empty(this.__contentBodyElem);
            this.__contentBodyElem.insertAdjacentHTML("afterbegin", contentHtml);
            WebsiteMiddleware.nodeScriptReplace(this.__contentBodyElem);
        }
        this.__page = new pageType(this, this.__navigation, this.__contentBodyElem);
        this.__page.render(this.__currentUrl.hash);
        items["page"] = this.__page;
        this.__loadingPage = false;
        next();
        this.__hideNavigationProgress();
    };
    WebsiteMiddleware.prototype.__precessPageResponse = function (response) {
        var pageAction = response.xhr.getResponseHeader(pageActionHeader);
        if (pageAction) {
            switch (pageAction) {
                case "reset":
                case "reload": {
                    location.reload();
                    return true;
                }
                default:
                    throw "Неизвестный тип действия для страницы.";
            }
        }
        var redirectLocation = response.xhr.getResponseHeader(pageLocationHeader);
        if (redirectLocation) {
            this.nav({
                url: redirectLocation,
                replace: response.xhr.getResponseHeader(pageReplaceHeader) === "true"
            });
            return true;
        }
        return false;
    };
    WebsiteMiddleware.prototype.__onPopState = function (event) {
        event.preventDefault();
        var url = location.href;
        var oldUrl = this.__currentUrl;
        var newUrl = this.__extractHashFromUrl(url);
        if (oldUrl.hash && !newUrl.hash) {
            console.log("remove hash: ".concat(oldUrl.hash));
        }
        else if (!oldUrl.hash && newUrl.hash) {
            console.log("add hash: ".concat(newUrl.hash));
        }
        else if (oldUrl.hash && newUrl.hash) {
            console.log("change hash: ".concat(newUrl.hash));
        }
        this.__currentUrl = newUrl;
        if ((oldUrl.hash || newUrl.hash) && oldUrl.url.toLowerCase() === newUrl.url.toLowerCase()) {
            this.__page.changedHash(newUrl.hash, oldUrl.hash);
            return;
        }
        console.log("PopState: " + url);
        this.app.nav({
            url: url,
            replace: true,
            context: {
                state: event.state
            }
        });
    };
    WebsiteMiddleware.prototype.__extractHashFromUrl = function (url) {
        var hashIndex = url.lastIndexOf("#");
        if (hashIndex > 0)
            return {
                url: url.substr(0, hashIndex),
                hash: url.substr(hashIndex + 1)
            };
        return { url: url, hash: null };
    };
    WebsiteMiddleware.prototype.__incNavSequence = function () {
        this.__navCounter++;
        return this.__navCounter;
    };
    WebsiteMiddleware.prototype.__isNavOutdated = function (navSequence) {
        return navSequence !== this.__navCounter;
    };
    WebsiteMiddleware.prototype.__showNavigationProgress = function () {
        var _this = this;
        window.clearTimeout(this.__progressTimeout);
        window.clearTimeout(this.__progressInterval);
        this.__loaderElem.classList.remove("show", "show2", "finish");
        this.__loaderElem.style.width = "0%";
        window.setTimeout(function () {
            _this.__loaderElem.classList.add("show");
            _this.__loaderElem.style.width = "70%";
        }, 10);
        this.__progressTimeout = window.setTimeout(function () {
            _this.__loaderElem.classList.add("show");
            _this.__loaderElem.style.width = "100%";
        }, 1700);
        this.__progressStart = Date.now();
    };
    WebsiteMiddleware.prototype.__hideNavigationProgress = function () {
        var _this = this;
        var d = 500 - (Date.now() - this.__progressStart);
        if (d < 0)
            d = 0;
        window.clearTimeout(this.__progressTimeout);
        this.__progressTimeout = window.setTimeout(function () {
            window.clearTimeout(_this.__progressInterval);
            _this.__loaderElem.classList.add("finish");
            _this.__loaderElem.style.width = "100%";
            _this.__progressInterval = window.setTimeout(function () {
                _this.__loaderElem.classList.remove("show", "finish");
                _this.__loaderElem.style.width = "0%";
            }, 180);
        }, d);
    };
    WebsiteMiddleware.nodeScriptReplace = function (node) {
        if (node.tagName === "SCRIPT") {
            var script = document.createElement("script");
            script.text = node.innerHTML;
            for (var i = node.attributes.length - 1; i >= 0; i--)
                script.setAttribute(node.attributes[i].name, node.attributes[i].value);
            node.parentNode.replaceChild(script, node);
        }
        else {
            var i = 0;
            var children = node.childNodes;
            while (i < children.length)
                WebsiteMiddleware.nodeScriptReplace(children[i++]);
        }
        return node;
    };
    return WebsiteMiddleware;
}(Middleware));
export { WebsiteMiddleware };
