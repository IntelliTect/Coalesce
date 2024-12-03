<script lang="ts">
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
    return h(VTextField, data, this.$slots);
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

type _ValueType<
  TModel extends Model | DataSource | AnyArgCaller | undefined,
  TFor extends ForSpec<TModel>
> = TModel extends ApiStateTypeWithArgs<any, any, infer TArgsObj, any>
  ? TFor extends keyof TArgsObj
    ? TArgsObj[TFor]
    : any
  : TFor extends Value
  ? MetadataToModelType<TFor>
  : TFor extends string
  ? TFor extends keyof ModelTypeLookup
    ? ModelTypeLookup[TFor]
    : TModel extends Model
    ? TFor extends PropNames<TModel["$metadata"]>
      ? MetadataToModelType<TModel["$metadata"]["props"][TFor]>
      : any
    : any
  : any;
</script>

<script
  lang="ts"
  setup
  generic="TModel extends Model | DataSource | AnyArgCaller | undefined, 
  TFor extends ForSpec<TModel> = any"
>
import { defineComponent, h, toHandlerKey, useSlots, useAttrs } from "vue";
import {
  buildVuetifyAttrs,
  useMetadataProps,
  type ForSpec,
} from "../c-metadata-component";
import { TypedValidationRule } from "../../util";
import {
  type Model,
  type DataSource,
  type AnyArgCaller,
  mapValueToModel,
  parseValue,
  ApiStateTypeWithArgs,
  ModelTypeLookup,
  PropNames,
  Value,
  MetadataToModelType,
} from "coalesce-vue";

import CSelect from "./c-select.vue";
import CSelectManyToMany from "./c-select-many-to-many.vue";
import CSelectValues from "./c-select-values.vue";
import CDisplay from "../display/c-display.vue";
import CDatetimePicker from "./c-datetime-picker.vue";
import {
  VAutocomplete,
  VCheckbox,
  VFileInput,
  VSelect,
  VSwitch,
  VTextarea,
  VTextField,
} from "vuetify/components";

type ValueType = _ValueType<TModel, TFor>;

defineOptions({
  name: "c-input",
  inject: {
    "c-input-props": { default: {} },
  },
});

const props = withDefaults(
  defineProps<{
    /** An object owning the value to be edited that is specified by the `for` prop. */
    model?: TModel | null;

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"firstName"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
     * * A string in dot-notation that starts with a type name. E.g. `"Person.firstName"`.
     */
    for: TFor;

    rules?: Array<TypedValidationRule<ValueType>>;

    modelValue?: ValueType | null;
  }>(),
  {}
);

const { valueMeta: valueMetaRef, valueOwner: valueOwnerRef } =
  useMetadataProps(props);

const emit = defineEmits<{
  (e: "update:modelValue", v: ValueType | null): void;
}>();

const attrs = useAttrs();

defineSlots(); // Empty defineSlots() prevents TS errors for passthrough slots.
const slots = useSlots();

function render() {
  const valueMeta = valueMetaRef.value;
  const valueOwner = valueOwnerRef.value;

  if (!valueMeta || !("role" in valueMeta)) {
    throw Error(
      "c-input requires value metadata. Specify it with the 'for' prop'"
    );
  }

  let data = {
    ...attrs, // Includes any non-props, as well as event handlers.
    rules: props.rules ?? attrs.rules,
    modelValue: valueOwner
      ? valueOwner[valueMeta.name]
      : primitiveTypes.includes(valueMeta.type)
      ? mapValueToModel(props.modelValue, valueMeta)
      : props.modelValue,
  } as any;
  if (data.rules === undefined) delete data.rules;

  // Do not pass the default slot through to vuetify.
  // It will put it in a weird spot in most inputs.
  const { default: defaultSlot, ...vuetifySlots } = slots;

  // Handle components that delegate to other c-metadata-component based components.
  // These components don't need to have complex attributes computed
  // because they will perform the same computation of attributes themselves.
  // They do this because they're designed to be used as standalone components
  // (and not strictly as components that have been delegated to by c-input).
  switch (valueMeta.type) {
    case "date":
      data.model = props.model;
      data.for = props.for;
      addHandler(data, "update:modelValue", (v: any) =>
        emit("update:modelValue", v)
      );
      return h(CDatetimePicker, data, vuetifySlots);

    case "model":
      data.model = props.model;
      data.for = props.for;
      addHandler(data, "update:modelValue", (v: any) =>
        emit("update:modelValue", v)
      );
      return h(CSelect<any>, data, vuetifySlots);

    case "collection":
      data.model = props.model;
      data.for = props.for;

      if ("manyToMany" in valueMeta) {
        addHandler(data, "update:modelValue", (v: any) =>
          emit("update:modelValue", v)
        );
        return h(CSelectManyToMany<any, any>, data, vuetifySlots);
      } else if (valueMeta.itemType.type == "model") {
        data.model = props.model;
        data.for = props.for;
        addHandler(data, "update:modelValue", (v: any) =>
          emit("update:modelValue", v)
        );
        return h(CSelect<any>, data, vuetifySlots);
      } else if (
        valueMeta.itemType.type != "object" &&
        valueMeta.itemType.type != "enum" &&
        valueMeta.itemType.type != "file"
      ) {
        addHandler(data, "update:modelValue", (v: any) =>
          emit("update:modelValue", v)
        );
        return h(CSelectValues<any>, data, vuetifySlots);
      } else {
        // console.warn(`Unsupported collection type ${valueMeta.itemType.type} for v-input`)
      }
  }

  // We've now handled any components that will do their own computation of these
  // attributes via c-metadata-component's inputBindAttrs.
  // We now need to compute them in order to render components
  // that delegate directly to vuetify components.
  data = {
    ...buildVuetifyAttrs(valueMeta, props.model, data),
    ...data,
  };

  const onInput = (value: any) => {
    const parsed = parseValue(value, valueMeta);
    if (valueOwner && valueMeta) {
      valueOwner[valueMeta.name] = parsed;
    }
    emit("update:modelValue", parsed);
  };

  // Handle components that delegate immediately to Vuetify
  switch (valueMeta.type) {
    case "string":
    case "number":
      if ("createOnly" in valueMeta && valueMeta.createOnly) {
        // If this is a create-only property (e.g. an editable primary key),
        // emit the value on change(leaving the field)
        // instead of on every keystroke. If we were to emit on every keystroke,
        // the very first character the user types would end up as the field value
        // when autosaves are being used.
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
            onInput(valueOrEvent);
          }
        });
      } else {
        addHandler(data, "update:modelValue", onInput);
      }

      if (valueMeta.type == "number") {
        // For numeric values, use a numeric text field.
        data.type = "number";
        return h(VTextField, data, vuetifySlots);
      }

      if (
        ("textarea" in data || valueMeta.subtype == "multiline") &&
        data.textarea !== false
      ) {
        return h(VTextarea, data, vuetifySlots);
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
              t?.matches(".v-input__slot") && t.querySelector("input")?.click();
            });
            data.type = valueMeta.subtype;
            break;

          case "url":
          case "url-image":
            data.type = "url";
            break;

          case "password":
            return h(passwordWrapper, data, vuetifySlots);
        }
      }
      return h(VTextField, data, vuetifySlots);

    case "boolean":
      addHandler(data, "update:modelValue", onInput);
      if ("checkbox" in data && data.checkbox !== false) {
        return h(VCheckbox, data, vuetifySlots);
      }
      return h(VSwitch, data, vuetifySlots);

    case "enum":
      addHandler(data, "update:modelValue", onInput);
      data.items = valueMeta.typeDef.values;
      data["item-title"] = "displayName";
      data["item-value"] = "value";
      // maps to the prop "subtitle" on v-list-item
      data["item-props"] = (item: any) => ({ subtitle: item.description });
      return h(VSelect, data, vuetifySlots);

    case "file":
      // Unwrap array on input. Vuetify used to not do this, and then they made a breaking change.
      // https://github.com/vuetifyjs/vuetify/commit/581bbbcab572074ab090b244d54a34c062404992
      // This wrapper maintains compatibility before and after this breaking change.
      const updateHandler: VFileInput["$props"]["onUpdate:modelValue"] = (
        value
      ) => onInput(Array.isArray(value) ? value[0] : value);
      addHandler(data, "update:modelValue", updateHandler);

      return h(VFileInput, data, vuetifySlots);

    case "collection":
      if (valueMeta.itemType.type == "file") {
        // This is how static bool flag props are passed in vue.
        // We don't use `props` because `multiple` as an explicit prop
        // for `v-file-input` was only added quite recently.
        // Doing it like this will work in older vuetify versions too.
        data.multiple = "";
        data.modelValue ??= [];

        addHandler(data, "update:modelValue", onInput);
        return h(VFileInput, data, vuetifySlots);
      } else if (valueMeta.itemType.type == "enum") {
        data.items = valueMeta.itemType.typeDef.values;
        data.multiple = true;
        data["chips"] = true;
        data["closable-chips"] = true;
        data["item-title"] = "displayName";
        data["item-value"] = "value";
        // maps to the prop "subtitle" on v-list-item
        data["item-props"] = (item: any) => ({ subtitle: item.description });

        addHandler(data, "update:modelValue", onInput);
        return h(VAutocomplete, data, vuetifySlots);
      }
  }

  // Fall back to just displaying the value.
  if (defaultSlot) {
    return h("div", {}, { default: defaultSlot });
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
}
</script>

<template>
  <render />
</template>
