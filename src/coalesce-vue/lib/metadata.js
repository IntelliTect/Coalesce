/* -----------------------------
   ---------- GENERAL ----------
   -----------------------------
*/
var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
/** Utility function for creating the required properties of `EnumType<>` from an array of `EnumValue`  */
export function getEnumMeta(values) {
    return {
        valueLookup: values.reduce(function (obj, v) {
            return __assign({}, obj, (_a = {}, _a[v.strValue] = v, _a[v.value] = v, _a));
            var _a;
        }, {}),
        values: values
    };
}
export function resolvePropMeta(metadata, propOrString, slient) {
    if (slient === void 0) { slient = false; }
    var propMeta = typeof propOrString == "string" ? metadata.props[propOrString] : propOrString;
    if (!propMeta) {
        if (slient)
            return undefined;
        throw "Unknown property " + propOrString;
    }
    else if (metadata.props[propMeta.name] !== propMeta) {
        if (slient)
            return undefined;
        throw "Property " + propMeta.name + " does not belong to object of type " + metadata.name;
    }
    return propMeta;
}
