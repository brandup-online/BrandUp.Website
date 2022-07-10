export var minWait = function (func, time) {
    if (time === void 0) { time = 700; }
    var n = Date.now();
    var ret = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i] = arguments[_i];
        }
        var n2 = Date.now();
        var w = time - (n2 - n);
        if (w > time * 0.1) {
            window.setTimeout(function () {
                func.apply(void 0, args);
            }, w);
        }
        else {
            func.apply(void 0, args);
        }
    };
    return ret;
};
