<script lang="ts">
const standaloneDateValueMeta: DateValue = {
  name: "",
  displayName: "",
  type: "date",
  dateKind: "datetime",
  role: "value",
};

const passwordWrapper = defineComponent({
  name: "CPasswordDisplay",
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
    return h(
      this.element,
      mergeProps(this.$attrs, {
        class: this.shown
          ? "c-password-display c-password-display--shown"
          : "c-password-display c-password-display--hidden",
      }),
      [
        h("i", {
          class: [!this.shown ? "fa fa-eye" : "fa fa-eye-slash", "pr-1"],
          role: "button",
          pressed: this.shown,
          title: !this.shown ? "Reveal" : "Hide",
          onClick: (ev) => {
            this.shown = !this.shown;
            ev.stopPropagation();
          },
        }),
        !this.shown ? "•".repeat(8) : this.value,
      ],
    );
  },
});

export type CDisplayProps<TModel extends Model | AnyArgCaller | undefined> = {
  /** An object owning the value to be edited that is specified by the `for` prop. */
  model?: TModel | null;

  /** A metadata specifier for the value being bound. One of:
   * * A string with the name of the value belonging to `model`. E.g. `"firstName"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.props.firstName`.
   * * A string in dot-notation that starts with a type name. E.g. `"Person.firstName"`.
   */
  for?: ForSpec<TModel>;

  element?: string;

  /** Options for formatting the output.
   * See [DisplayOptions](https://coalesce.intellitect.com/stacks/vue/layers/models.html#displayoptions). */
  options?: DisplayOptions;

  /** Shorthand for `:options="{ format: ... }"` */
  format?: DisplayOptions["format"];

  modelValue?: any;
  // `value` is backwards compat with vue2 style:
  value?: any;
};
</script>

<script
  lang="ts"
  setup
  generic="TModel extends Model | AnyArgCaller | undefined"
>
import {
  defineComponent,
  h,
  mergeProps,
  useAttrs,
  useSlots,
  ref,
  watchEffect,
} from "vue";
import { formatDistance } from "date-fns";
import { type ForSpec, useMetadataProps } from "../c-metadata-component";
import { valueDisplay } from "coalesce-vue";
import type {
  DisplayOptions,
  DateValue,
  Model,
  AnyArgCaller,
} from "coalesce-vue";

const props = withDefaults(defineProps<CDisplayProps<TModel>>(), {
  element: "span",
});

const {
  modelMeta: modelMetaRef,
  valueMeta,
  valueOwner,
} = useMetadataProps(props);

defineOptions({
  name: "c-display",
});

const attrs = useAttrs();

defineSlots(); // Empty defineSlots() prevents TS errors for passthrough slots.
const slots = useSlots();
const refreshTick = ref(0);

function resolveDisplayValue() {
  const model = props.model;
  let valueProp = props.modelValue ?? props.value;

  if (model == null && valueProp == null) {
    return null;
  }

  const modelMeta = modelMetaRef.value;
  let meta = valueMeta.value;
  if (!meta && modelMeta && "displayProp" in modelMeta) {
    meta = modelMeta.displayProp || null;
  }

  if (!meta && valueProp instanceof Date) {
    // Allow direct formatting of dates with <c-display :value="date" />
    meta = standaloneDateValueMeta;
  }

  if (!meta) {
    throw Error(
      "Provided model has no $metadata property, and no specific value was provided via the 'for' component prop to c-display.",
    );
  }

  if ("params" in meta) {
    throw Error("Cannot display a method");
  }

  let options = props.options;
  if (props.format) {
    options = { ...options, format: props.format as any };
  }

  valueProp ??= (valueOwner.value as any)[meta.name];

  return { meta, options, valueProp };
}

function getRefreshDelayMs(): number | null {
  const resolved = resolveDisplayValue();
  if (!resolved || resolved.meta.type !== "date") return null;

  const formatOption = resolved.options?.format;
  if (
    !formatOption ||
    typeof formatOption != "object" ||
    !("distance" in formatOption)
  ) {
    return null;
  }

  const parsed = resolved.valueProp;
  if (!(parsed instanceof Date) || isNaN(parsed.getTime())) return null;

  const addSuffix = formatOption.addSuffix ?? true;
  const includeSeconds = formatOption.includeSeconds ?? false;
  const now = new Date();
  const currentDistance = formatDistance(parsed, now, {
    addSuffix,
    includeSeconds,
  });

  const candidateDelaysMs = includeSeconds
    ? [
        1000, 2000, 5000, 10000, 15000, 30000, 60000, 300000, 900000, 1800000,
        3600000, 21600000, 43200000, 86400000,
      ]
    : [30000, 60000, 300000, 900000, 1800000, 3600000, 21600000, 86400000];

  for (const delayMs of candidateDelaysMs) {
    const nextDistance = formatDistance(
      parsed,
      new Date(now.getTime() + delayMs),
      {
        addSuffix,
        includeSeconds,
      },
    );
    if (nextDistance !== currentDistance) {
      return delayMs;
    }
  }

  return 86400000;
}

watchEffect((onCleanup) => {
  refreshTick.value;
  const refreshDelayMs = getRefreshDelayMs();
  if (refreshDelayMs == null) return;

  const timeout = setTimeout(() => {
    refreshTick.value += 1;
  }, refreshDelayMs);

  onCleanup(() => clearTimeout(timeout));
});

function render() {
  const resolved = resolveDisplayValue();
  if (resolved == null) {
    // If no model and no value were provided, just display nothing.
    // This isn't an error case - it just means the thing we're trying to display
    // is `null`-ish, and should be treated the same way that vue would treat {{null}}
    return h(props.element);
  }

  void refreshTick.value;
  const { meta, valueProp, options } = resolved;
  let valueString = valueDisplay(valueProp, meta, options);

  if (meta.type === "string" && valueString) {
    switch (meta.subtype) {
      case "password":
        return h(passwordWrapper, {
          ...attrs,
          value: valueString,
        });

      case "multiline":
        return h(
          props.element,
          {
            ...attrs,
            style: "white-space: pre-wrap",
          },
          valueString || slots,
        );

      case "url-image":
        return h("img", {
          ...attrs,
          src: valueString,
        });

      case "color":
        return h(
          props.element,
          {
            ...attrs,
            style: "white-space: nowrap",
          },
          [
            h("span", {
              style: `background-color: ${valueString.replace(
                /[^A-Fa-f0-9#]/g,
                "",
              )};`,
              class: "c-display--color-swatch",
            }),
            valueString || slots,
          ] as any,
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
            return h(
              "a",
              {
                ...attrs,
                href,
              },
              valueString,
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

  return h(props.element, attrs, valueString || slots);
}
</script>

<template>
  <render />
</template>
