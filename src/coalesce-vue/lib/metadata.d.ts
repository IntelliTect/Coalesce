/** Role that a value or property plays in a relational model. */
export declare type ValueRole = "value" | "primaryKey" | "foreignKey" | "referenceNavigation" | "collectionNavigation";
/** Root metadata object, off of which all metadata for the application's data model is stored. */
export interface Domain {
    types: {
        [modelName: string]: ClassType;
    };
    enums: {
        [modelName: string]: EnumType;
    };
}
/** Type discriminator of non-object types that are natively represented in javascript, i.e. they are valid results of the typeof operator. */
export declare type NativeTypeDiscriminator = "string" | "number" | "boolean";
/**
 * Type discriminator of types that do not have a `CustomType` representation in the metadata.
 * Enums do have such a representation, and so aren't included.
 */
export declare type SimpleTypeDiscriminator = NativeTypeDiscriminator | "date";
/** Type discriminator of non-object types */
export declare type ValueTypeDiscriminator = SimpleTypeDiscriminator | "enum";
/** Type discriminator of object-based types. */
export declare type ClassTypeDiscriminator = "model" | "object";
/** Type discriminator of custom types that have their own metadata representation. */
export declare type CustomTypeDiscriminator = ClassTypeDiscriminator | "enum";
/** Type discriminator of all non-collection types */
export declare type NonCollectionTypeDiscriminator = ValueTypeDiscriminator | ClassTypeDiscriminator;
/** Union of all valid type discriminators. */
export declare type TypeDiscriminator = NonCollectionTypeDiscriminator | "collection";
/** Base properties found on all pieces of metadata. */
export interface Metadata {
    /** The camel-cased, machine-readable name of the value or type. */
    readonly name: string;
    /** The human-readable name of the value or type */
    displayName: string;
}
/** Common properties for custom object types. */
export interface CustomReferenceTypeBase extends Metadata {
    /** The properties of a the represented type */
    readonly props: {
        [propName in string]: Property;
    };
    /**
     * The description of the property that holds a value that offers some textual representation of this model.
     * This property is not necessarily a string property - it may even be a object property.
     * To get a textual representation of any object, use function `modelDisplay` from the `model` module.
    */
    readonly displayProp?: Property;
}
/** Represents a type that is not part of a relational model. */
export interface ObjectType extends CustomReferenceTypeBase {
    readonly type: "object";
}
/** Represents a type that is part of a relational model and can be identified by a single, unique key. */
export interface ModelType extends CustomReferenceTypeBase {
    readonly type: "model";
    /** The methods of the represented entity */
    readonly methods: {
        [methodName in string]: Method;
    };
    readonly displayProp: Property;
    /** The primary key property of the entity */
    readonly keyProp: Property;
    /**
     * The URI path segment that identifies this model in API endpoints.
     * This value contains no leading/trailing slashes.
     */
    readonly controllerRoute: string;
}
/** Represents a value of an enum */
export interface EnumMember {
    readonly strValue: string;
    readonly displayName: string;
    readonly value: number;
}
/** A dictionary with both string and numeric keys for looking up `EnumValue` objects by their string or numeric value. */
export declare type EnumMembers<K extends string> = {
    [strValue in K]: EnumMember;
} & {
    [n: number]: EnumMember | undefined;
};
/** Utility function for creating the required properties of `EnumType<>` from an array of `EnumValue`  */
export declare function getEnumMeta<K extends string>(values: EnumMember[]): {
    readonly valueLookup: EnumMembers<K>;
    readonly values: EnumMember[];
};
/** Metadata representation of an enum type */
export interface EnumType<K extends string = string> extends Metadata {
    readonly type: "enum";
    /** A lookup object with both string and numeric keys for obtaining metadata about a particular enum value. */
    readonly valueLookup: EnumMembers<K>;
    /**
     * The collection of all declared values of this enumeration.
     */
    readonly values: EnumMember[];
}
/** Union of all metadata descriptions of object-based types. */
export declare type ClassType = ObjectType | ModelType;
/** Union of all metadata descriptions of custom types, including enums. */
export declare type CustomType = ClassType | EnumType;
/** A special value that represents void.
 * Not be included in the standard unions of all `Value` kinds,
 * since its usage only applies to method returns - it should instead
 * only be included in unions where its usage is applicable.
 * Also note that its `type` propety is not part of `TypeDiscriminator`.
 */
export interface VoidValue extends Metadata {
    readonly role: "value";
    readonly type: "void";
}
/**
 * Base interface for all normal value metadata representations.
 * For our purposes, a value is defined as the usage of a type.
 * This includes properties, method parameters, method returns,
 * items in a collection, and more.
 */
export interface ValueMeta<TType extends TypeDiscriminator> extends Metadata {
    /**
     * Role that the value or property plays in a relational model.
     * Some values may be nonapplicable in some contexts, like method parameters.
     */
    readonly role: ValueRole;
    readonly type: TType;
}
/**
 * Base interface for values whose type has a custom definition in the metadata.
 */
export interface ValueMetaWithTypeDef<TType extends TypeDiscriminator, TTypeDef extends Metadata> extends ValueMeta<TType> {
    /** Full description of the type represented by this value. */
    readonly typeDef: TTypeDef;
}
/** Represents the usage of a primitive value (string, number, or bool) */
export interface PrimitiveValue extends ValueMeta<NativeTypeDiscriminator> {
    role: "value" | "foreignKey" | "primaryKey";
}
/** Represents the usage of a date */
export interface DateValue extends ValueMeta<"date"> {
    role: "value";
}
/** Represents the usage of an enum */
export interface EnumValue extends ValueMetaWithTypeDef<"enum", EnumType> {
    role: "value";
}
/** Represents the usage of an 'external type', i.e. an object that is not part of a relational model */
export interface ObjectValue extends ValueMetaWithTypeDef<"object", ObjectType> {
    role: "value";
}
/** Represents the usage of an object that is part of a relational model */
export interface ModelValue extends ValueMetaWithTypeDef<"model", ModelType> {
    role: "value" | "referenceNavigation";
}
/** Represents the usage of a collection of values */
export interface CollectionValue extends ValueMeta<"collection"> {
    role: "value" | "collectionNavigation";
    readonly itemType: NonCollectionValue;
}
/** Union of all representations of the usage of types with explicit type metadata */
export declare type CustomTypeValue = EnumValue | ObjectValue | ModelValue;
/** Union of all representations of the usage of non-collection types */
export declare type NonCollectionValue = PrimitiveValue | DateValue | CustomTypeValue;
/** Union of all representations of the usage of a type */
export declare type Value = NonCollectionValue | CollectionValue;
/** Represents a primitive property */
export interface PrimitiveProperty extends PrimitiveValue {
    readonly role: "value";
}
export interface PrimaryKeyProperty extends PrimitiveValue {
    readonly role: "primaryKey";
}
/** Represents a property that serves as a foreign key */
export interface ForeignKeyProperty extends PrimitiveValue {
    readonly role: "foreignKey";
    readonly type: "string" | "number";
    readonly principalKey: PrimaryKeyProperty;
    readonly principalType: ModelType;
    readonly navigationProp?: ModelProperty;
}
/** Represents a date property */
export interface DateProperty extends DateValue {
}
/** Represents an enum property */
export interface EnumProperty extends EnumValue {
}
/** Represents an object property */
export interface ObjectProperty extends ObjectValue {
}
/**
 * Represents an object property that represents the foreign end of
 * a 1-to-1 or 1-to-many relationship in a relational model.
 */
export interface ModelProperty extends ModelValue {
    readonly role: "referenceNavigation";
    readonly foreignKey: ForeignKeyProperty;
    readonly principalKey: PrimaryKeyProperty;
}
/**
 * Represents a collection property that simple contains values that do not
 * have any special meaning in a relational model.
 */
export interface BasicCollectionProperty extends CollectionValue {
    readonly role: "value";
}
/**
 * Represents a collection property that represents
 * the foreign objects in a many-to-1 relationship in a relational model.
 */
export interface ModelCollectionNavigationProperty extends CollectionValue {
    readonly role: "collectionNavigation";
    /**
     * Reference to the property on the type contained in this collection that relates
     * to the primary key of the model that owns the collection property.
     */
    readonly foreignKey: ForeignKeyProperty;
    readonly itemType: ModelValue;
}
export declare type CollectionProperty = BasicCollectionProperty | ModelCollectionNavigationProperty;
export declare type Property = PrimitiveProperty | PrimaryKeyProperty | ForeignKeyProperty | DateProperty | EnumProperty | ObjectProperty | ModelProperty | CollectionProperty;
export interface Method extends Metadata {
    /** The return type of the method. */
    readonly return: Value | VoidValue;
    /** The parameters of the method */
    readonly params: {
        [paramName in string]: Value;
    };
}
export declare type PropNames<TMeta extends ClassType, Kind extends Property = Property> = {
    [K in keyof TMeta["props"]]: TMeta["props"][K] extends Kind ? K : never;
}[keyof TMeta["props"]];
export declare type PropertyOrName<TMeta extends ClassType> = Property | PropNames<TMeta, Property>;
export declare function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string): Exclude<TProp, string>;
export declare function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string, slient: true): Exclude<TProp, string> | undefined;
