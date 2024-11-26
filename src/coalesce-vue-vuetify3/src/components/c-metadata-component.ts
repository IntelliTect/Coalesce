import type {
  Model,
  Property,
  Value,
  Method,
  EnumType,
  ClassType,
  ModelType,
  ObjectType,
  DataSourceType,
  Domain,
  AnyArgCaller,
  DataSource,
  PropNames,
  ModelValue,
  DateValue,
  StringValue,
  NumberValue,
  FileValue,
  BooleanValue,
  CollectionValue,
  ObjectValue,
  ApiStateTypeWithArgs,
  ListViewModel,
  ServiceViewModel,
  ModelCollectionValue,
} from "coalesce-vue";
import { ApiState, ViewModel } from "coalesce-vue";
import { computed, inject, useAttrs } from "vue";
import { useMetadata } from "../composables/useMetadata";

type PropsOf<TModel> = TModel extends {
  $metadata: {
    props: infer O extends Record<string, Property>;
  };
}
  ? O
  : never;

type MethodsOf<TModel> = TModel extends {
  $metadata: {
    methods: infer O extends Record<string, Method>;
  };
}
  ? O
  : never;

// prettier-ignore
export type ForSpec<
  TModel extends ModelAllowedType | unknown = unknown,
  ValueKind extends Value = Value
> = 
// Handle binding of `:model` to a Model or ViewModel:
TModel extends Model ? 
  // Check if we only know that the type's prop names are any strings
  "__neverPropName" extends PropNames<TModel["$metadata"]>
    // If so, we have to allow any string because the exact prop names aren't known.
    ? string | ValueKind
    // We know the exact prop names of the type, so restrict to just those:
    : {
        [K in keyof PropsOf<TModel>]: PropsOf<TModel>[K] extends ValueKind
        ? // Allow the property name
          | K
          // Or the full property metadata object
          | PropsOf<TModel>[K]
        : never;
      }[keyof PropsOf<TModel>]
    
// Handle binding of `:model` to an API caller (which binds values to the caller's args object):    
: TModel extends ApiStateTypeWithArgs<
  infer TMethod extends Method,
  any,
  infer TArgsObj,
  any
> ?
  // HACK: Pulling types off of TArgsObj is a concession we make
  // due to ApiStateTypeWithArgs's constituent types not actually capturing
  // the type of their metadata. At some point this could be made better if
  // we were able to pull metadata off of `TMethod. In other words, what we'd 
  // really like to do here is do the same thing we do for props on a model.
  | {
      [K in keyof TArgsObj]: TArgsObj[K] extends ((
        ValueKind extends ModelValue ? Model<ModelType> :
        ValueKind extends ObjectValue ? Model<ClassType> :
        ValueKind extends DateValue ? Date :
        // Narrowed to role:value so we don't produce these if only searching for a FK.
        ValueKind extends (StringValue & {role: 'value'}) ? string :
        ValueKind extends (NumberValue & {role: 'value'}) ? number :
        ValueKind extends FileValue ? File :
        ValueKind extends BooleanValue ? boolean :
        ValueKind extends ModelCollectionValue ? (Model<ModelType>[]) :
        ValueKind extends CollectionValue ? (string[] | number[]) :
        ValueKind extends Property ? never :
        ValueKind extends Value ? any :
        never
      ) | null)
      ? K
      : never;
    }[keyof TArgsObj]
  | ((
      // Remove from the ValueKind union any property types,
      // since method parameters aren't properties. 
      // This helps to create cleaner intellisense.
      ValueKind extends Property ? never :
      ValueKind
    )
      // While intersecting this with MethodParameter is *technically* correct,
      // it creates noisy intellisense and we don't actually care if the meta
      // has the few extra fields specific to method parameters,
      // as our input components don't use those parameter-specific fields.
      // & MethodParameter
    )
  
// Fallback to allowing anything:
: undefined | string | ValueKind;

export type MethodForSpec<
  TModel extends
    | Model
    | ServiceViewModel
    | ListViewModel
    | ViewModel
    | unknown = unknown,
  MethodKind extends Method = Method
> = "__never" extends keyof MethodsOf<TModel> // Check if we only know that the type's method names are any strings
  ? // If so, we have to allow any string because the exact method names aren't known.
    string | Method
  : // We know the exact method names of the type, so restrict to just those:
    {
      [K in keyof MethodsOf<TModel>]: MethodsOf<TModel>[K] extends MethodKind
        ? // Allow the method name
          | (K & string)
            // Or the full method metadata object
            | MethodsOf<TModel>[K]
        : never;
    }[keyof MethodsOf<TModel>];

export function getValueMetaAndOwner(
  forVal: ForSpec | null | undefined,
  model: Model | DataSource | AnyArgCaller | null | undefined,
  $metadata?: Domain
) {
  const valueMeta = getValueMeta(forVal, model?.$metadata, $metadata);

  // Support binding to method args via `:model="myModel.myMethod" for="myArg"`.
  // getValueMeta will resolve to the metadata of the specific parameter;
  // we then have to resolve the args object from the ApiState.
  if (
    model instanceof ApiState &&
    "args" in model &&
    valueMeta &&
    valueMeta.name in model.args
  ) {
    model = model.args;
  }

  return { valueMeta, valueOwner: model as any };
}

export function getValueMeta(
  forVal: ForSpec | null | undefined,
  modelMeta:
    | ObjectType
    | ModelType
    | DataSourceType
    | Method
    | null
    | undefined,
  $metadata?: Domain
): Property | Value | Method | null {
  if (!forVal) {
    return null;
  }

  if (typeof forVal != "string") {
    return forVal;
  }

  if (forVal.length === 0) {
    throw `prop 'for' must not be an empty string`;
  }

  if (modelMeta && "props" in modelMeta) {
    // Handle the 90% case: check if 'for' is a prop on 'model'
    const matchedProp = modelMeta.props[forVal];
    if (matchedProp) {
      return matchedProp;
    }
  }

  const forParts = forVal.split(".");

  let tail: ClassType | Method | Property | Value | undefined = undefined;
  let tailKind: "type" | "method" | "property" | "value" | undefined =
    undefined;

  if (modelMeta) {
    if ("params" in modelMeta) {
      tail = modelMeta;
      tailKind = "method";
    } else if (
      modelMeta.type == "object" ||
      modelMeta.type == "model" ||
      modelMeta.type == "dataSource"
    ) {
      tail = modelMeta;
      tailKind = "type";
    }
  }

  $metadata ??= useMetadata();

  for (let i = 0; i < forParts.length; i++) {
    const forPart = forParts[i];
    const forPartNext = forParts[i + 1];

    // Check if 'for' is a type name. Type name is only valid in the first position.
    if (i == 0 && $metadata) {
      if (forPart in $metadata.types) {
        tail = ($metadata.types as any)[forPart];
        tailKind = "type";
        continue;
      }
      if (forPart in $metadata.enums) {
        const type: EnumType = ($metadata.enums as any)[forPart];
        tail = <Value>{
          type: type.type,
          displayName: type.displayName,
          name: "",
          role: "value",
          typeDef: type,
        };
        tailKind = "value";
        continue;
      }
    }

    if (tailKind == "type") {
      // See if the part is a prop name.
      const type = tail as ClassType;
      if (type.props[forPart]) {
        tail = type.props[forPart];
        tailKind = "property";
        continue;
      }

      // See if the part is a method name.
      if (type.type == "model" && type.methods[forPart]) {
        tail = type.methods[forPart];
        tailKind = "method";
        continue;
      }

      // forPart wasn't itself a method or prop.
      // Check if forPart is the literal string "props" or "method"
      // and the actual name is the following token.
      if (forPart == "props" && type.props[forPartNext]) {
        i++;
        tail = type.props[forPartNext];
        tailKind = "property";
        continue;
      }

      if (
        forPart == "methods" &&
        type.type == "model" &&
        type.methods[forPartNext]
      ) {
        i++;
        tail = type.methods[forPartNext];
        tailKind = "method";
        continue;
      }
    } else if (tailKind == "method") {
      const method = tail as Method;
      if (method.params[forPart]) {
        tail = method.params[forPart];
        tailKind = "value";
        continue;
      }

      // Check if forPart is the literal string "params"
      // and the actual name is the following token.
      if (forPart == "params" && method.params[forPartNext]) {
        i++;
        tail = method.params[forPartNext];
        tailKind = "value";
        continue;
      }
    }

    throw Error(
      `Could not resolve token '${forPart}'${
        forVal != forPart ? " in " + forVal : ""
      } from ${tailKind} '${tail?.name}'`
    );
  }

  if (!tail) {
    throw Error(`Could not find any metadata with for specifier '${forVal}'`);
  }

  switch (tailKind) {
    case "method":
      return tail as Method;
    case "type":
      const type = tail as ClassType;
      switch (type.type) {
        // Create a fake `Value` implementation to represent a usage of the type.
        case "model":
          return {
            type: type.type,
            displayName: type.displayName,
            name: "",
            role: "referenceNavigation",
            typeDef: type,
          };
        case "object":
          return {
            type: type.type,
            displayName: type.displayName,
            name: "",
            role: "value",
            typeDef: type,
          };
      }
  }
  return tail as Property | Value;
}

export function buildVuetifyAttrs(
  valueMeta: Property | Value | null,
  model: Model<ClassType> | AnyArgCaller | null | undefined,
  attrs?: { [k: string]: any }
): { [s: string]: any } {
  if (attrs) {
    attrs = { ...attrs };
    // Do not pass update:modelValue down to child components.
    // Components will always handle emitting this event themselves.
    delete attrs["onUpdate:modelValue"];
  }

  if (!valueMeta) {
    return { ...attrs };
  }

  const modelMeta = model ? model.$metadata : null;

  return {
    // If a label is not provided to the component, default to the displayName of the value.
    label: valueMeta?.displayName,
    hint: valueMeta?.description,

    // Normalize multi-word name based on what might exist in `attrs`
    // (so that it can be overridden using either casing style).
    // Use kebab style if the camel version isn't detected.
    [attrs && "persistentHint" in attrs ? "persistentHint" : "persistent-hint"]:
      !!valueMeta?.description,

    rules:
      // We're bound to a ViewModel instance, and the value is a prop on that viewmodel. Ask the ViewModel for the rules.
      model &&
      model instanceof ViewModel &&
      valueMeta.name in (modelMeta as ModelType)!.props
        ? model.$getRules(valueMeta.name)
        : // Grab the rules from the metadata for the bound value
        "rules" in valueMeta && valueMeta.rules
        ? Object.values(valueMeta.rules)
        : undefined,

    ...attrs,
  };
}

type ModelAllowedType = Model | AnyArgCaller;

export function useMetadataProps<TModel extends ModelAllowedType = Model>(
  props: {
    model: TModel | null | undefined;
    for: ForSpec | null | undefined;
  },
  transformValueMeta?: (meta: Value | Property) => Value | Property
) {
  const metadata = useMetadata();

  const modelMeta = computed(() => {
    return props.model
      ? (props.model.$metadata as any as TModel["$metadata"])
      : null;
  });

  const valueMeta = computed((() => {
    const valueMeta = getValueMeta(props.for, modelMeta.value, metadata);
    if (valueMeta && "role" in valueMeta) {
      return transformValueMeta?.(valueMeta) ?? valueMeta;
    }
    return null;
  }) as () => Property | Value | null);

  /** The object that owns the value described by `valueMeta`. */
  const valueOwner = computed(() => {
    return getValueMetaAndOwner(props.for, props.model, metadata).valueOwner;
  });

  const attrs = useAttrs();
  const inputBindAttrs = computed(() =>
    buildVuetifyAttrs(valueMeta.value, props.model, attrs)
  );

  return { modelMeta, valueMeta, valueOwner, inputBindAttrs };
}

export function useCustomInput(props: {
  readonly?: boolean | null;
  disabled?: boolean | null;
}) {
  const form: any = inject(Symbol.for("vuetify:form"), null);

  const isDisabled = computed(
    (): boolean => props.disabled || form?.isDisabled.value
  );

  const isReadonly = computed(
    (): boolean => props.readonly || form?.isReadonly.value
  );

  const isInteractive = computed((): boolean => {
    return !isDisabled.value && !isReadonly.value;
  });
  return { form, isDisabled, isReadonly, isInteractive };
}
