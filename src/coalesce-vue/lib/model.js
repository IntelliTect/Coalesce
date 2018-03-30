var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
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
                    var itemType_1 = propMeta.itemType;
                    if (Array.isArray(propVal) && (itemType_1.type == "model" || itemType_1.type == "object")) {
                        propVal.forEach(function (item) { return convertToModel(item, itemType_1.typeDef); });
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
// export function mapValueToDtoValue(value: any, valueMeta: Value, maxObjectDepth: number = 0, depth: number = 0) {
//     switch (valueMeta.type) {
//         case "model":
//         case "object":
//             if (depth < maxObjectDepth){
//                 value = mapObjectToDto(value, valueMeta.typeDef.props)
//             }
//             break;
//         case "collection":
//             if (depth < maxObjectDepth){
//                 if (!value) {
//                     value = [];
//                 }
//                 if (!Array.isArray(value)){
//                     throw `Value for collection ${valueMeta.name} was not an array`
//                 }
//                 const collectedType = valueMeta.collectedType
//                 return value
//                     .map(childItem => {
//                         // if (propMeta.collectedTypeDef)
//                         var child = 'collectedTypeDef' in valueMeta
//                             ? mapValueToDtoValue(childItem, valueMeta.collectedTypeDef, maxObjectDepth, depth)
//                             : mapValueToDtoValue(childItem, valueMeta.collectedType, maxObjectDepth, depth)
//                         return child;
//                     })
//             }
//             break;
//         case "date":
//             if (isValid(value)) {
//                 // TODO: exclude timezone (Z) for DateTime, keep it for DateTimeOffset
//                 value = format(value, 'YYYY-MM-DDTHH:mm:ss.SSSZ')
//             } else if (value != null) {
//                 console.warn(`Invalid date couldn't be mapped: ${value}`)
//                 value = null
//             }
//         case "string":
//         case "number":
//         case "boolean":
//         case "enum":
//             value = value || null;    
//             break;
//         default:
//             value = undefined;
//     }
// }
// export function mapObjectToDto(object: any, valuesMetadata: { [valueName: string]: Value }, maxObjectDepth: number = 0, depth: number = 0) {
//     var dto: { [k: string]: any } = {};
//     for (const valueName in valuesMetadata) {
//         const valueMeta = valuesMetadata[valueName];
//         var value = object[valueName];
//         switch (valueMeta.type) {
//             case "model":
//             case "object":
//                 if (depth < maxObjectDepth){
//                     value = mapObjectToDto(object, valueMeta.typeDef.props)
//                 }
//                 break;
//             case "collection":
//                 if (depth < maxObjectDepth){
//                     if (!value) {
//                         value = [];
//                     }
//                     if (!Array.isArray(value)){
//                         throw `Value for collection ${valueMeta.name} was not an array`
//                     }
//                     const collectedType = valueMeta.collectedType
//                     return value
//                         .map(childItem => {
//                             // if (propMeta.collectedTypeDef)
//                             var child = 'collectedTypeDef' in valueMeta
//                                 ? getDisplayForType(valueMeta.collectedTypeDef, childItem)
//                                 : getDisplayForValue(valueMeta.collectedType, childItem)
//                             return child;
//                         })
//                 }
//                 break;
//             case "date":
//                 if (isValid(value)) {
//                     // TODO: exclude timezone (Z) for DateTime, keep it for DateTimeOffset
//                     value = format(value, 'YYYY-MM-DDTHH:mm:ss.SSSZ')
//                 } else if (value != null) {
//                     console.warn(`Invalid date couldn't be mapped: ${value}`)
//                     value = null
//                 }
//             case "string":
//             case "number":
//             case "boolean":
//             case "enum":
//                 value = value || null;    
//                 break;
//             default:
//                 value = undefined;
//         }
//         if (value !== undefined) {
//             dto[valueName] = value;
//         }
//     }
//     return dto;
// }
var Visitor = /** @class */ (function () {
    function Visitor() {
    }
    Visitor.prototype.visitValue = function (value, meta) {
        switch (meta.type) {
            case "model": return this.visitModelValue(value, meta);
            case "object": return this.visitExternalTypeValue(value, meta);
            case "collection": return this.visitCollection(value, meta);
            case "enum": return this.visitEnumValue(value, meta);
            case "date": return this.visitDateValue(value, meta);
            default: return this.visitPrimitiveValue(value, meta);
        }
    };
    Visitor.prototype.visitObject = function (value, meta) {
        var props = meta.props;
        var output = {};
        for (var propName in props) {
            if (propName in value) {
                output[propName] = this.visitValue(value[propName], props[propName]);
            }
        }
        return output;
    };
    Visitor.prototype.visitExternalTypeValue = function (value, meta) {
        return this.visitObject(value, meta.typeDef);
    };
    Visitor.prototype.visitModelValue = function (value, meta) {
        return this.visitObject(value, meta.typeDef);
    };
    Visitor.prototype.visitCollection = function (value, meta) {
        var _this = this;
        if (value == null)
            return value;
        if (!Array.isArray(value))
            throw "Value for collection " + meta.name + " was not an array";
        return value.map(function (element, index) { return _this.visitValue(element, meta.itemType); });
    };
    Visitor.prototype.visitEnumValue = function (value, meta) {
        return value;
    };
    Visitor.prototype.visitDateValue = function (value, meta) {
        return value;
    };
    Visitor.prototype.visitPrimitiveValue = function (value, meta) {
        return value;
    };
    return Visitor;
}());
var GetDisplayVisitor = /** @class */ (function (_super) {
    __extends(GetDisplayVisitor, _super);
    function GetDisplayVisitor() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    GetDisplayVisitor.prototype.visitObject = function (value, meta) {
        if (value == null)
            return value;
        if (meta.displayProp) {
            return this.visitValue(value[meta.displayProp.name], meta.displayProp);
        }
        else {
            // https://stackoverflow.com/a/46908358 - stringify only first-level properties.
            try {
                return JSON.stringify(value, function (k, v) { return k ? "" + v : v; });
            }
            catch (_a) {
                return value.toLocaleString();
            }
        }
    };
    GetDisplayVisitor.prototype.visitCollection = function (value, meta) {
        var _this = this;
        if (!value)
            return null;
        if (!Array.isArray(value))
            throw "Value for collection " + meta.name + " was not an array";
        // Is this what we want? I think so - its the cleanest option.
        // Perhaps an prop that controls this would be best.
        if (value.length == 0)
            return "";
        // TODO: a prop that controls this number would also be good.
        if (value.length <= 5) {
            return (value)
                .map(function (childItem) {
                return _this.visitValue(childItem, meta.itemType)
                    || '???';
            } // TODO: what should this be for un-displayable members of a collection?
            )
                .join(", ");
        }
        return value.length.toLocaleString();
    };
    GetDisplayVisitor.prototype.visitEnumValue = function (value, meta) {
        if (value == null)
            return value;
        var enumData = meta.typeDef.valueLookup[value];
        if (!enumData)
            return '';
        return enumData.displayName;
    };
    GetDisplayVisitor.prototype.visitDateValue = function (value, meta) {
        if (value == null)
            return value;
        return value.toLocaleString();
    };
    GetDisplayVisitor.prototype.visitPrimitiveValue = function (value, meta) {
        if (value == null)
            return value;
        return value.toLocaleString();
    };
    return GetDisplayVisitor;
}(Visitor));
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
    return new GetDisplayVisitor().visitObject(item, item.$metadata);
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
    return new GetDisplayVisitor().visitValue(value, propMeta);
}
