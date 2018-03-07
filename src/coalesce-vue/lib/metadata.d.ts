export declare type PrimitiveType = "string" | "number" | "boolean";
export declare type ValueType = PrimitiveType | "date" | "enum";
export declare type SimpleType = PrimitiveType | "date";
export declare type ObjectType = "model" | "object";
export declare type NonCollectionType = ValueType | ObjectType;
export declare type TypeDiscriminator = NonCollectionType | "collection";
export declare type Role = "value" | "primaryKey" | "foreignKey" | "referenceNavigation" | "collectionNavigation";
export interface IHaveMetadata {
    readonly $metadata: ClassType;
}
export interface Domain {
    types: {
        [modelName: string]: ClassType;
    };
    enums: {
        [modelName: string]: EnumType;
    };
}
export interface Metadata {
    readonly type: TypeDiscriminator;
    readonly name: string;
    displayName: string;
}
export interface CustomReferenceTypeBase extends Metadata {
    readonly props: {
        [propName in string]: Property;
    };
    readonly displayProp?: Property;
}
export interface ExternalType extends CustomReferenceTypeBase {
    readonly type: "object";
}
export interface ModelType extends CustomReferenceTypeBase {
    readonly type: "model";
    readonly methods: {
        [methodName in string]: Method;
    };
    readonly displayProp: Property;
    readonly keyProp: Property;
    readonly controllerRoute: string;
}
export interface EnumValue {
    readonly strValue: string;
    readonly displayName: string;
    readonly value: number;
}
export declare type EnumValues<K extends string> = {
    [strValue in K]: EnumValue;
} & {
    [n: number]: EnumValue | undefined;
};
export declare function getEnumMeta<K extends string>(values: EnumValue[]): {
    readonly valueLookup: EnumValues<K>;
    readonly values: EnumValue[];
};
export interface EnumType<K extends string = string> extends Metadata {
    readonly type: "enum";
    readonly valueLookup: EnumValues<K>;
    readonly values: EnumValue[];
}
export declare type ClassType = ExternalType | ModelType;
export declare type CustomType = ClassType | EnumType;
export declare type CollectableType = CustomType | SimpleType;
export interface PropMetaBase extends Metadata {
    readonly role: Role;
}
export interface PrimitiveProperty extends PropMetaBase {
    readonly type: PrimitiveType;
}
export interface DateProperty extends PropMetaBase {
    readonly type: "date";
}
export interface EnumProperty extends PropMetaBase {
    readonly type: "enum";
    readonly typeDef: EnumType;
}
export interface ObjectProperty extends PropMetaBase {
    readonly type: "object";
    readonly typeDef: ExternalType;
}
export interface ModelProperty extends PropMetaBase {
    readonly type: "model";
    readonly typeDef: ModelType;
    readonly foreignKey: PrimitiveProperty;
    readonly principalKey: PrimitiveProperty;
}
export interface CollectionProperty extends PropMetaBase {
    readonly type: "collection";
    readonly typeDef: CollectableType;
    readonly foreignKey?: PrimitiveProperty;
}
export declare type Property = PrimitiveProperty | DateProperty | EnumProperty | ObjectProperty | ModelProperty | CollectionProperty;
export interface Method extends Metadata {
    readonly params: Property[];
}
export declare type PropNames<TMeta extends ClassType, Kind extends Property = Property> = {
    [K in keyof TMeta["props"]]: TMeta["props"][K] extends Kind ? K : never;
}[keyof TMeta["props"]];
export declare type PropertyOrName<TMeta extends ClassType> = Property | PropNames<TMeta, Property>;
export declare function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string): Exclude<TProp, string>;
export declare function resolvePropMeta<TProp extends Property>(metadata: ClassType, propOrString: TProp | string, slient: true): Exclude<TProp, string> | undefined;
export declare function isClassType(prop: CollectableType): prop is ClassType;
