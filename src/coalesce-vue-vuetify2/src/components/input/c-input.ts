import { defineComponent, type PropOptions, type VNodeData } from "vue";
import { buildVuetifyAttrs, getValueMeta } from "../c-metadata-component";
import {
  Model,
  ClassType,
  DataSource,
  DataSourceType,
  mapValueToModel,
  AnyArgCaller,
  ApiState,
  parseValue,
} from "coalesce-vue";

const primitiveTypes = ["string", "number", "date", "enum", "boolean"];

const passwordWrapper = defineComponent({
  name: "c-password-input",
  data() {
    return {
      shown: false,
    };
  },
  render(_c) {
    const data = {
      attrs: { ...this.$attrs },
      on: { ...this.$listeners },
      scopedSlots: this.$scopedSlots,
    } as VNodeData;
    data.attrs!["append-icon"] ??= !this.shown
      ? "fa fa-eye"
      : "fa fa-eye-slash";
    data.attrs!.type = this.shown ? "text" : "password";
    addHandler(data.on!, "click:append", () => (this.shown = !this.shown));
    return _c("v-text-field", data);
  },
});

export default defineComponent({
  functional: true,

  props: {
    for: <PropOptions<any>>{ required: false },
    model: <
      PropOptions<Model<ClassType> | DataSource<DataSourceType> | AnyArgCaller>
    >{ required: false },
    value: <PropOptions<any>>{ required: false },
  },

  render(_c, ctx) {
    // NOTE: CreateElement fn must be named `_c` for unplugin-vue-components to work correctly.

    let model = ctx.props.model;
    const modelMeta = model ? model.$metadata : null;

    let _valueMeta = getValueMeta(
      ctx.props.for,
      modelMeta,
      ctx.parent.$coalesce.metadata
    );
    if (!_valueMeta || !("role" in _valueMeta)) {
      throw Error(
        "c-input requires value metadata. Specify it with the 'for' prop'"
      );
    }
    const valueMeta = _valueMeta; // Alias so type inside closures will be correct;

    // Support binding to method args via `:model="myModel.myMethod" for="myArg"`.
    // getValueMeta will resolve to the metadata of the specific parameter;
    // we then have to resolve the args object from the ApiState.
    if (
      model instanceof ApiState &&
      "args" in model &&
      valueMeta.name in model.args
    ) {
      model = model.args;
    }

    const { on: ctxOn, props: ctxProps, ...ctxData } = ctx.data;
    let data = {
      ...ctxData,
      attrs: undefined as typeof ctxData.attrs,

      props: {
        // If a model is provided, pull the value off the model.
        value: model
          ? (model as any)[valueMeta.name]
          : primitiveTypes.includes(valueMeta.type)
          ? mapValueToModel(ctx.props.value, valueMeta)
          : ctx.props.value,
      } as Exclude<typeof ctxProps, undefined>,

      on: {
        ...ctxOn,
      } as Exclude<typeof ctxOn, undefined>,
    };

    // Handle components that delegate to other c-metadata-component based components.
    // These components don't need to have complex attributes computed
    // because they will perform the same computation of attributes themselves.
    // They do this because they're designed to be used as standalone components
    // (and not strictly as components that have been delegated to by c-input).
    switch (valueMeta.type) {
      case "date":
        data.attrs = { ...ctxData.attrs };
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;
        return _c("c-datetime-picker", data);

      case "model":
        data.attrs = { ...ctxData.attrs };
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;
        return _c("c-select", data);

      case "collection":
        data.attrs = { ...ctxData.attrs };
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;

        if ("manyToMany" in valueMeta) {
          return _c("c-select-many-to-many", data);
        } else if (
          valueMeta.itemType.type != "model" &&
          valueMeta.itemType.type != "object" &&
          valueMeta.itemType.type != "file"
        ) {
          return _c("c-select-values", data);
        } else {
          // console.warn(`Unsupported collection type ${valueMeta.itemType.type} for v-input`)
        }
    }

    // We've now handled any components that will do their own computation of these
    // attributes via c-metadata-component's inputBindAttrs.
    // We now need to compute them in order to render components
    // that delegate directly to vuetify components.
    const attrs = (data.attrs = buildVuetifyAttrs(
      valueMeta,
      model,
      ctxData.attrs
    ));

    const onInput = function (value: any) {
      if (model && valueMeta) {
        (model as any)[valueMeta.name] = parseValue(value, valueMeta);
      }
    };

    // Handle components that delegate immediately to Vuetify
    switch (valueMeta.type) {
      case "string":
      case "number":
        if (valueMeta.role == "primaryKey") {
          // If this is an editable primary key, emit the value on change (leaving the field)
          // instead of on every keystroke. If we were to emit on every keystroke,
          // the very first character the user types would end up as the PK.
          addHandler(data.on, "change", onInput);
        } else {
          addHandler(data.on, "input", onInput);
        }

        if (valueMeta.type == "number") {
          // For numeric values, use a numeric text field.
          attrs.type = "number";
          return _c("v-text-field", data);
        }

        if (
          ("textarea" in attrs || valueMeta.subtype == "multiline") &&
          attrs.textarea !== false
        ) {
          return _c("v-textarea", data);
        }

        if (!attrs.type && valueMeta.subtype) {
          switch (valueMeta.subtype) {
            case "email":
            case "tel":
              attrs.type = valueMeta.subtype;
              break;
            case "url":
            case "url-image":
              attrs.type = "url";
              break;
            case "password":
              data.attrs = { ...data.attrs, ...data.props };
              return _c(passwordWrapper, data);
          }
        }
        return _c("v-text-field", data);

      case "boolean":
        // v-switch uses 'change' as its event, not 'input'.
        addHandler(data.on, "change", onInput);
        // It also uses 'input-value' instead of 'value' for its value prop.
        data.props["input-value"] = data.props.value;
        delete data.props.value;

        if ("checkbox" in attrs && attrs.checkbox !== false) {
          return _c("v-checkbox", data);
        }
        return _c("v-switch", data);

      case "enum":
        addHandler(data.on, "input", onInput);
        data.props.items = valueMeta.typeDef.values;
        data.props["item-text"] = "displayName";
        data.props["item-value"] = "value";
        if (valueMeta.typeDef.values.some((v) => v.description)) {
          data.scopedSlots = {
            item: ({ item }) =>
              _c("v-list-item-content", [
                _c("v-list-item-title", [item.displayName]),
                item.description
                  ? _c("v-list-item-subtitle", [item.description])
                  : _c(),
              ]),
          };
        }
        return _c("v-select", data);

      case "file":
        // v-file-input uses 'change' as its event, not 'input'.
        addHandler(data.on, "change", onInput);
        return _c("v-file-input", data);

      case "collection":
        if (valueMeta.itemType.type == "file") {
          // This is how static bool flag props are passed in vue.
          // We don't use `props` because `multiple` as an explicit prop
          // for `v-file-input` was only added quite recently.
          // Doing it like this will work in older vuetify versions too.
          data.attrs.multiple = "";

          // v-file-input uses 'change' as its event, not 'input'.
          addHandler(data.on, "change", onInput);
          return _c("v-file-input", data);
        }
    }

    // Fall back to just displaying the value.
    // Note that this probably looks bad on Vuetify 2+ because we're
    // abusing its internal classes to try to emulate the look of a text field,
    // but this hasn't been updated for 2.0.
    if (ctx.children) {
      return _c("div", {}, ctx.children);
    }
    return _c(
      "div",
      {
        staticClass: "input-group input-group--dirty input-group--text-field",
      },
      [
        _c("label", attrs.label),
        _c("p", [
          _c("c-display", {
            staticClass: "subtitle-1",
            props: {
              value: data.props.value,
              model: ctx.props.model,
              for: ctx.props.for,
            },
          }),
        ]),
      ]
    );
  },
});

function addHandler(
  on: { [key: string]: Function | Function[] },
  eventName: string,
  handler: Function
) {
  const oldValue = on[eventName];
  if (oldValue == null) {
    on[eventName] = handler;
  } else if (typeof oldValue == "function") {
    on[eventName] = [oldValue, handler];
  } else {
    oldValue.push(handler);
  }
}
