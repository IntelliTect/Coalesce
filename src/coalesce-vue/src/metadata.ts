
/* -----------------------------
   ---------- GENERAL ----------
   -----------------------------
*/

/** Role that a value or property plays in a relational model. */
export type ValueRole = "value" | "primaryKey" | "foreignKey" | "referenceNavigation" | "collectionNavigation"


/** Root metadata object, off of which all metadata for the application's data model is stored. */
export interface Domain {
    // models: { [modelName: string]: ModelMetadata },
    // externalTypes: { [modelName: string]: ExternalTypeMetadata },
    types: { [modelName: string]: ClassType }
    enums: { [modelName: string]: EnumType },
}




/* -----------------------------
   ---- TYPE DISCRIMINATORS ----
   -----------------------------
*/

/** Type discriminator of types that are natively represented in javascript, i.e. they are valid results of the typeof operator. */
export type NativeTypeDiscriminator = "string" | "number"  | "boolean"
/** 
 * Type discriminator of types that do not have a `CustomType` representation in the metadata.
 * Enums do have such a representation, and so aren't included.
 */
export type SimpleTypeDiscriminator = NativeTypeDiscriminator | "date"
/** Type discriminator of non-object types */
export type ValueTypeDiscriminator = SimpleTypeDiscriminator | "enum"


/** Type discriminator of object-based types. */
export type ObjectTypeDiscriminator = "model" | "object"
/** Type discriminator of custom types that have their own metadata representation. */
export type CustomTypeDiscriminator = ObjectTypeDiscriminator | "enum"

/** Type discriminator of all non-collection types */
export type NonCollectionTypeDiscriminator = ValueTypeDiscriminator | ObjectTypeDiscriminator

/** Union of all valid type discriminators. */
export type TypeDiscriminator = NonCollectionTypeDiscriminator | "collection"




/* -----------------------------
   ------- TYPE METADATA -------
   -----------------------------
*/

/** Base properties found on all pieces of metadata. */
export interface Metadata {
    /** The kind of type represented by the property or type. */
    readonly type: TypeDiscriminator

    /** The camel-cased, machine-readable name of the property or type. */
    readonly name: string

    // TODO: i18n? How does it fit into this property? Do we take a dependency on an i18n framework and compute it in a getter?
    /** The human-readable name of the property or type */
    displayName: string
}

/** Common properties for custom object types. */
export interface CustomReferenceTypeBase extends Metadata {
    /** The properties of a the represented type */
    readonly props: { [propName in string]: Property } 

    /** 
     * The description of the property that holds a value that offers some textual representation of this model. 
     * This property is not necessarily a string property - it may even be a object property.
     * To get a textual representation of any object, use function `modelDisplay` from the `model` module.
    */
    readonly displayProp?: Property
}

/** Represents a type that is not part of a relational model. */
export interface ExternalType extends CustomReferenceTypeBase {
    readonly type: "object"
}

/** Represents a type that is part of a relational model and can be identified by a single, unique key. */
export interface ModelType extends CustomReferenceTypeBase {
    readonly type: "model"

    /** The methods of the represented entity */
    readonly methods: { [methodName in string]: Method }

    // NOTE: This is declared on CustomReferenceTypeBase as optional - we re-define it here as non-optional
    readonly displayProp: Property

    /** The primary key property of the entity */
    readonly keyProp: Property

    /** 
     * The URI path segment that identifies this model in API endpoints.
     * This value contains no leading/trailing slashes.
     */
    readonly controllerRoute: string;
}

/** Represents a value of an enum */
export interface EnumValue {
    readonly strValue: string
    readonly displayName: string
    readonly value: number
}

/** A dictionary with both string and numeric keys for looking up `EnumValue` objects by their string or numeric value. */
export type EnumValues<K extends string> = 
  { [strValue in K]: EnumValue } 
& { [n: number]: EnumValue | undefined } 

/** Utility function for creating the required properties of `EnumType<>` from an array of `EnumValue`  */
export function getEnumMeta<K extends string>(values: EnumValue[]): {
    readonly valueLookup: EnumValues<K>,
    readonly values: EnumValue[]
} {
    return {
        valueLookup: {
            ...values.reduce((obj, v) => Object.assign(obj, {
                [v.strValue]: v, 
                [v.value]: v
            } as EnumValues<K>), {} as any)
        }, 
        values: values
    }
}

/** Metadata representation of an enum type */
export interface EnumType<K extends string = string> extends Metadata {
    readonly type: "enum"

    /** A lookup object with both string and numeric keys for obtaining metadata about a particular enum value. */
    readonly valueLookup: EnumValues<K>

    /** 
     * The collection of all declared values of this enumeration.
     */
    readonly values: EnumValue[]

    // TODO: support [Flags] enums? Would need special handling for displaying/editing, and a flag on the metadata to flag it as [Flags]
}

/** Union of all metadata descriptions of object-based types. */
export type ClassType = ExternalType | ModelType 

/** Union of all metadata descriptions of custom types, including enums. */
export type CustomType = ClassType | EnumType

/** 
 * All valid types that can be contained within a collection.
 * This union includes both type discriminator strings and metadata objects.
 */
export type CollectableType = CustomType | SimpleTypeDiscriminator





/* -----------------------------
   ----- PROPERTY METADATA -----
   -----------------------------
*/

export interface PropMetaBase extends Metadata {
    readonly role: ValueRole
}
export interface PrimitiveProperty extends PropMetaBase {
    readonly type: NativeTypeDiscriminator
}
export interface DateProperty extends PropMetaBase {
    readonly type: "date"
}
export interface EnumProperty extends PropMetaBase {
    readonly type: "enum"
    readonly typeDef: EnumType
}
export interface ObjectProperty extends PropMetaBase {
    readonly type: "object"
    readonly typeDef: ExternalType
}
export interface ModelProperty extends PropMetaBase {
    readonly type: "model"
    readonly typeDef: ModelType
    readonly foreignKey: PrimitiveProperty
    readonly principalKey: PrimitiveProperty
}
export interface SimpleCollectionProperty extends PropMetaBase {
    readonly type: "collection"
    readonly collectedType: SimpleTypeDiscriminator
}
export interface EnumCollectionProperty extends PropMetaBase {
    readonly type: "collection"
    readonly collectedType: "enum"
    readonly collectedTypeDef: EnumType
}
export interface ExternalTypeCollectionProperty extends PropMetaBase {
    readonly type: "collection"
    readonly collectedType: "object"
    readonly collectedTypeDef: ExternalType
}
export interface ModelCollectionProperty extends PropMetaBase {
    readonly type: "collection"
    readonly collectedType: "model"
    readonly collectedTypeDef: ModelType
    readonly foreignKey: PrimitiveProperty
}
export type CollectionProperty = 
  SimpleCollectionProperty
| EnumCollectionProperty
| ExternalTypeCollectionProperty
| ModelCollectionProperty

export type Property = 
  PrimitiveProperty
| DateProperty
| EnumProperty
| ObjectProperty
| ModelProperty
| CollectionProperty




/* -----------------------------
   ------ METHOD METADATA ------
   -----------------------------
*/

export interface Method extends Metadata  {
    readonly params: Property[]
}




/* -----------------------------
   ---- USEFUL TYPE ALIASES ----
   -----------------------------
*/

export type PropNames<TMeta extends ClassType, Kind extends Property = Property>
    = { [K in keyof TMeta["props"]]: TMeta["props"][K] extends Kind ? K : never }[keyof TMeta["props"]];

// This doesn't support restriction of property kind - typescript makes unintelligible intellisense tooltips if we do.
export type PropertyOrName<TMeta extends ClassType>
    = Property | PropNames<TMeta, Property>

export function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string) : Exclude<TProp, string>
export function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string, slient: true) : Exclude<TProp, string> | undefined
export function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string, slient: boolean = false)
{
    const propMeta = typeof propOrString == "string" ? metadata.props[propOrString] : propOrString
    if (!propMeta) {
        if (slient) return undefined
        throw `Unknown property ${propOrString}`
    } else if (metadata.props[propMeta.name] !== propMeta) {
        if (slient) return undefined
        throw `Property ${propMeta.name} does not belong to object of type ${metadata.name}`
    }
    return propMeta
}



export function isClassType(prop: TypeDiscriminator | CollectableType | null | undefined): prop is ClassType {
    if (prop == "object" || prop == "model") return true;
    return prop !== null && typeof(prop) === "object" && (prop.type === "model" || prop.type === "object")
}