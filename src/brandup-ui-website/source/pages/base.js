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
import { UIElement } from "brandup-ui";
import { AjaxQueue } from "brandup-ui-ajax";
import { DOM } from "brandup-ui-dom";
var Page = /** @class */ (function (_super) {
    __extends(Page, _super);
    function Page(website, nav, element) {
        var _this = _super.call(this) || this;
        _this.__destroyCallbacks = [];
        _this.__scripts = [];
        _this.__isRendered = false;
        _this.__hash = null;
        _this.setElement(element);
        _this.website = website;
        _this.nav = nav;
        _this.queue = new AjaxQueue({
            preRequest: function (options) {
                if (!options.headers)
                    options.headers = {};
                if (website.antiforgery && options.method !== "GET" && options.method)
                    options.headers[website.antiforgery.headerName] = nav.validationToken;
            }
        });
        return _this;
    }
    Object.defineProperty(Page.prototype, "typeName", {
        get: function () { return "BrandUp.Page"; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(Page.prototype, "model", {
        get: function () { return this.nav.page; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(Page.prototype, "hash", {
        get: function () { return this.__hash; },
        enumerable: false,
        configurable: true
    });
    Page.prototype.onRenderContent = function () { return; };
    Page.prototype.onChangedHash = function (newHash, oldHash) {
        return;
    };
    Page.prototype.render = function (hash) {
        if (this.__isRendered)
            return;
        this.__isRendered = true;
        this.__hash = hash;
        this.refreshScripts();
        this.onRenderContent();
    };
    Page.prototype.changedHash = function (newHash, oldHash) {
        this.__hash = newHash;
        this.onChangedHash(newHash, oldHash);
    };
    Page.prototype.submit = function (form) {
        if (!form)
            form = DOM.queryElement(this.element, "form");
        if (!form)
            throw "Not found form by submit.";
        this.website.app.submit({ form: form });
    };
    Page.prototype.buildUrl = function (queryParams) {
        var params = {};
        for (var k in this.nav.query) {
            params[k] = this.nav.query[k];
        }
        if (queryParams) {
            for (var k in queryParams) {
                params[k] = queryParams[k];
            }
        }
        return this.website.app.uri(this.nav.path, params);
    };
    Page.prototype.refreshScripts = function () {
        var _this = this;
        var scriptElements = DOM.queryElements(this.element, "[data-content-script]");
        var _loop_1 = function (i) {
            var elem = scriptElements.item(i);
            if (elem.hasAttribute("brandup-ui-element"))
                return "continue";
            var scriptName = elem.getAttribute("data-content-script");
            var script = this_1.website.getScript(scriptName);
            if (script) {
                script.then(function (t) {
                    if (!_this.__scripts)
                        return;
                    var uiElem = new t.default(elem);
                    _this.__scripts.push(uiElem);
                });
            }
        };
        var this_1 = this;
        for (var i = 0; i < scriptElements.length; i++) {
            _loop_1(i);
        }
    };
    Page.prototype.attachDestroyFunc = function (f) {
        this.__destroyCallbacks.push(f);
    };
    Page.prototype.attachDestroyElement = function (elem) {
        this.__destroyCallbacks.push(function () { elem.destroy(); });
    };
    Page.prototype.destroy = function () {
        if (this.__scripts) {
            this.__scripts.map(function (elem) { elem.destroy(); });
            this.__scripts = null;
        }
        if (this.__destroyCallbacks) {
            this.__destroyCallbacks.map(function (f) { f(); });
            this.__destroyCallbacks = null;
        }
        this.queue.destroy();
        _super.prototype.destroy.call(this);
    };
    return Page;
}(UIElement));
export { Page };
