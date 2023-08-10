import { defineComponent, PropOptions } from "vue";
import { getValueMeta, makeMetadataProps } from "../c-metadata-component";
import {
  propDisplay,
  valueDisplay,
  Property,
  DisplayOptions,
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
  render(_c) {
    return _c(
      this.element,
      {
        attrs: { ...this.$attrs },
        on: { ...this.$listeners },
        staticClass: this.shown
          ? "c-password-display c-password-display--shown"
          : "c-password-display c-password-display--hidden",
      },
      [
        _c("i", {
          class: !this.shown ? "fa fa-eye" : "fa fa-eye-slash",
          staticClass: "pr-1",
          attrs: {
            role: "button",
            pressed: this.shown,
            title: !this.shown ? "Reveal" : "Hide",
          },
          on: {
            click: () => (this.shown = !this.shown),
          },
        }),
        !this.shown ? "•".repeat(8) : this.value,
      ]
    );
  },
});

export default defineComponent({
  // name: "c-display",
  functional: true,
  props: {
    element: { type: String, default: "span" },

    ...makeMetadataProps(),

    options: <PropOptions<DisplayOptions>>{
      required: false,
      type: Object,
      default: null,
    },
    // Shorthand for { options: format }
    format: { required: false },

    value: <PropOptions<any>>{ required: false },
  },

  render(_c, ctx) {
    // NOTE: CreateElement fn must be named `_c` for unplugin-vue-components to work correctly.

    const props: typeof ctx.props = ctx.props;
    const { model, value: valueProp } = props;

    if (model == null && valueProp == null) {
      // If no model and no value were provided, just display nothing.
      // This isn't an error case - it just means the thing we're trying to display
      // is `null`-ish, and should be treated the same way that vue would treat {{null}}
      return _c(props.element);
    }

    const modelMeta = model ? model.$metadata : null;
    let meta = getValueMeta(
      props.for,
      modelMeta,
      ctx.parent.$coalesce.metadata
    );
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

    let options: null | undefined | DisplayOptions = props.options;
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
            ...ctx.data,
            attrs: { ...ctx.data.attrs, value: valueString },
          });
        case "multiline":
          return _c(
            props.element,
            {
              ...ctx.data,
              style: "white-space: pre-wrap",
            },
            valueString || ctx.children
          );
        case "url-image":
          return _c("img", {
            ...ctx.data,
            attrs: { ...ctx.data.attrs, src: valueString },
          });
        case "color":
          return _c(
            props.element,
            {
              ...ctx.data,
              attrs: {
                ...ctx.data.attrs,
                style: "white-space: nowrap",
              },
            },
            [
              _c("span", {
                attrs: {
                  style: `background-color: ${valueString.replace(
                    /[^A-Fa-f0-9#]/g,
                    ""
                  )};`,
                  class: "c-display--color-swatch",
                },
              }),
              valueString || ctx.children,
            ]
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
                  ...ctx.data,
                  attrs: { ...ctx.data.attrs, href },
                },
                valueString
              );
            } catch {
              /* value is not a valid url */
            }
          }
      }
    } else if (meta.type === "boolean") {
      if (valueString === "true") {
        valueString = "✓";
      } else if (valueString === "false") {
        valueString = "✗";
      }
    }

    return _c(props.element, ctx.data, valueString || ctx.children);
  },
});
