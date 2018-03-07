"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// Tedious imports for maximum tree shaking
var toDate = require("date-fns/toDate");
var isValid = require("date-fns/isValid");
var format = require("date-fns/format");
var metadata_1 = require("./metadata");
/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
function convertToModel(object, metadata) {
    if (!object)
        return object;
    // Assume that an object that already has $metadata is already valid. 
    // This prevents this method from infinitely recursing when it encounters a circular graph.
    // It may be worth changing this to use an ES6 symbol to mark this instead.
    if ("$metadata" in object)
        return object;
    var hydrated = Object.assign(object, { $metadata: metadata });
    var _loop_1 = function (propName) {
        var propMeta = metadata.props[propName];
        var propVal = hydrated[propName];
        if (!(propName in hydrated) || propVal === undefined) {
            // All propertes that are not defined need to be declared
            // so that Vue's reactivity system can discover them.
            // Null is a valid type for all model properties (or at least generated models). Undefined is not.
            hydrated[propName] = null;
        }
        else if (propVal === null) {
            // Incoming value was explicit null. Nothing to be done. Nulls are valid for all model properties.
        }
        else {
            switch (propMeta.type) {
                case "date":
                    // If value is already a date, keep the exact same object.
                    date = propVal instanceof Date ? propVal : toDate(propVal);
                    if (!isValid(date)) {
                        throw "Recieved unparsable date: " + propVal;
                    }
                    hydrated[propName] = date;
                    break;
                case "model":
                case "object":
                    convertToModel(propVal, propMeta.typeDef);
                    break;
                case "collection":
                    var typeDef_1 = propMeta.typeDef;
                    if (Array.isArray(propVal)
                        && typeof (typeDef_1) == 'object'
                        && (typeDef_1.type == "model" || typeDef_1.type == "object")) {
                        propVal.forEach(function (item) { return convertToModel(item, typeDef_1); });
                    }
                    break;
            }
        }
    };
    var date;
    for (var propName in metadata.props) {
        _loop_1(propName);
    }
    return object;
}
exports.convertToModel = convertToModel;
function mapToDto(object) {
    if (object === null || object === undefined)
        return null;
    if (!object.$metadata) {
        throw "Object has no $metadata property.";
    }
    var dto = {};
    for (var propName in object.$metadata.props) {
        var propMeta = object.$metadata.props[propName];
        var value = object[propName];
        switch (propMeta.type) {
            case "date":
                if (isValid(value)) {
                    // TODO: exclude timezone (Z) for DateTime, keep it for DateTimeOffset
                    value = format(value, 'YYYY-MM-DDTHH:mm:ss.SSSZ');
                }
                else if (value != null) {
                    console.warn("Invalid date couldn't be mapped: " + value);
                    value = null;
                }
            case "string":
            case "number":
            case "boolean":
            case "enum":
                value = value || null;
                break;
            default:
                value = undefined;
        }
        if (value !== undefined) {
            dto[propName] = value;
        }
    }
    return dto;
}
exports.mapToDto = mapToDto;
/**
 * Given a non-collection value and its type's metadata,
 * return a string representation of the value suitable for display.
 */
// Intentionally not exported - this is a helper for the other display functions.
function getDisplayForType(type, value) {
    if (value == null)
        return value;
    switch (type) {
        case "date":
        case "number":
        case "boolean":
        case "string":
            return value.toLocaleString();
    }
    switch (type.type) {
        case "enum":
            var enumData = type.valueLookup[value];
            if (!enumData)
                return null;
            return enumData.displayName;
        case "model":
        case "object":
            return modelDisplay(value);
    }
}
/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
function modelDisplay(item) {
    var modelMeta = item.$metadata;
    if (!modelMeta) {
        throw "Item passed to modelDisplay(item) is missing its $metadata property";
    }
    if (modelMeta.displayProp)
        return propDisplay(item, modelMeta.displayProp);
    else {
        // https://stackoverflow.com/a/46908358 - stringify only first-level properties.
        try {
            return JSON.stringify(item, function (k, v) { return k ? "" + v : v; });
        }
        catch (_a) {
            return item.toLocaleString();
        }
    }
}
exports.modelDisplay = modelDisplay;
/**
 * Given a model instance and a descriptor of a property on the instance,
 * return a string representation of the property suitable for display.
 * @param item An instance of the model that holds the property to be displayed
 * @param prop The property to be displayed - either the name of a property or a property metadata object.
 */
function propDisplay(item, prop) {
    var propMeta = metadata_1.resolvePropMeta(item.$metadata, prop);
    var value = item[propMeta.name];
    switch (propMeta.type) {
        case "enum":
        case "model":
        case "object":
            return getDisplayForType(propMeta.typeDef, value);
        case "collection":
            if (!value) {
                value = [];
            }
            if (!Array.isArray(value)) {
                throw "Value for collection " + propMeta.name + " was not an array";
            }
            // Is this what we want? I think so - its the cleanest option.
            // Perhaps an prop that controls this would be best.
            if (value.length == 0)
                return "";
            // TODO: a prop that controls this number would also be good.
            if (value.length <= 5) {
                var collectedType_1 = propMeta.typeDef;
                return (value)
                    .map(function (childItem) {
                    var display = getDisplayForType(collectedType_1, childItem);
                    if (display === null)
                        display = '???'; // TODO: what should this be for un-displayable members of a collection?
                    return display;
                })
                    .join(", ");
            }
            return value.length.toLocaleString();
        default:
            return getDisplayForType(propMeta.type, value);
    }
}
exports.propDisplay = propDisplay;
// type HydratedModel<T extends Model> = {
//     [P in keyof T]?: DeepTransport<T[P]>;
// } & IHaveMetadata
// e.g. 
/*

type DeepTransport<T> =
    T extends any[] ? DeepTransportArray<T[number]> :
    T extends Date ? string :
    T extends object ? DeepTransportObject<T> :
    T;
interface DeepTransportArray<T> extends Array<DeepTransport<T>> {}
type DeepTransportObject<T> = { [P in keyof T]?: DeepTransport<T[P]>; };


export type Transport<T> = DeepTransport<Pick<T, Exclude<keyof T, keyof IHaveMetadata>>>



    type CaseTransport = Transport<Case>
    var a: CaseTransport;

*/ 
