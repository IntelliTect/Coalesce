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
} from "coalesce-vue";
import {
  computed,
  PropType,
  useAttrs,
  ExtractPropTypes,
  inject,
  getCurrentInstance,
} from "vue";

export type ForSpec = undefined | null | string | Property | Value | Method;

export function getValueMeta(
  forVal: ForSpec,
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
    default:
      return tail as Property | Value;
  }
}

export function buildVuetifyAttrs(
  valueMeta: Property | Value | null,
  model: Model<ClassType> | AnyArgCaller | null | undefined,
  attrs?: {}
): { [s: string]: any } {
  if (!valueMeta) {
    return {
      ...attrs,
    };
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

export function makeMetadataProps() {
  return {
    for: {
      required: false,
      type: [String, Object] as PropType<ForSpec>,
      default: null,
    },
    model: { type: Object as PropType<Model<ClassType>>, default: null },
  };
}

export function useMetadataProps(
  props: ExtractPropTypes<ReturnType<typeof makeMetadataProps>>
) {
  const modelMeta = computed(() => {
    return props.model ? props.model.$metadata : null;
  });

  const valueMeta = computed((() => {
    const valueMeta = getValueMeta(
      props.for,
      modelMeta.value,
      getCurrentInstance()!.proxy.$coalesce.metadata
    );
    if (valueMeta && "role" in valueMeta) {
      return valueMeta;
    }
    return null;
  }) as () => Property | Value | null);

  const inputBindAttrs = computed(() =>
    buildVuetifyAttrs(valueMeta.value, props.model, {
      ...useAttrs(),
      ...inject("c-input-props", {}),
    })
  );

  return { modelMeta, valueMeta, inputBindAttrs };
}
