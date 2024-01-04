import {
  Model,
  Property,
  Value,
  Method,
  EnumType,
  ClassType,
  ModelType,
  ObjectType,
  DataSourceType,
  ViewModel,
  Domain,
  AnyArgCaller,
  ApiState,
  DataSource,
  PropNames,
  KeysOfType,
  MethodParameter,
} from "coalesce-vue";
import { computed, PropType, useAttrs } from "vue";
import { useMetadata } from "..";
import { ApiStateTypeWithArgs } from "coalesce-vue";

type PropsOf<TModel> = TModel extends {
  $metadata: {
    props: infer O extends Record<string, Property>;
  };
}
  ? O
  : never;

// prettier-ignore
export type ForSpec<
  TModel extends ModelAllowedType | unknown = unknown,
  ValueKind extends Value = Property
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
      
  // Handle binding of `:model` to an API caller's args object:    
  : TModel extends ApiStateTypeWithArgs<
    infer TMethod extends Method,
    any,
    infer TArgsObj,
    any
  > ?
    // NOTE: Pulling types off of TArgsObj is a concession we make
    // due to ApiStateTypeWithArgs's constituent types not actually capturing
    // the type of their metadata. At some point this could be made better if
    // we were able to pull metadata off of `TMethod.
    // What we'd really like to do here is this:
    // | Extract<TMethod["params"], ValueKind>
    // | KeysOfType<TMethod["params"], ValueKind>
    // TODO: THIS IS WRONG - HARDCODED AGAINST Model AND IGNORING ValueKind
    KeysOfType<TArgsObj, Model | null> | MethodParameter
    
  // Fallback to allowing anything:
  : undefined | null | string | Property | Value | Method;

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

export function makeMetadataProps<TModel extends ModelAllowedType = Model>() {
  return {
    /** An object owning the value that is specified by the `for` prop. */
    model: {
      type: [Object, Function] as PropType<TModel | null>,
      default: null,
    },

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"firstName"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
     * * A string in dot-notation that starts with a type name. E.g. `"Person.firstName"`.
     */
    for: {
      required: false,
      type: [String, Object] as PropType<ForSpec<TModel>>,
      default: null,
    },
  };
}

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

  const inputBindAttrs = computed(() =>
    buildVuetifyAttrs(valueMeta.value, props.model, useAttrs())
  );

  return { modelMeta, valueMeta, valueOwner, inputBindAttrs };
}
