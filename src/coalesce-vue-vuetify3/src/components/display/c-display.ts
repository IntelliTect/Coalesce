import { defineComponent, Prop, h as _c, PropType, mergeProps } from "vue";
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

const passwordWrapper = defineComponent({
  name: "c-password-display",
  props: {
    element: { type: String, default: "span" },
    value: { type: String, default: "" },
  },
  data() {
    return {
      shown: false,
    };
  },
  render() {
    return _c(
      this.element,
      mergeProps(this.$attrs, {
        class: this.shown
          ? "c-password-display c-password-display--shown"
          : "c-password-display c-password-display--hidden",
      }),
      [
        _c("i", {
          class: [!this.shown ? "fa fa-eye" : "fa fa-eye-slash", "pr-1"],
          role: "button",
          pressed: this.shown,
          title: !this.shown ? "Reveal" : "Hide",
          onClick: () => (this.shown = !this.shown),
        }),
        !this.shown ? "•".repeat(8) : this.value,
      ]
    );
  },
});

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

    if (meta.type === "string" && valueString) {
      switch (meta.subtype) {
        case "password":
          return _c(passwordWrapper, {
            ...this.$attrs,
            value: valueString,
          });
        case "multiline":
          return _c(
            props.element,
            {
              ...this.$attrs,
              style: "white-space: pre-wrap",
            },
            valueString || this.$slots
          );
        case "url-image":
          return _c("img", {
            ...this.$attrs,
            src: valueString,
            title: valueString,
          });
        case "color":
          return _c(
            props.element,
            {
              ...this.$attrs,
              style: "white-space: nowrap",
            },
            [
              _c("span", {
                style: `background-color: ${valueString.replace(
                  /[^A-Fa-f0-9#]/g,
                  ""
                )};`,
                class: "c-display--color-swatch",
              }),
              valueString || this.$slots,
            ] as any
          );
        case "url":
        case "email":
        case "tel":
          let href;
          if (meta.subtype == "url") {
            href = valueString;
          } else if (meta.subtype == "email") {
            href = "mailto:" + valueString;
          } else if (meta.subtype == "tel") {
            href = "tel:" + valueString;
          }
          if (href) {
            try {
              new URL(valueString, window.location.origin);
              return _c(
                "a",
                {
                  ...this.$attrs,
                  href,
                },
                valueString
              );
            } catch {}
          }
      }
    } else if (meta.type === "boolean") {
      if (valueString === "true") {
        valueString = "✓";
      } else if (valueString === "false") {
        valueString = "✗";
      }
    }

    return _c(props.element, this.$attrs, valueString || this.$slots);
  },
});
