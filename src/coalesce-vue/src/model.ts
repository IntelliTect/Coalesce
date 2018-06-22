
// Tedious imports for maximum tree shaking
import * as toDate from 'date-fns/toDate'
import * as isValid from 'date-fns/isValid'
import * as format from 'date-fns/format'

import { ClassType, ModelType, Property, ObjectType, PropNames, resolvePropMeta, CustomType, TypeDiscriminator, NonCollectionTypeDiscriminator, SimpleTypeDiscriminator, Value, EnumType, ModelValue, ObjectValue, EnumValue, PrimitiveValue, DateValue, CustomTypeValue, ValueMeta, CollectionValue, PrimitiveProperty, ForeignKeyProperty, DataSourceType } from "./metadata"
import { Indexable } from './util'

/**
 * Represents a model with metadata information.
 */
export interface Model<TMeta extends ClassType> {
    readonly $metadata: TMeta;
}

/**
 * Represents a data source with metadata information and parameter values.
 */
export interface DataSource<TMeta extends DataSourceType> {
    readonly $metadata: TMeta;
}



class Visitor<TValue = any, TArray = any[], TObject = any> {

    public visitValue(value: any, meta: Value): TValue | TArray | TObject {
        switch (meta.type) {
            case undefined: throw "Missing type on value metadata";
            case "model": return this.visitModelValue(value, meta);
            case "object": return this.visitObjectValue(value, meta);
            case "collection": return this.visitCollection(value, meta);
            case "enum": return this.visitEnumValue(value, meta);
            case "date": return this.visitDateValue(value, meta);
            default: return this.visitPrimitiveValue(value, meta);
        }
    }
    
    public visitObject(value: any, meta: ClassType): TObject {
        if (value == null) return value;
        const props = meta.props;
        const output: any = {}
        for (const propName in props) {
            if (propName in value) {
                output[propName] = this.visitValue(value[propName], props[propName]);
            }
        }
        return output;
    }

    public visitObjectValue(value: any, meta: ObjectValue): TObject {
        return this.visitObject(value, meta.typeDef);
    }

    public visitModelValue(value: any, meta: ModelValue): TObject {
        return this.visitObject(value, meta.typeDef);
    }

    public visitCollection(value: any[], meta: CollectionValue): TArray {
        if (value == null) return value;
        if (!Array.isArray(value)) throw `Value for collection ${meta.name} was not an array`;

        return value.map((element, index) => this.visitValue(element, meta.itemType)) as any;
    }

    public visitPrimitiveValue(value: any, meta: PrimitiveValue): TValue {
        return value;
    }

    public visitDateValue(value: any, meta: DateValue): TValue {
        return value;
    }

    public visitEnumValue(value: any, meta: EnumValue): TValue {
        return value;
    }
}

class ModelConversionVisitor extends Visitor<any, any[] | null, any | null> {

    private objects = new Map<object, object>();

    public visitValue(value: any, meta: Value): any {
        if (value === undefined) return null;
        return super.visitValue(value, meta);
    }

    public visitObject(value: any, meta: ClassType) {
        if (value == null) return null;
        if (typeof value !== "object" || Array.isArray(value)) 
            throw `Value for object ${meta.name} was not an object`;

        // Prevent infinite recursion on circular object graphs.
        if (this.objects.has(value)) return this.objects.get(value);

        const props = meta.props;

        let target: any;
        if (this.mode == "convert") {
            
            // If there already is metadata but it doesn't match,
            // this is bad - someone passed mismatched parameters.
            if ("$metadata" in value && value.$metadata !== meta) {
                throw `While trying to convert object, found metadata for ${value.$metadata.name} where metadata for ${meta.name} was expected.`   
            };

            target = value;
        } else if (this.mode == "map") {
            target = {};
        } else {
            throw `Unhandled mode ${this.mode}`
        }

        this.objects.set(value, target);
        target.$metadata = meta;

        for (const propName in props) {
            const propVal = value[propName];
            if (!(propName in value) || propVal === undefined) {
                // All propertes that are not defined need to be declared
                // so that Vue's reactivity system can discover them.
                // Null is a valid type for all model properties (or at least generated models). Undefined is not.
                target[propName] = null
            } else {
                target[propName] = this.visitValue(value[propName], props[propName]);
            }

        }

        return target;
    }

    public visitCollection(value: any[], meta: CollectionValue) {
        if (value == null) return null;
        if (!Array.isArray(value)) throw `Value for collection ${meta.name} was not an array`;

        if (this.mode == "convert") {
            for (let i = 0; i < value.length; i++) {
                value[i] = this.visitValue(value[i], meta.itemType);
            }
            return value;
        } else if (this.mode == "map") {
            return value.map((element, index) => this.visitValue(element, meta.itemType));
        } else {
            throw `Unhandled mode ${this.mode}`
        }
    }

    public visitDateValue(value: any, meta: DateValue) {
        if (value == null) return null;
        
        if (value instanceof Date) {
            if (this.mode == "convert") {
                // Preserve object ref when converting.
                return value;
            } else if (this.mode == "map") {
                // Get a new object ref when mapping
                return new Date(value);
            }
        } else {
            if (typeof value !== "string") {
                // dateFns `toDate` is way too lenient - 
                // it will parse any number as milliseconds since the epoch,
                // and parses `true` as the epoch.
                throw `Recieved unparsable date: ${value}`;
            }
            var date = toDate(value);
            if (!isValid(date)) {
                throw `Recieved unparsable date: ${value}`;
            }
            return date;
        }
    }

    private visitNumeric(value: any, meta: EnumValue | (PrimitiveValue & { type: "number" })) {
        if (value == null) return null;

        if (typeof value === "number") return value;

        if (typeof value !== "string") {
            // We don't want to parse things like booleans into numbers.
            // Strings are all we should be handling.
            throw `Recieved unparsable ${meta.type}: ${value}`;
        }

        const parsed = Number(value);
        if (isNaN(parsed)) {
            throw `Recieved unparsable ${meta.type}: ${value}`;
        }
        return parsed;
    }

    public visitEnumValue(value: any, meta: EnumValue) {
        return this.visitNumeric(value, meta);
    }

    public visitPrimitiveValue(value: any, meta: PrimitiveValue) {
        if (value == null) return null;

        switch (meta.type) {
            case "number":
                return this.visitNumeric(value, meta as (typeof meta & { type: "number" }));
            case "boolean":
                if (typeof value === "boolean") return value;
                if (value === "false") return false;
                if (value === "true") return true;
                throw `Recieved unparsable boolean ${value}`
            case "string":
                if (typeof value === "string") return value;
                return String(value);
            default:
                throw `Unhandled primitive value type ${meta.type}`
        }
    }

    constructor(private mode: "map" | "convert") {
        super();
    }
}


/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function convertToModel<TMeta extends ClassType, TModel extends Model<TMeta>>(value: {[k: string]: any}, metadata: TMeta): TModel {
    if (value == null) return value;
    return new ModelConversionVisitor("convert").visitObject(value, metadata);
}

/**
 * Transforms a raw value into a valid implemenation of a model value.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export function convertValueToModel(value: any, metadata: Value): any | null {
    if (value == null) return value;
    return new ModelConversionVisitor("convert").visitValue(value, metadata);
}

/**
 * Maps the given object with data properties into a valid implemenation of TModel.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param object The object with data properties that should be mapped to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export function mapToModel<TMeta extends ClassType, TModel extends Model<TMeta>>(object: {[k: string]: any}, metadata: TMeta): TModel {
    if (!object) return object;

    return new ModelConversionVisitor("map").visitObject(object, metadata);
}

/**
 * Maps a raw value into a valid implemenation of a model value.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export function mapValueToModel(value: any, metadata: Value): any | null {
    if (value === null || value === undefined) return value;
    return new ModelConversionVisitor("map").visitValue(value, metadata);
}

export function updateFromModel<TMeta extends ClassType, TModel extends Model<TMeta>>(target: TModel, source: TModel): TModel {

    return Object.assign(target, source);
    // I was going to go further with this and make it more like the Knockout code,
    // but I'm not sure that's whats best. The knockout code did what it did largely to trick
    // knockout into being performant. I think that if complex object merging strategies are needed,
    // we should build that as a configurable feature of the ViewModels, and not have it be the default behavior.
    // For now, we'll just object.assign and call it good.

    /*
    const metadata = target.$metadata;

    for (const prop of Object.values(metadata.props).filter(p => p.type == "collection")) {

        switch (prop.type) {
            case "collection":
                switch (prop.role) {
                    case "value":
                        // something 
                        break;
                    case "collectionNavigation":
                        // something 
                        break;
                }
                break;
            case "model":
            case "object":
                // something 
                break;
            default: 
                // @ts-ignore
                target[propName] = source[propName];
        }
    }

    // Continue for objects, and then for values, in a similar fashion to the knockout code.
    */
}

class MapToDtoVisitor extends Visitor<any | undefined, any[] | undefined, any | undefined> {
    private depth: number = 0;

    public visitObject(value: any, meta: ClassType) {
        // If we've exceded max depth, return undefined to prevent the 
        // creation of an entry in the parent object for this object.
        if (this.depth >= this.maxObjectDepth) return undefined;
        this.depth++;

        const props = meta.props;
        const output: any = {}
        for (const propName in props) {
            const propMeta = props[propName];
            
            if (propMeta.isReadOnly) continue;

            if (propName in value) {
                const newValue = this.visitValue(value[propName], propMeta);
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
                const objectValue = value[propMeta.navigationProp.name];
                if (objectValue) {
                    const objectValuePkValue = objectValue[propMeta.navigationProp.typeDef.keyProp.name];
                    if (objectValuePkValue != null) {
                        output[propName] = objectValuePkValue
                    }
                }
                propMeta.principalType.keyProp.name
            }
        }

        this.depth--;

        return output;
    }

    public visitCollection(value: any[], meta: CollectionValue) {
        // If we've exceded max depth, return undefined to prevent the 
        // creation of an entry in the parent object for this collection.
        if (this.depth >= this.maxObjectDepth) return undefined;
        
        // Don't increase depth for collections - only objects increase depth.
        const ret = super.visitCollection(value, meta);
        
        return ret;
    }

    public visitDateValue(value: any, meta: DateValue) {
        if (isValid(value)) {
            // TODO: exclude timezone (Z) for DateTime, keep it for DateTimeOffset
            value = format(value, 'YYYY-MM-DDTHH:mm:ss.SSSZ')
        } else if (value != null) {
            console.warn(`Invalid date couldn't be mapped: ${value}`)
            value = null
        }
        return value;
    }

    constructor(private maxObjectDepth: number = 1) {
        super();
    }
}

export function mapToDto<T extends Model<ClassType>>(object: T | null | undefined): {} | null {
    if (object === null || object === undefined) return null;

    if (!object.$metadata){
        throw "Object has no $metadata property."
    }
    
    var dto = new MapToDtoVisitor(1).visitObject(object, object.$metadata);

    return dto;
}

export function mapValueToDto(value: any, metadata: Value): any | null {
    if (value === null || value === undefined) return value;
    return new MapToDtoVisitor(1).visitValue(value, metadata);
}



/** Visitor that maps its input to a string representation of its value, suitable for display. */
class DisplayVisitor extends Visitor<string | null, string | null, string | null> {
    public visitObject(value: any, meta: ClassType): string | null {
        if (value == null) return value;

        if (meta.displayProp) {
            return this.visitValue(value[meta.displayProp.name], meta.displayProp);
        } else {
            // https://stackoverflow.com/a/46908358 - stringify only first-level properties.
            try {
                return JSON.stringify(value, function (k, v) { return k ? "" + v : v; });
            } catch {
                return value.toLocaleString();
            }
        }
    }

    public visitCollection(value: any[], meta: CollectionValue): string | null {
        if (!value) return null;
        if (!Array.isArray(value)) throw `Value for collection ${meta.name} was not an array`;

        // Is this what we want? I think so - its the cleanest option.
        // Perhaps an prop that controls this would be best.
        if (value.length == 0) return "";
        // TODO: a prop that controls this number would also be good.
        if (value.length <= 5) {
            return (value)
                .map<string>(childItem => 
                    this.visitValue(childItem, meta.itemType) 
                    || '???' // TODO: what should this be for un-displayable members of a collection?
                )
                .join(", ")
        }
        return value.length.toLocaleString() + " items"; // TODO: i18n
    }

    public visitEnumValue(value: any, meta: EnumValue): string | null {
        if (value == null) return value;
        const enumData = meta.typeDef.valueLookup[value];
        if (!enumData) return '';
        return enumData.displayName;
    }

    public visitDateValue(value: any, meta: DateValue): string | null {
        if (value == null) return value;
        return value.toLocaleString();
    }

    public visitPrimitiveValue(value: any, meta?: PrimitiveValue): string | null {
        if (value == null) return value;
        return value.toLocaleString();
    }
}

/** Singleton instance of `GetDisplayVisitor`, since the visitor is stateless. */
const displayVisitor = new DisplayVisitor();

/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
export function modelDisplay<T extends Model<TMeta>, TMeta extends ClassType>(item: T): string | null {
    const modelMeta = item.$metadata
    if (!modelMeta) {
        throw `Item passed to modelDisplay(item) is missing its $metadata property`
    }

    return displayVisitor.visitObject(item, item.$metadata);
}

/**
 * Given a model instance and a descriptor of a property on the instance,
 * return a string representation of the property suitable for display.
 * @param item An instance of the model that holds the property to be displayed
 * @param prop The property to be displayed - either the name of a property or a property metadata object.
 */
export function propDisplay<T extends Model<TMeta>, TMeta extends ClassType>(item: T, prop: Property | PropNames<TMeta>) {
    const propMeta = resolvePropMeta(item.$metadata, prop);

    var value = (item as Indexable<T>)[propMeta.name];
    return displayVisitor.visitValue(value, propMeta);
}
