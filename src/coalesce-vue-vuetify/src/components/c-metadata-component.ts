
import { Vue, Component, Prop } from 'vue-property-decorator';
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
  AnyArgCaller
} from 'coalesce-vue';

export function getValueMeta(
  forVal: undefined | null | string | Property | Value | Method, 
  modelMeta: ObjectType | ModelType | DataSourceType | Method | null | undefined,
  $metadata?: Domain
): Property | Value | Method | null {

  if (!forVal) {
    return null;
  }

  if (typeof forVal != "string") {
    return forVal;
  }

  if (forVal.length === 0) {
    throw `prop 'for' must not be an empty string`
  }

  if (modelMeta && "props" in modelMeta) {
    // Handle the 90% case: check if 'for' is a prop on 'model'
    const matchedProp = modelMeta.props[forVal];
    if (matchedProp) {
      return matchedProp;
    }
  }

  const forParts = forVal.split('.');

  let tail: ClassType | Method | Property | Value | undefined = undefined;
  let tailKind: "type" | "method" | "property" | "value" | undefined = undefined;

  if (modelMeta) {
    if ("params" in modelMeta) {
      tail = modelMeta;
      tailKind = "method";
    } else if (modelMeta.type == "object" || modelMeta.type == "model" || modelMeta.type == "dataSource") {
      tail = modelMeta
      tailKind = "type"
    }
  }

  for (let i = 0; i < forParts.length; i++) {
    const forPart = forParts[i];
    const forPartNext = forParts[i+1];

    // Check if 'for' is a type name. Type name is only valid in the first position.
    if (i == 0 && $metadata) {
      if (forPart in $metadata.types){
        tail = ($metadata.types as any)[forPart];
        tailKind = "type";
        continue;
      }
      if (forPart in $metadata.enums){
        const type: EnumType = ($metadata.enums as any)[forPart];
        tail = <Value>{
          type: type.type,
          displayName: type.displayName,
          name: '',
          role: "value",
          typeDef: type
        }
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
      
      if (forPart == "methods" && type.type == "model" && type.methods[forPartNext]) {
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

    throw Error(`Could not resolve token '${forPart}'${forVal != forPart ? ` in '${forVal}'` : ''} from ${tailKind} '${tail?.name}'`)
  }

  if (!tail) {
    throw Error(`Could not find any metadata with for specifier '${forVal}'`)
  }

  switch (tailKind) {
    case "method":
      return tail as Method
    case "type":
      const type = tail as ClassType;
      switch (type.type){
        // Create a fake `Value` implementation to represent a usage of the type.
        case "model":
          return {
            type: type.type,
            displayName: type.displayName,
            name: '',
            role: "referenceNavigation",
            typeDef: type
          }
        case "object":
          return {
            type: type.type,
            displayName: type.displayName,
            name: '',
            role: "value",
            typeDef: type
          }
      }
    default:
      return tail as Property | Value;
  }
}


export function buildVuetifyAttrs(
  valueMeta: Property | Value | null, 
  model: Model<ClassType> | AnyArgCaller | null,
  attrs?: {},
  injections?: {[s: string]: any},
): {[s: string]: any} {

  if (!valueMeta) {
    return {
      ...injections?.['c-input-props'],
      ...attrs,
    }
  }

  const modelMeta = model ? model.$metadata : null;

  return {
    // If a label is not provided to the component, default to the displayName of the value. 
    label: valueMeta?.displayName,
    hint: valueMeta?.description,

    // Normalize multi-word name based on what might exist in `attrs`
    // (so that it can be overridden using either casing style).
    // Use kebab style if the camel version isn't detected.
    [attrs && 'persistentHint' in attrs 
      ? 'persistentHint' 
      : 'persistent-hint']: !!valueMeta?.description,

    rules: model && model instanceof ViewModel && valueMeta.name in (modelMeta as ModelType)!.props
      ? model.$getRules(valueMeta.name)
      : 'rules' in valueMeta && valueMeta.rules
        ? Object.values(valueMeta.rules)
        : undefined,

    ...injections?.['c-input-props'],
    ...attrs,
  }
}

@Component({
  inject: {
    'c-input-props': { default: {} }
  },
})
export default class extends Vue {

  // NOTE: these props are intentionally don't have types specified. 
  // Vue's type checking is slow because of some nonsense about 
  // compatibility with iframes - it tostrings the ctor function and then 
  // runs a regex against the string to determine the type.

  /**
   * A 'parent' object that owns the property which we are binding.
   * The corresponding property is specified via prop `for`,
   * and should be accessed via `valueMeta`
   */
  @Prop({required: false, default: null, /*type: Object*/})
  public model!: Model<ClassType> | null;

  @Prop({required: false, default: null, /*type: [String, Object]*/})
  public for?: string | Property | Value | null;

  get inputBindAttrs() {
    return buildVuetifyAttrs(this.valueMeta, this.model, this.$attrs, this);
  }

  get modelMeta(): ClassType | null {
    return this.model ? this.model.$metadata : null;
  }

  get valueMeta(): Property | Value | null {
    const valueMeta = getValueMeta(this.for, this.modelMeta, this.$coalesce.metadata);
    if (valueMeta && "role" in valueMeta) {
      return valueMeta;
    }
    return null;
  }
}