import Vue, { defineComponent, Prop, h as _c, PropType } from "vue";
import { ForSpec, getValueMeta } from "../c-metadata-component";
import {
  propDisplay,
  valueDisplay,
  Property,
  DisplayOptions,
  Model,
  ClassType,
  DateValue,
} from "coalesce-vue";

const standaloneDateValueMeta = <DateValue>{
  name: "",
  displayName: "",
  type: "date",
  dateKind: "datetime",
};

export const cDisplayProps = {
  element: { type: String, default: "span" },
  for: { required: false, type: [String, Object] as PropType<ForSpec> },
  model: { type: Object as PropType<Model<ClassType>> },

  options: {
    required: false,
    type: Object as PropType<DisplayOptions>,
    default: null,
  },
  // Shorthand for { options: format }
  format: {
    required: false,
    type: [String, Object] as PropType<DisplayOptions["format"]>,
  },

  modelValue: <Prop<any>>{ required: false },
  // `value` is backwards compat with vue2 style:
  value: <Prop<any>>{ required: false },
};

export default defineComponent({
  name: "c-display",

  props: cDisplayProps,

  render() {
    // NOTE: CreateElement fn must be named `_c` for unplugin-vue-components to work correctly.

    const props = this.$props;
    const model = props.model;
    const valueProp = props.modelValue ?? props.value;

    if (model == null && valueProp == null) {
      // If no model and no value were provided, just display nothing.
      // This isn't an error case - it just means the thing we're trying to display
      // is `null`-ish, and should be treated the same way that vue would treat {{null}}
      return _c(props.element);
    }

    const modelMeta = model ? model.$metadata : null;
    let meta = getValueMeta(props.for, modelMeta, this.$coalesce.metadata);
    if (!meta && modelMeta && "displayProp" in modelMeta) {
      meta = modelMeta.displayProp || null;
    }

    if (!meta && valueProp instanceof Date) {
      // Allow direct formatting of dates with <c-display :value="date" />
      meta = standaloneDateValueMeta;
    }

    if (!meta) {
      throw Error(
        "Provided model has no $metadata property, and no specific value was provided via the 'for' component prop to c-display."
      );
    }

    if ("params" in meta) {
      throw Error("Cannot display a method");
    }

    let options = props.options;
    if (props.format) {
      options = { ...options, format: props.format as any };
    }

    let valueString =
      model && "role" in meta
        ? propDisplay(model, meta as Property, options)
        : valueDisplay(valueProp, meta, options);

    if (meta.type === "boolean") {
      if (valueString === "true") {
        valueString = "✓";
      } else if (valueString === "false") {
        valueString = "✗";
      }
    }

    return _c(props.element, this.$attrs, valueString || this.$slots);
  },
});
