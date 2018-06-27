import { ClassType, Property, PropNames, Value, EnumValue, PrimitiveValue, DateValue, CollectionValue, DataSourceType, ModelValue, ObjectValue } from "./metadata";
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
/**
 * Attempts to change the input value into a correctly-typed
 * result given some metadata describing the desired result.
 * Values that cannot be converted will throw an error.
*/
export declare function parseValue(value: null | undefined, meta: Value): null;
export declare function parseValue(value: any, meta: PrimitiveValue): null | string | number | boolean;
export declare function parseValue(value: any, meta: PrimitiveValue & {
    type: "string";
}): null | string;
export declare function parseValue(value: any, meta: PrimitiveValue & {
    type: "number";
}): null | number;
export declare function parseValue(value: any, meta: PrimitiveValue & {
    type: "boolean";
}): null | boolean;
export declare function parseValue(value: any, meta: EnumValue): null | number;
export declare function parseValue(value: any, meta: DateValue): null | Date;
export declare function parseValue(value: any, meta: ModelValue): null | object;
export declare function parseValue(value: any, meta: ObjectValue): null | object;
export declare function parseValue(value: any[], meta: CollectionValue): Array<any>;
/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export declare function convertToModel<TMeta extends ClassType, TModel extends Model<TMeta>>(object: {
    [k: string]: any;
}, metadata: TMeta): TModel;
/**
 * Transforms a raw value into a valid implemenation of a model value.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export declare function convertValueToModel(value: any, metadata: Value): any;
/**
 * Maps the given object with data properties into a valid implemenation of TModel.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param object The object with data properties that should be mapped to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export declare function mapToModel<TMeta extends ClassType, TModel extends Model<TMeta>>(object: {
    [k: string]: any;
}, metadata: TMeta): TModel;
/**
 * Maps a raw value into a valid implemenation of a model value.
 * This function returns a new copy of its input and all descendent properties of its input - it does not mutate its input.
 * @param value The value that should be converted
 * @param metadata The metadata describing the value
 */
export declare function mapValueToModel(value: any, metadata: Value): any;
/**
 * Updates the target model with values from the source model.
 * Any properties defined on the source will be copied to the target.
 * This perform a shallow copy of properties, using `Object.assign`.
 * @param target The model to be updated.
 * @param source The model whose values will be used to perform the update.
 */
export declare function updateFromModel<TMeta extends ClassType, TModel extends Model<TMeta>>(target: TModel, source: TModel): TModel;
/**
 * Maps the given object to a POJO suitable for JSON serialization.
 * Will not serialize child objects or collections.
 * @param object The object to map.
 */
export declare function mapToDto<T extends Model<ClassType>>(object: T | null | undefined): {} | null;
/**
 * Maps the given value to a representation suitable for JSON serialization.
 * Will not serialize the children of any objects encountered.
 * Will serialize objects found in arrays.
 * @param object The object to map.
 */
export declare function mapValueToDto(value: any, metadata: Value): any | null;
/**
 * Given a model instance, return a string representation of the instance suitable for display.
 * @param item The model instance to return a string representation of
 */
export declare function modelDisplay<T extends Model<TMeta>, TMeta extends ClassType>(item: T): string | null;
/**
 * Given a model instance and a descriptor of a property on the instance,
 * return a string representation of the property suitable for display.
 * @param item An instance of the model that holds the property to be displayed
 * @param prop The property to be displayed - either the name of a property or a property metadata object.
 */
export declare function propDisplay<T extends Model<TMeta>, TMeta extends ClassType>(item: T, prop: Property | PropNames<TMeta>): string | null;
/**
 * Given a value and metadata which describes that value,
 * return a string representation of the value suitable for display.
 * @param value Any value which is valid for the metadata provided.
 * @param prop The metadata which describes the value given.
 */
export declare function valueDisplay<T extends Model<TMeta>, TMeta extends ClassType>(value: any, meta: Value): string | null;
