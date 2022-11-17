/* -----------------------------
   ---------- GENERAL ----------
   -----------------------------
*/

import { markRaw } from "vue";

/** Role that a value or property plays in a relational model. */
export type ValueRole =
  | "value"
  | "primaryKey"
  | "foreignKey"
  | "referenceNavigation"
  | "collectionNavigation";

/** Root metadata object, off of which all metadata for the application's data model is stored. */
export interface Domain {
  // models: { [modelName: string]: ModelMetadata },
  // externalTypes: { [modelName: string]: ExternalTypeMetadata },
  types: { [modelName: string]: ClassType };
  enums: { [modelName: string]: EnumType };
  services: { [modelName: string]: Service };
}

/* -----------------------------
   ---- TYPE DISCRIMINATORS ----
   -----------------------------
*/

/** Type discriminators of non-object types that are natively represented in javascript, i.e. they are valid results of the typeof operator. */
export type NativePrimitiveTypeDiscriminator = "string" | "number" | "boolean";

/** Type discriminators of object types that exist natively in javascript. */
export type NativeObjectTypeDiscriminator = "date" | "file" | "binary";

/**
 * Type discriminators of types that do not have a `CustomType` representation in the metadata.
 * Enums do have such a representation, and so aren't included.
 */
export type NativeTypeDiscriminator =
  | NativePrimitiveTypeDiscriminator
  | NativeObjectTypeDiscriminator;

/** Type discriminators of non-poco types */
export type ValueTypeDiscriminator = NativeTypeDiscriminator | "enum";

/** Type discriminators of object-based types. */
export type ClassTypeDiscriminator = "model" | "object";
/** Type discriminators of custom types that have their own metadata representation. */
export type CustomTypeDiscriminator = ClassTypeDiscriminator | "enum";

/** Type discriminator of all non-collection types */
export type NonCollectionTypeDiscriminator =
  | ValueTypeDiscriminator
  | ClassTypeDiscriminator
  | "unknown";

/** Union of all valid type discriminators. */
export type TypeDiscriminator = NonCollectionTypeDiscriminator | "collection";

/** Maps a type discriminator to its best match TypeScript type */
export type TypeDiscriminatorToType<T> = T extends "string"
  ? string
  : T extends "number"
  ? number
  : T extends "boolean"
  ? boolean
  : T extends "date"
  ? Date
  : T extends "file"
  ? File
  : T extends "unknown"
  ? unknown
  : T extends "binary"
  ? Uint8Array | string
  : any;

/* -----------------------------
   ------- TYPE METADATA -------
   -----------------------------
*/

/** Base properties found on all pieces of metadata. */
export interface Metadata {
  /** The machine-readable name of the value or type. Values are typically camel-cased; types are typically PascalCased. */
  readonly name: string;

  // TODO: i18n? How does it fit into this property? Do we take a dependency on an i18n framework and compute it in a getter?
  /** The human-readable name of the value or type */
  readonly displayName: string;
}

/** Common properties for custom object types. */
export interface CustomReferenceTypeBase extends Metadata {
  readonly type: ClassTypeDiscriminator;

  /** The properties of a the represented type */
  readonly props: { [propName in string]: Property };

  /**
   * The description of the property that holds a value that offers some textual representation of this model.
   * This property is not necessarily a string property - it may even be a object property.
   * To get a textual representation of any object, use function `modelDisplay` from the `model` module.
   */
  readonly displayProp?: Property;
}

export interface ApiRoutedType {
  /**
   * The URI path segment that identifies this type in API endpoints.
   * This value contains no leading/trailing slashes.
   */
  readonly controllerRoute: string;

  /** The methods of the type that are invokable via API endpoints. */
  readonly methods: { [methodName in string]: Method };
}

/** Represents a type that is not part of a relational model. */
export interface ObjectType extends CustomReferenceTypeBase {
  readonly type: "object";
}

/** Flags representing the 3 different behaviors in Coalesce - Create, Edit, and Delete. */
export enum BehaviorFlags {
  Create = 1 << 0,
  Edit = 1 << 1,
  Delete = 1 << 2,
}

/** Represents a type that is part of a relational model and can be identified by a single, unique key. */
export interface ModelType extends CustomReferenceTypeBase, ApiRoutedType {
  readonly type: "model";

  // NOTE: This is declared on CustomReferenceTypeBase as optional - we re-define it here as non-optional
  readonly displayProp: Property;

  /** The primary key property of the entity */
  readonly keyProp: Property;

  /** Flags indicating the create/edit/delete capabilities of the type. */
  readonly behaviorFlags: BehaviorFlags;

  /** The data sources that can be used to query the API for objects of this type. */
  readonly dataSources: { [sourceName in string]: DataSourceType };
}

export interface DataSourceType extends Metadata {
  readonly type: "dataSource";
  readonly isDefault?: true;

  /** The parameters of the data source.
   * Stored as `props` so it can be treated like a ModelType/ObjectType in many cases.
   */
  readonly props: {
    [paramName in string]: PrimitiveProperty | DateProperty | EnumProperty;
  };
  // NOTE: this union is the currently supported set of data source parameters.
  // When we support more types in the future (e.g. objects), adjust accordingly.
}

export interface Service extends Metadata, ApiRoutedType {
  readonly type: "service";
}

/** Represents a value of an enum */
export interface EnumMember {
  readonly value: number;
  readonly strValue: string;
  readonly displayName: string;
  readonly description?: string;
}

/** A dictionary with both string and numeric keys for looking up `EnumValue` objects by their string or numeric value. */
export type EnumMembers<K extends string> = { [strValue in K]: EnumMember } & {
  [n: number]: EnumMember;
};

/** Utility function for creating the required properties of `EnumType<>` from an array of `EnumValue`  */
export function getEnumMeta<K extends string>(
  values: EnumMember[]
): {
  readonly valueLookup: EnumMembers<K>;
  readonly values: EnumMember[];
} {
  return {
    valueLookup: values.reduce((obj, v) => {
      return {
        ...obj,
        [v.strValue]: v,
        [v.value]: v,
      } as EnumMembers<K>;
    }, {} as any),
    values: values,
  };
}

/** Metadata representation of an enum type */
export interface EnumType<K extends string = string> extends Metadata {
  readonly type: "enum";

  /** A lookup object with both string and numeric keys for obtaining metadata about a particular enum value. */
  readonly valueLookup: EnumMembers<K>;

  /**
   * The collection of all declared values of this enumeration.
   */
  readonly values: EnumMember[];

  // TODO: support [Flags] enums? Would need special handling for displaying/editing, and a flag on the metadata to flag it as [Flags]
}

/** Union of all metadata descriptions of object-based types. */
export type ClassType = ObjectType | ModelType | DataSourceType;

/** Union of all metadata descriptions of custom types, including enums. */
export type CustomType = ClassType | EnumType;

/* -----------------------------
   ------ VALUE METADATA -------
   -----------------------------
*/

/** A special value that represents void.
 * Not be included in the standard unions of all `Value` kinds,
 * since its usage only applies to method returns - it should instead
 * only be included in unions where its usage is applicable.
 * Also note that its `type` property is not part of `TypeDiscriminator`.
 */
export interface VoidValue extends Metadata {
  readonly role: "value";
  readonly type: "void";
}

// /** Narrowing intersection interface for the 'itemType' value on collection values that represents the type contained within the collection.  */
// export interface CollectionItemValue {
//     // While this is currently always this string, no real reason to restrict it.
//     // name: "$collectionItem"

//     // While this is currently always emptystring, not sure that we want to restrict this in the typings.
//     // displayName: "" // Yep, always emptystring
// }

/** Narrowing intersection interface for the 'return' value on methods.  */
export interface MethodReturnValue {
  readonly name: "$return";
}

export type Rule<T> = (val: T | null | undefined) => true | string;

export interface Rules {
  required?: Rule<any>;
  minLength?: Rule<string>;
  maxLength?: Rule<string>;
  min?: Rule<number>;
  max?: Rule<number>;
  pattern?: Rule<string>;
  email?: Rule<string>;
  phone?: Rule<string>;
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
  readonly description?: string;
}

/**
 * Base interface for values whose type has a custom definition in the metadata.
 */
export interface ValueMetaWithTypeDef<
  TType extends TypeDiscriminator,
  TTypeDef extends Metadata
> extends ValueMeta<TType> {
  /** Full description of the type represented by this value. */
  readonly typeDef: TTypeDef;
}

/** Represents the usage of a string */
export interface StringValue extends ValueMeta<"string"> {
  readonly role: "value" | "foreignKey" | "primaryKey";

  /** Details about the kind of data that the string represents. */
  readonly subtype?:
    | "password"
    | "url"
    | "email"
    | "tel"
    | "multiline"
    | "url-image";

  readonly rules?: Rules;
}

/** Represents the usage of a number */
export interface NumberValue extends ValueMeta<"number"> {
  readonly role: "value" | "foreignKey" | "primaryKey";
  readonly rules?: Rules;
}

/** Represents the usage of a boolean */
export interface BooleanValue extends ValueMeta<"boolean"> {
  readonly rules?: Rules;
}

/** Represents the usage of a primitive value (string, number, or bool) */
export type PrimitiveValue = StringValue | NumberValue | BooleanValue;

/** Represents the usage of a date */
export interface DateValue extends ValueMeta<"date"> {
  readonly role: "value";
  readonly dateKind: "date" | "time" | "datetime";

  /** True if the date value is insensitive to timezone offsets
   * (i.e. the C# type is `DateTime`, not `DateTimeOffset`) */
  readonly noOffset?: boolean;

  readonly rules?: Rules;
}

export interface FileValue extends ValueMeta<"file"> {
  readonly role: "value";
}

export interface BinaryValue extends ValueMeta<"binary"> {
  readonly role: "value";

  /** True if the value only supports base64 representation (and not raw formats like ArrayBuffer) */
  readonly base64?: boolean;
}

/** Represents the usage of an unknown type, i.e. an object declared as `object` in C#.
 * Such types have no metadata and their values only exist as their JSON representation. */
export interface UnknownValue extends ValueMeta<"unknown"> {
  readonly role: "value";
}

/** Represents the usage of an enum */
export interface EnumValue extends ValueMetaWithTypeDef<"enum", EnumType> {
  readonly role: "value" | "foreignKey" | "primaryKey";
  readonly rules?: Rules;
}

/** Represents the usage of an 'external type', i.e. an object that is not part of a relational model */
export interface ObjectValue
  extends ValueMetaWithTypeDef<"object", ObjectType> {
  readonly role: "value";
}

/** Represents the usage of an object that is part of a relational model */
export interface ModelValue extends ValueMetaWithTypeDef<"model", ModelType> {
  readonly role: "value" | "referenceNavigation";
}

/** Represents the usage of a collection of values */
export interface CollectionValue extends ValueMeta<"collection"> {
  readonly role: "value" | "collectionNavigation";
  readonly itemType: NonCollectionValue; // & CollectionItemValue
}

/** Represents the usage of a collection of model values */
export interface ModelCollectionValue extends ValueMeta<"collection"> {
  readonly role: "value" | "collectionNavigation";
  readonly itemType: ModelValue;
}

/** Union of all representations of the usage of types with explicit type metadata */
export type CustomTypeValue = EnumValue | ObjectValue | ModelValue;

/** Union of all representations of the usage of non-collection types */
export type NonCollectionValue =
  | PrimitiveValue
  | DateValue
  | FileValue
  | BinaryValue
  | UnknownValue
  | CustomTypeValue;

/** Union of all representations of the usage of a type */
export type Value = NonCollectionValue | CollectionValue;

/** Union of all type usages that can be represented in JSON. */
export type JsonValue = Exclude<Value, FileValue>;

/* -----------------------------
   ----- PROPERTY METADATA -----
   -----------------------------
*/

export enum HiddenAreas {
  List = 1 << 0,
  Edit = 1 << 1,
}

export interface PropertyBase {
  /** True if the property should be skipped when mapping to a DTO. */
  readonly dontSerialize?: boolean | undefined;

  /** Enum indicating what admin views the property is hidden on, if any. */
  readonly hidden?: HiddenAreas;
}

/** Represents a primitive property */
export type PrimitiveProperty = PropertyBase &
  PrimitiveValue & {
    readonly role: "value";
  };

/** Represents a property that serves as a primary key */
export type PrimaryKeyProperty = PropertyBase &
  (StringValue | NumberValue | EnumValue) & {
    readonly role: "primaryKey";
  };

/** Represents a property that serves as a foreign key */
export type ForeignKeyProperty = PropertyBase &
  (StringValue | NumberValue | EnumValue) & {
    readonly role: "foreignKey";
    readonly principalKey: PrimaryKeyProperty;
    readonly principalType: ModelType;
    readonly navigationProp?: ModelReferenceNavigationProperty;
  };

/** Represents a date property */
export interface DateProperty extends PropertyBase, DateValue {}

/** Represents an enum property */
export type EnumProperty = PropertyBase &
  EnumValue & {
    readonly role: "value";
  };

/** Represents an object property */
export interface ObjectProperty extends PropertyBase, ObjectValue {}

/** Represents an unknown property */
export interface UnknownProperty extends PropertyBase, UnknownValue {}

/** Represents a binary property */
export interface BinaryProperty extends PropertyBase, BinaryValue {
  readonly base64: true;
}

/**
 * Represents a model property that simply exists as a value,
 * not as a relational navigation property.
 */
export interface ModelValueProperty extends PropertyBase, ModelValue {
  readonly role: "value";
}

/**
 * Represents an object property that represents the foreign end of
 * a 1-to-1 or 1-to-many relationship in a relational model.
 */
export interface ModelReferenceNavigationProperty
  extends PropertyBase,
    ModelValue {
  readonly role: "referenceNavigation";
  readonly foreignKey: ForeignKeyProperty;
  readonly principalKey: PrimaryKeyProperty;
  readonly inverseNavigation?: ModelCollectionNavigationProperty;
}

/**
 * Represents a collection property that simple contains values that do not
 * have any special meaning in a relational model.
 */
export interface BasicCollectionProperty extends PropertyBase, CollectionValue {
  readonly role: "value";
}

/**
 * Represents a collection property that represents
 * the foreign objects in a many-to-1 relationship in a relational model.
 */
export interface ModelCollectionNavigationProperty
  extends PropertyBase,
    CollectionValue {
  readonly role: "collectionNavigation";
  /**
   * Reference to the property on the type contained in this collection that relates
   * to the primary key of the model that owns the collection property.
   */
  readonly foreignKey: ForeignKeyProperty;
  readonly itemType: ModelValue; // & CollectionItemValue
  readonly inverseNavigation?: ModelReferenceNavigationProperty;

  readonly manyToMany?: {
    /** The name of the many-to-many collection. */
    readonly name: string;
    /** The human-readable name of the many-to-many collection. */
    displayName: string;
    /** The type represented by the other side of the many-to-many relationship. */
    readonly typeDef: ModelType;
    /** The foreign key on the join entity that refers to an entity on the current property's side of the relationship. */
    readonly nearForeignKey: ForeignKeyProperty;
    /** The navigation on the join entity that refers to an entity on the current property's side of the relationship. */
    readonly nearNavigationProp: ModelReferenceNavigationProperty;
    /** The foreign key on the join entity that refers to an entity on the other side of the relationship. */
    readonly farForeignKey: ForeignKeyProperty;
    /** The navigation on the join entity that refers to an entity on the other side of the relationship. */
    readonly farNavigationProp: ModelReferenceNavigationProperty;
  };
}

export type CollectionProperty =
  | BasicCollectionProperty
  | ModelCollectionNavigationProperty;

export type Property =
  | PrimitiveProperty
  | PrimaryKeyProperty
  | ForeignKeyProperty
  | DateProperty
  | EnumProperty
  | ObjectProperty
  | UnknownProperty
  | BinaryProperty
  | ModelValueProperty
  | ModelReferenceNavigationProperty
  | CollectionProperty;

/* -----------------------------
   ------ METHOD METADATA ------
   -----------------------------
*/

export type MethodParameter = Value & {
  /** If specified, the name of a property on the model that owns the parameter's method
   * that the value of this parameter should be automatically sourced from when invoked on viewmodels.
   */
  source?: Property;
};

export interface MethodBase extends Metadata {
  /** The HTTP method to use when calling the method. */
  readonly httpMethod: "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

  /** True if the method is static; otherwise undefined/false */
  readonly isStatic?: boolean;

  /** The parameters of the method */
  readonly params: { [paramName in string]: MethodParameter };
}

export interface ItemMethod extends MethodBase {
  /** The return type of the method. */
  readonly return: Value | VoidValue;

  /** The transport container for the return type, corresponding to "ListResult" or "ItemResult". */
  readonly transportType: "item";
}

export interface ListMethod extends MethodBase {
  /** The return type of the method. */
  readonly return: CollectionValue;

  /** The transport container for the return type, corresponding to "ListResult" or "ItemResult". */
  readonly transportType: "list";
}

export type Method = ItemMethod | ListMethod;

/* -----------------------------
   ---- USEFUL TYPE ALIASES ----
   -----------------------------
*/

export type PropsNames<
  Props extends ClassType["props"],
  Kind extends Property = Property
> = {
  [K in Extract<keyof Props, string>]: Props[K] extends Kind ? K : never;
}[Extract<keyof Props, string>];

export type PropNames<
  TMeta extends ClassType,
  Kind extends Property = Property
> = PropsNames<TMeta["props"], Kind>;

// This doesn't support restriction of property kind - typescript makes unintelligible intellisense tooltips if we do.
export type PropertyOrName<TMeta extends ClassType> =
  | Property
  | PropNames<TMeta, Property>;

export function resolvePropMeta<TProp extends Property>(
  metadata: ClassType,
  propOrString: TProp | string
): Exclude<TProp, string>;
export function resolvePropMeta<TProp extends Property>(
  metadata: ClassType,
  propOrString: TProp | string,
  slient: true
): Exclude<TProp, string> | undefined;
export function resolvePropMeta<TProp extends Property>(
  metadata: ClassType,
  propOrString: TProp | string,
  slient: boolean = false
) {
  const propMeta =
    typeof propOrString == "string"
      ? metadata.props[propOrString]
      : propOrString;
  if (!propMeta) {
    if (slient) return undefined;
    throw `Unknown property ${propOrString}`;
  } else if (metadata.props[propMeta.name] !== propMeta) {
    if (slient) return undefined;
    throw `Property ${propMeta.name} does not belong to object of type ${metadata.name}`;
  }
  return propMeta;
}

/** Recursively freeze the object, and convert any getter props into props
 * that hold the current value of the getter.
 */
export function solidify<T>(root: T): T {
  const map = new Map();

  function walk(o: any) {
    if (o == null) {
      return o;
    }

    if (Object.isFrozen(o)) return o;
    if (map.has(o)) return map.get(o);

    // Store that we've visited the object.
    // This lets us avoid infinite recursion.
    // We can't use the frozen status to check for recursion
    // because we can't freeze the object until we're done changing its props.
    map.set(o, o);

    for (const prop of Object.getOwnPropertyNames(o)) {
      const value = o[prop];
      if (value != null && typeof value === "object") {
        if (Object.getOwnPropertyDescriptor(o, prop)!.get) {
          // The prop is defined as a getter; eval it and store the static value.
          // The whole point of the getters is to make the metadata easier to generate,
          // since we can reference other types before those types are declared,
          // but they're not needed at runtime.
          Object.defineProperty(o, prop, { value: walk(value) });
        } else {
          o[prop] = walk(value);
        }
      }
    }

    return Object.freeze(markRaw(o));
  }

  return walk(root);
}
