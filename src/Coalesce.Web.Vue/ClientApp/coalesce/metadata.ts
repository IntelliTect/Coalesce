
type PrimitiveType = "string" | "number"  | "boolean"
type ValueType = PrimitiveType | "date" | "enum"
type ObjectType = "model" | "object"
type NonCollectionTypes = ValueType | ObjectType
export type Type = NonCollectionTypes | "collection"

export type Role = "value" | "primaryKey" | "foreignKey" | "referenceNavigation" | "collectionNavigation"

export interface IHaveMetadata {
    readonly $metadata: ModelMetadata
}

export interface Domain {
    [modelName: string]: ModelMetadata
}

export interface Metadata {
    readonly type: Type
    readonly name: string
    displayName: string
}
interface ModelMetaBase extends Metadata {
    readonly props: { [propName in string]: PropertyMetadata } 

    // The property that holds the value that represents this model.
    readonly displayProp: PropertyMetadata
}
export interface ExternalTypeMetadata extends ModelMetaBase {
    readonly type: "object"
}
export interface EntityMetadata extends ModelMetaBase {
    readonly type: "model"
    readonly methods: { [methodName in string]: MethodMetadata }
    
    // The primary key property.
    readonly keyProp: PropertyMetadata
}
export type ModelMetadata = ExternalTypeMetadata | EntityMetadata

export interface EnumValue<TEnum> {
    readonly strValue: keyof TEnum
    readonly displayName: string
    readonly value: number
}
export type EnumValues<TEnum> = {
    [strValue in keyof TEnum]: EnumValue<TEnum>;
} & { [n: number]: EnumValue<TEnum> | undefined } & object

interface PropMetaBase extends Metadata {
    readonly role: Role
}

export interface PrimitivePropertyMetadata extends PropMetaBase {
    readonly type: PrimitiveType
}
export interface DatePropertyMetadata extends PropMetaBase {
    readonly type: "date"
}

export function getEnumMeta<TEnum>(values: EnumValue<TEnum>[]): {
    readonly valueLookup: EnumValues<TEnum>,
    readonly values: EnumValue<TEnum>[]
} {
    return {
        valueLookup: {
            ...values.reduce((obj, v) => Object.assign(obj, {
                [v.strValue]: v, 
                [v.value]: v
            } as EnumValues<TEnum>), {} as any)
        }, 
        values: values
    }
}
var res = getEnumMeta([]);

export interface EnumPropertyMetadata<TEnum=any> extends PropMetaBase {
    readonly type: "enum"
    // readonly enum: TEnum // TODO: does this belong here? Not sure. See the metadata generator for more thoughts.
    readonly valueLookup: EnumValues<TEnum>
    readonly values: EnumValue<TEnum>[]
}
export interface ObjectPropertyMetadata extends PropMetaBase {
    readonly type: "object"
    readonly model: ExternalTypeMetadata
}
export interface ModelPropertyMetadata extends PropMetaBase {
    readonly type: "model"
    readonly model: ModelMetadata
    readonly keyProp: PropertyMetadata
}
export interface CollectionPropertyMetadata extends PropMetaBase {
    readonly type: "collection"
    readonly collectionType?: NonCollectionTypes
    readonly model?: ModelMetadata
    readonly keyProp?: PropertyMetadata
}

export type PropertyMetadata = 
  PrimitivePropertyMetadata
| DatePropertyMetadata
| EnumPropertyMetadata
| ObjectPropertyMetadata
| ModelPropertyMetadata
| CollectionPropertyMetadata

export interface MethodMetadata extends Metadata  {
    readonly params: PropertyMetadata[]
}

