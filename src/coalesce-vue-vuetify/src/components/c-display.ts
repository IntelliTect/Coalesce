import Vue, { PropOptions } from "vue";
import { getValueMeta } from "./c-metadata-component";
import { propDisplay, valueDisplay, Property, DisplayOptions, Model, ClassType } from "coalesce-vue";

export default Vue.extend({
  name: "c-display",
  functional: true,
  props: {
    element: { type: String, default: "span" },
    for: <PropOptions<any>>{ required: false },
    model: <PropOptions<Model<ClassType>>>{ type: Object },

    options: <PropOptions<DisplayOptions>>{ required: false, type: Object, default: null },
    // Shorthand for { options: format }
    format: { required: false },

    value: <PropOptions<any>>{ required: false }
  },

	render(h, ctx) {
    const props = ctx.props;
    const { model, value: valueProp } = props;

    if (model == null && valueProp == null) {
      // If no model and no value were provided, just display nothing.
      // This isn't an error case - it just means the thing we're tring to display 
      // is `null`-ish, and should be treated the same way that vue would treat {{null}}
      return h('span');
    }

    const modelMeta = model ? model.$metadata : null;
    let meta = getValueMeta(props.for, modelMeta, ctx.parent.$coalesce.metadata);
    if (!meta && modelMeta && "displayProp" in modelMeta) {
      meta = modelMeta.displayProp || null;
    }

    if (!meta) {
      throw "Provided model has no $metadata property, and no specific value was provided via the 'for' component prop to c-display.";
    }

    if ("params" in meta) {
      throw Error("Cannot display a method");
    }

    let options: null | DisplayOptions = props.options;
    if (props.format) {
      options = {...options, format: props.format as any};
    }

    let value = model && "role" in meta
      ? propDisplay(model, meta as Property, options)
      : valueDisplay(valueProp, meta, options);

    if (meta.type === "boolean") {
      if (value === "true") {
        value = "✓";
      } else if (value === "false") {
        value = "✗";
      }
    }

    return h(props.element, ctx.data, value || ctx.children);
  }
});
