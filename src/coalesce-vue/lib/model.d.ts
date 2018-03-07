import { ClassType, IHaveMetadata, Property, PropNames } from "./metadata";
/**
 * Represents a model with metadata information.
 */
export interface Model<TMeta extends ClassType> extends IHaveMetadata {
    readonly $metadata: TMeta;
}
/**
 * Transforms a given object with data properties into a valid implemenation of TModel.
 * This function mutates its input and all descendent properties of its input - it does not map to a new object.
 * @param object The object with data properties that should be converted to a TModel
 * @param metadata The metadata describing the TModel that is desired
 */
export declare function convertToModel<TMeta extends ClassType, TModel extends Model<TMeta>>(object: {
    [k: string]: any;
}, metadata: TMeta): TModel;
export declare function mapToDto<T extends Model<ClassType>>(object: T | null | undefined): {} | null;
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
