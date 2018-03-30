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
var Visitor = /** @class */ (function () {
    function Visitor() {
    }
    Visitor.prototype.visitValue = function (value, meta) {
        switch (meta.type) {
            case "model": return this.visitModelValue(value, meta);
            case "object": return this.visitObjectValue(value, meta);
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
    Visitor.prototype.visitObjectValue = function (value, meta) {
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
    Visitor.prototype.visitPrimitiveValue = function (value, meta) {
        return value;
    };
    Visitor.prototype.visitDateValue = function (value, meta) {
        return value;
    };
    Visitor.prototype.visitEnumValue = function (value, meta) {
        return value;
    };
    return Visitor;
}());
var MapToDtoVisitor = /** @class */ (function (_super) {
    __extends(MapToDtoVisitor, _super);
    function MapToDtoVisitor(maxObjectDepth) {
        if (maxObjectDepth === void 0) { maxObjectDepth = 1; }
        var _this = _super.call(this) || this;
        _this.maxObjectDepth = maxObjectDepth;
        _this.depth = 0;
        return _this;
    }
    MapToDtoVisitor.prototype.visitObject = function (value, meta) {
        // If we've exceded max depth, return undefined to prevent the 
        // creation of an entry in the parent object for this object.
        if (this.depth >= this.maxObjectDepth)
            return undefined;
        this.depth++;
        var props = meta.props;
        var output = {};
        for (var propName in props) {
            var propMeta = props[propName];
            if (propName in value) {
                var newValue = this.visitValue(value[propName], propMeta);
                if (newValue !== undefined) {
                    // Only store values that aren't undefined.
                    // We don't any properties with undefined as their value - we shouldn't define these in the first place.
                    output[propName] = newValue;
                }
            }
            // This prop is a foreign key, and it has no value.
            // Lets check and see if the corresponding referenceNavigation prop has an object in it.
            // If it does, try and use that object's primary key as the value of our FK.
            if (output[propName] == null && propMeta.role == "foreignKey" && propMeta.navigationProp) {
                var objectValue = value[propMeta.navigationProp.name];
                if (objectValue) {
                    var objectValuePkValue = objectValue[propMeta.navigationProp.typeDef.keyProp.name];
                    if (objectValuePkValue != null) {
                        output[propName] = objectValuePkValue;
                    }
                }
                propMeta.principalType.keyProp.name;
            }
        }
        this.depth--;
        return output;
    };
    MapToDtoVisitor.prototype.visitCollection = function (value, meta) {
        // If we've exceded max depth, return undefined to prevent the 
        // creation of an entry in the parent object for this collection.
        if (this.depth >= this.maxObjectDepth)
            return undefined;
        this.depth++;
        var ret = _super.prototype.visitCollection.call(this, value, meta);
        this.depth--;
        return ret;
    };
    MapToDtoVisitor.prototype.visitDateValue = function (value, meta) {
        if (isValid(value)) {
            // TODO: exclude timezone (Z) for DateTime, keep it for DateTimeOffset
            value = format(value, 'YYYY-MM-DDTHH:mm:ss.SSSZ');
        }
        else if (value != null) {
            console.warn("Invalid date couldn't be mapped: " + value);
            value = null;
        }
        return value;
    };
    return MapToDtoVisitor;
}(Visitor));
export function mapToDto(object) {
    if (object === null || object === undefined)
        return null;
    if (!object.$metadata) {
        throw "Object has no $metadata property.";
    }
    var dto = new MapToDtoVisitor(1).visitObject(object, object.$metadata);
    return dto;
}
export function mapValueToDto(value, meta) {
    if (value === null || value === undefined)
        return value;
    return new MapToDtoVisitor(1).visitValue(value, meta);
}
/** Visitor that maps its input to a string representation of its value, suitable for display. */
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
/** Singleton instance of `GetDisplayVisitor`, since the visitor is stateless. */
var displayVisitor = new GetDisplayVisitor();
/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
export function modelDisplay(item) {
    var modelMeta = item.$metadata;
    if (!modelMeta) {
        throw "Item passed to modelDisplay(item) is missing its $metadata property";
    }
    return displayVisitor.visitObject(item, item.$metadata);
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
    return displayVisitor.visitValue(value, propMeta);
}
