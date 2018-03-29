// Tedious imports for maximum tree shaking
import * as toDate from 'date-fns/toDate';
import * as isValid from 'date-fns/isValid';
import * as format from 'date-fns/format';
import { resolvePropMeta } from "./metadata";
/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function convertToModel(object, metadata) {
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
                    if (Array.isArray(propVal) && (propMeta.collectedType == "model" || propMeta.collectedType == "object")) {
                        propVal.forEach(function (item) { return convertToModel(item, propMeta.collectedTypeDef); });
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
export function mapToDto(object) {
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
/**
 * Given a value and its custom type's metadata,
 * return a string representation of the value suitable for display.
 */
// NOTE: Intentionally not exported - this is a helper for the other display functions.
function getDisplayForType(typeDef, value) {
    if (value == null)
        return value;
    switch (typeDef.type) {
        case "enum":
            var enumData = typeDef.valueLookup[value];
            if (!enumData)
                return null;
            return enumData.displayName;
        case "model":
        case "object":
            return modelDisplay(value);
    }
}
/**
 * Given a value and its simple type kind,
 * return a string representation of the value suitable for display.
 */
function getDisplayForValue(type, value) {
    if (value == null)
        return value;
    switch (type) {
        case "date": // TODO: handle date better than this?
        case "number":
        case "boolean":
        case "string":
            return value.toLocaleString();
    }
}
/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
export function modelDisplay(item) {
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
/**
 * Given a model instance and a descriptor of a property on the instance,
 * return a string representation of the property suitable for display.
 * @param item An instance of the model that holds the property to be displayed
 * @param prop The property to be displayed - either the name of a property or a property metadata object.
 */
export function propDisplay(item, prop) {
    var propMeta = resolvePropMeta(item.$metadata, prop);
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
                var collectedType = propMeta.collectedType;
                return (value)
                    .map(function (childItem) {
                    // if (propMeta.collectedTypeDef)
                    var display = 'collectedTypeDef' in propMeta
                        ? getDisplayForType(propMeta.collectedTypeDef, childItem)
                        : getDisplayForValue(propMeta.collectedType, childItem);
                    if (display === null)
                        display = '???'; // TODO: what should this be for un-displayable members of a collection?
                    return display;
                })
                    .join(", ");
            }
            return value.length.toLocaleString();
        default:
            return getDisplayForValue(propMeta.type, value);
    }
}
