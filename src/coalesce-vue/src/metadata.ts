
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

/** Type discriminator of non-object types that are natively represented in javascript, i.e. they are valid results of the typeof operator. */
export type NativeTypeDiscriminator = "string" | "number" | "boolean"
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
export interface EnumMember {
    readonly strValue: string
    readonly displayName: string
    readonly value: number
}

/** A dictionary with both string and numeric keys for looking up `EnumValue` objects by their string or numeric value. */
export type EnumMembers<K extends string> = 
  { [strValue in K]: EnumMember } 
& { [n: number]: EnumMember | undefined } 

/** Utility function for creating the required properties of `EnumType<>` from an array of `EnumValue`  */
export function getEnumMeta<K extends string>(values: EnumMember[]): {
    readonly valueLookup: EnumMembers<K>,
    readonly values: EnumMember[]
} {
    return {
        valueLookup: {
            ...values.reduce((obj, v) => Object.assign(obj, {
                [v.strValue]: v, 
                [v.value]: v
            } as EnumMembers<K>), {} as any)
        }, 
        values: values
    }
}

/** Metadata representation of an enum type */
export interface EnumType<K extends string = string> extends Metadata {
    readonly type: "enum"

    /** A lookup object with both string and numeric keys for obtaining metadata about a particular enum value. */
    readonly valueLookup: EnumMembers<K>

    /** 
     * The collection of all declared values of this enumeration.
     */
    readonly values: EnumMember[]

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
   ------ VALUE METADATA -------
   -----------------------------
*/

export interface VoidValue extends Metadata {
    readonly role: "value"
    readonly type: "void"
}
export interface ValueMeta<TType extends TypeDiscriminator> extends Metadata {
    /** 
     * Role that the value or property plays in a relational model. 
     * Some values may be nonapplicable in some contexts, like method parameters. 
     */
    readonly role: ValueRole
    readonly type: TType
}
export interface ValueMetaWithTypeDef<TType extends TypeDiscriminator, TTypeDef extends Metadata> extends ValueMeta<TType> {
    /** Full description of the type represented by this value. */
    readonly typeDef: TTypeDef
}
export interface PrimitiveValue extends ValueMeta<NativeTypeDiscriminator> { }
export interface DateValue extends ValueMeta<"date"> { }
export interface EnumValue extends ValueMetaWithTypeDef<"enum", EnumType> { }
export interface ObjectValue extends ValueMetaWithTypeDef<"object", ExternalType> { }
export interface ModelValue extends ValueMetaWithTypeDef<"model", ModelType> { }

export interface CollectionValue extends ValueMeta<"collection"> {
    readonly itemType: NonCollectionValue
}
export interface ModelCollectionValue extends CollectionValue {
    readonly itemType: ModelValue
}

export type CustomTypeValue = 
  EnumValue
| ObjectValue
| ModelValue

export type NonCollectionValue = 
  PrimitiveValue
| DateValue
| CustomTypeValue

export type Value = 
  NonCollectionValue
| CollectionValue
| ModelCollectionValue



/* -----------------------------
   ----- PROPERTY METADATA -----
   -----------------------------
*/
export interface PrimitiveProperty extends PrimitiveValue { }
export interface ForeignKeyProperty extends PrimitiveValue {
    readonly role: "foreignKey"
    readonly principalKey: PrimitiveProperty
    readonly principalType: ModelType
}
export interface DateProperty extends DateValue { }
export interface EnumProperty extends EnumValue { }
export interface ObjectProperty extends ObjectValue { }
export interface ModelProperty extends ModelValue {
    readonly role: "referenceNavigation"
    readonly foreignKey: PrimitiveProperty
    readonly principalKey: PrimitiveProperty
}
export interface BasicCollectionProperty extends CollectionValue { 
    readonly role: "value"
}
export interface ModelCollectionProperty extends ModelCollectionValue {
    readonly role: "collectionNavigation"
    readonly foreignKey: PrimitiveProperty
}

export type CollectionProperty = 
  BasicCollectionProperty
| ModelCollectionProperty

export type Property = 
  PrimitiveProperty
| ForeignKeyProperty
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
    
    /** The return type of the method. */
    readonly return: Value | VoidValue

    /** The parameters of the method */
    readonly params: { [paramName in string]: Value } 
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