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
        if (value == null)
            return value;
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
var ModelConversionVisitor = /** @class */ (function (_super) {
    __extends(ModelConversionVisitor, _super);
    function ModelConversionVisitor(mode) {
        var _this = _super.call(this) || this;
        _this.mode = mode;
        _this.objects = new Map();
        return _this;
    }
    ModelConversionVisitor.prototype.visitValue = function (value, meta) {
        if (value === undefined)
            return null;
        return _super.prototype.visitValue.call(this, value, meta);
    };
    ModelConversionVisitor.prototype.visitObject = function (value, meta) {
        if (!value)
            return null;
        if (typeof value !== "object")
            throw "Value for object " + meta.name + " was not an object";
        // Prevent infinite recursion on circular object graphs.
        if (this.objects.has(value))
            return this.objects.get(value);
        var props = meta.props;
        var target;
        if (this.mode == "convert") {
            // If there already is metadata but it doesn't match,
            // this is bad - someone passed mismatched parameters.
            if ("$metadata" in value && value.$metadata !== meta) {
                throw "While trying to convert object, found metadata for " + value.$metadata.name + " where metadata for " + meta.name + " was expected.";
            }
            ;
            target = value;
        }
        else if (this.mode == "map") {
            target = {};
        }
        else {
            throw "Unhandled mode " + this.mode;
        }
        this.objects.set(value, target);
        target.$metadata = meta;
        for (var propName in props) {
            var propVal = value[propName];
            if (!(propName in value) || propVal === undefined) {
                // All propertes that are not defined need to be declared
                // so that Vue's reactivity system can discover them.
                // Null is a valid type for all model properties (or at least generated models). Undefined is not.
                target[propName] = null;
            }
            else {
                target[propName] = this.visitValue(value[propName], props[propName]);
            }
        }
        return target;
    };
    ModelConversionVisitor.prototype.visitCollection = function (value, meta) {
        var _this = this;
        if (!value)
            return null;
        if (!Array.isArray(value))
            throw "Value for collection " + meta.name + " was not an array";
        if (this.mode == "convert") {
            for (var i = 0; i < value.length; i++) {
                value[i] = this.visitValue(value[i], meta.itemType);
            }
            return value;
        }
        else if (this.mode == "map") {
            return value.map(function (element, index) { return _this.visitValue(element, meta.itemType); });
        }
        else {
            throw "Unhandled mode " + this.mode;
        }
    };
    ModelConversionVisitor.prototype.visitDateValue = function (value, meta) {
        if (!value)
            return null;
        if (value instanceof Date) {
            if (this.mode == "convert") {
                // Preserve object ref when converting.
                return value;
            }
            else if (this.mode == "map") {
                // Get a new object ref when mapping
                return new Date(value);
            }
        }
        else {
            var date = toDate(value);
            if (!isValid(date)) {
                console.warn("Recieved unparsable date: " + value);
            }
            return date;
        }
    };
    return ModelConversionVisitor;
}(Visitor));
/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function convertToModel(value, metadata) {
    if (value == null)
        return value;
    return new ModelConversionVisitor("convert").visitObject(value, metadata);
}
/**
 * Transforms a raw value into a valid implemenation of a model value.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The value that should be converted
 * @param metadata The metadata describing the value
 */
export function convertValueToModel(value, metadata) {
    if (value == null)
        return value;
    return new ModelConversionVisitor("convert").visitValue(value, metadata);
}
/**
 * Maps the given object with data properties into a valid implemenation of TModel.
 * This function returns a new copy of its input and all descendent properties of its input - it does not preserve original objects.
 * @param object The object with data properties that should be mapped to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function mapToModel(object, metadata) {
    if (!object)
        return object;
    return new ModelConversionVisitor("map").visitObject(object, metadata);
}
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
export function mapValueToDto(value, metadata) {
    if (value === null || value === undefined)
        return value;
    return new MapToDtoVisitor(1).visitValue(value, metadata);
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
