import { defineComponent, Prop, h, toHandlerKey } from "vue";
import {
  buildVuetifyAttrs,
  getValueMeta,
  makeMetadataProps,
} from "../c-metadata-component";
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

import CSelect from "./c-select.vue";
import CSelectManyToMany from "./c-select-many-to-many.vue";
import CSelectValues from "./c-select-values.vue";
import CDisplay from "../display/c-display";
import CDatetimePicker from "./c-datetime-picker.vue";
import {
  VCheckbox,
  VFileInput,
  VSelect,
  VSwitch,
  VTextarea,
  VTextField,
} from "vuetify/components";

const primitiveTypes = ["string", "number", "date", "enum", "boolean"];

const passwordWrapper = defineComponent({
  name: "c-password-input",
  data() {
    return {
      shown: false,
    };
  },
  render() {
    const data = {
      ...this.$attrs,
    } as Record<string, unknown>;
    data["append-inner-icon"] ??= !this.shown ? "fa fa-eye" : "fa fa-eye-slash";
    data.type = this.shown ? "text" : "password";
    addHandler(data, "click:appendInner", () => (this.shown = !this.shown));
    return h(VTextField, data);
  },
});

export default defineComponent({
  name: "c-input",

  inject: {
    "c-input-props": { default: {} },
  },

  props: {
    ...makeMetadataProps<
      Model<ClassType> | DataSource<DataSourceType> | AnyArgCaller
    >(),

    modelValue: <Prop<any>>{ required: false },
  },

  render() {
    let model = this.model;
    const modelMeta = model ? model.$metadata : null;

    const _valueMeta = getValueMeta(
      this.for,
      modelMeta,
      this.$coalesce.metadata
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

    const props = this.$props;

    let data = {
      ...this.$attrs, // Includes any non-props, as well as event handlers.
      modelValue: model
        ? (model as any)[valueMeta.name]
        : primitiveTypes.includes(valueMeta.type)
        ? mapValueToModel(props.modelValue, valueMeta)
        : props.modelValue,
    } as any;

    // Handle components that delegate to other c-metadata-component based components.
    // These components don't need to have complex attributes computed
    // because they will perform the same computation of attributes themselves.
    // They do this because they're designed to be used as standalone components
    // (and not strictly as components that have been delegated to by c-input).
    switch (valueMeta.type) {
      case "date":
        data.model = props.model;
        data.for = props.for;
        return h(CDatetimePicker, data);

      case "model":
        data.model = props.model;
        data.for = props.for;
        return h(CSelect, data);

      case "collection":
        data.model = props.model;
        data.for = props.for;

        if ("manyToMany" in valueMeta) {
          return h(CSelectManyToMany, data);
        } else if (
          valueMeta.itemType.type != "model" &&
          valueMeta.itemType.type != "object" &&
          valueMeta.itemType.type != "file"
        ) {
          return h(CSelectValues, data);
        } else {
          // console.warn(`Unsupported collection type ${valueMeta.itemType.type} for v-input`)
        }
    }

    // We've now handled any components that will do their own computation of these
    // attributes via c-metadata-component's inputBindAttrs.
    // We now need to compute them in order to render components
    // that delegate directly to vuetify components.
    data = {
      ...buildVuetifyAttrs(valueMeta, model, data),
      ...data,
    };

    const onInput = (value: any) => {
      const parsed = parseValue(value, valueMeta);
      if (model && valueMeta) {
        (model as any)[valueMeta.name] = parsed;
      }
      this.$emit("update:modelValue", parsed);
    };

    // Handle components that delegate immediately to Vuetify
    switch (valueMeta.type) {
      case "string":
      case "number":
        if ("createOnly" in valueMeta && valueMeta.createOnly) {
          // If this is a create-only property (e.g. an editable primary key),
          // emit the value on change(leaving the field)
          // instead of on every keystroke. If we were to emit on every keystroke,
          // the very first character the user types would end up as the field value.
          addHandler(data, "change", (valueOrEvent: Event | string) => {
            if (valueOrEvent instanceof Event) {
              // Vuetify3: workaround https://github.com/vuetifyjs/vuetify/issues/16637
              if (
                valueOrEvent.target instanceof HTMLInputElement ||
                valueOrEvent.target instanceof HTMLTextAreaElement
              ) {
                onInput(valueOrEvent.target.value);
              }
            } else {
              onInput(value);
            }
          });
        } else {
          addHandler(data, "update:modelValue", onInput);
        }

        if (valueMeta.type == "number") {
          // For numeric values, use a numeric text field.
          data.type = "number";
          return h(VTextField, data);
        }

        if (
          ("textarea" in data || valueMeta.subtype == "multiline") &&
          data.textarea !== false
        ) {
          return h(VTextarea, data);
        }

        if (!data.type && valueMeta.subtype) {
          switch (valueMeta.subtype) {
            case "email":
            case "tel":
              data.type = valueMeta.subtype;
              break;

            case "color":
              // Make clicks on the entire vuetify text field element trigger the color picker.
              // Without this, only clicks on the html input element itself, inside the text field, will open the picker.
              addHandler(data, "mouseup", function (e: MouseEvent) {
                const t = e.currentTarget as HTMLElement | null;
                t?.matches(".v-input__slot") &&
                  t.querySelector("input")?.click();
              });
              data.type = valueMeta.subtype;
              break;

            case "url":
            case "url-image":
              data.type = "url";
              break;

            case "password":
              return h(passwordWrapper, data);
          }
        }
        return h(VTextField, data);

      case "boolean":
        if ("checkbox" in data && data.checkbox !== false) {
          return h(VCheckbox, data);
        }
        return h(VSwitch, data);

      case "enum":
        addHandler(data, "update:modelValue", onInput);
        data.items = valueMeta.typeDef.values;
        data["item-title"] = "displayName";
        data["item-value"] = "value";
        // maps to the prop "subtitle" on v-list-item
        data["item-props"] = (item: any) => ({ subtitle: item.description });
        return h(VSelect, data);

      case "file":
        // v-file-input uses 'change' as its event, not 'input'.

        // In Vuetify3, VFileInput ONLY takes an array, even for single file selection.
        // It also explodes on null/undefined.
        const value = data.modelValue;
        if (!Array.isArray(value)) {
          data.modelValue = [];
          if (value) data.modelValue.push(value);
        }
        // Unwrap array on input
        addHandler(data, "update:modelValue", (value: any[]) =>
          onInput(value[0])
        );

        return h(VFileInput, data);

      case "collection":
        if (valueMeta.itemType.type == "file") {
          // This is how static bool flag props are passed in vue.
          // We don't use `props` because `multiple` as an explicit prop
          // for `v-file-input` was only added quite recently.
          // Doing it like this will work in older vuetify versions too.
          data.multiple = "";
          data.modelValue ??= [];

          addHandler(data, "update:modelValue", onInput);
          return h(VFileInput, data);
        }
    }

    // Fall back to just displaying the value.
    // Note that this probably looks bad on Vuetify 2+ because we're
    // abusing its internal classes to try to emulate the look of a text field,
    // but this hasn't been updated for 2.0.
    if (this.$slots) {
      // TODO: this.$slots might be always defined
      return h("div", {}, this.$slots);
    }
    return h(
      "div",
      {
        staticClass: "input-group input-group--dirty input-group--text-field",
      },
      [
        h("label", data.label),
        h("p", [
          h(CDisplay, {
            staticClass: "subtitle-1",
            props: {
              value: data.value,
              model: props.model,
              for: props.for,
            },
          }),
        ]),
      ]
    );
  },
});

function addHandler(data: any, eventName: string, handler: Function) {
  eventName = toHandlerKey(eventName);
  // consider using mergeProps (import from vue) here?
  const oldValue = data[eventName];
  if (oldValue == null) {
    data[eventName] = handler;
  } else if (typeof oldValue == "function") {
    // TODO: Does this work in vue3?
    data[eventName] = [oldValue, handler];
  } else {
    oldValue.push(handler);
  }
}
