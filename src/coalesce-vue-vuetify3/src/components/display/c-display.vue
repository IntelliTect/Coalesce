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

type DistanceFormatOption = {
  distance: true;
  addSuffix?: boolean;
  includeSeconds?: boolean;
};

function isDistanceFormat(
  formatOption: DisplayOptions["format"] | undefined,
): formatOption is DistanceFormatOption {
  return (
    !!formatOption &&
    typeof formatOption == "object" &&
    "distance" in formatOption
  );
}

/** Determine how long until the rendered distance string (e.g. "5 minutes ago")
 * would next change, so we can schedule a refresh exactly when it's needed
 * rather than on a fixed interval. */
function getDistanceRefreshDelayMs(
  parsed: Date,
  formatOption: DistanceFormatOption,
): number {
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

/** Wrapper that re-renders a date distance display on an adaptive schedule so
 * that relative time text stays current without requiring callers to manually
 * force re-renders. Only instantiated for date values formatted with
 * `{ distance: true }`, so non-distance displays incur no timer overhead. */
const distanceWrapper = defineComponent({
  name: "CDistanceDisplay",
  props: {
    element: { type: String, default: "span" },
    value: { type: Date, required: true },
    meta: { type: Object as PropType<DateValue>, required: true },
    options: { type: Object as PropType<DisplayOptions>, default: undefined },
  },
  setup(props, { attrs, slots }) {
    const refreshTick = ref(0);

    watchEffect((onCleanup) => {
      // Re-arm the timer whenever a tick fires or the inputs change.
      void refreshTick.value;
      const formatOption = props.options?.format;
      if (!isDistanceFormat(formatOption) || isNaN(props.value.getTime())) {
        return;
      }

      const delayMs = getDistanceRefreshDelayMs(props.value, formatOption);
      const timeout = setTimeout(() => {
        refreshTick.value += 1;
      }, delayMs);
      onCleanup(() => clearTimeout(timeout));
    });

    return () => {
      void refreshTick.value;
      const valueString = valueDisplay(props.value, props.meta, props.options);
      return h(props.element, attrs, valueString || slots);
    };
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
import type { PropType } from "vue";
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

function render() {
  const resolved = resolveDisplayValue();
  if (resolved == null) {
    // If no model and no value were provided, just display nothing.
    // This isn't an error case - it just means the thing we're trying to display
    // is `null`-ish, and should be treated the same way that vue would treat {{null}}
    return h(props.element);
  }

  const { meta, valueProp, options } = resolved;

  if (
    meta.type === "date" &&
    isDistanceFormat(options?.format) &&
    valueProp instanceof Date &&
    !isNaN(valueProp.getTime())
  ) {
    // Delegate to a wrapper that keeps the relative time text up to date.
    return h(distanceWrapper, {
      ...attrs,
      element: props.element,
      value: valueProp,
      meta,
      options,
    });
  }

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
