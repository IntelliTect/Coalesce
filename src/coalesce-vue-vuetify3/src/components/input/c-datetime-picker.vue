<template>
  <v-text-field
    v-if="native"
    class="c-datetime-picker"
    :type="
      internalDateKind == 'time'
        ? 'time'
        : internalDateKind == 'date'
        ? 'date'
        : 'datetime-local'
    "
    :modelValue="nativeValue"
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :error-messages="error"
    :readonly="isReadonly"
    :disabled="isDisabled"
    autocomplete="off"
    v-model:focused="focused"
    @change="textInputChanged($event, true)"
  >
    <template v-for="(_, slot) of $slots" v-slot:[slot]="scope">
      <slot :name="slot" v-bind="scope" />
    </template>
  </v-text-field>

  <v-text-field
    v-else
    class="c-datetime-picker"
    :placeholder="internalFormat"
    :append-inner-icon="
      internalDateKind == 'time'
        ? 'fa fa-clock cursor-pointer'
        : 'fa fa-calendar-alt cursor-pointer'
    "
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :modelValue="internalTextValue == null ? displayedValue : internalTextValue"
    :error-messages="error"
    :readonly="isReadonly"
    :disabled="isDisabled"
    autocomplete="off"
    v-model:focused="focused"
    @keydown.enter="focused = false"
    @keydown.escape="focused = false"
    @update:model-value="textInputChanged($event, false)"
    @click="menu = !menu"
  >
    <template v-for="(_, slot) of $slots" v-slot:[slot]="scope">
      <slot :name="slot" v-bind="scope" />
    </template>
    <template #default>
      <!-- TODO: Consider fullscreen modal on small devices -->
      <v-menu
        v-if="isInteractive"
        v-model="menu"
        activator="parent"
        content-class="c-datetime-picker__menu"
        :close-on-content-click="false"
        :open-on-click="false"
        min-width="1px"
      >
        <v-fab
          app
          location="bottom right"
          :color="color"
          size="x-small"
          icon="$complete"
          @click="menu = false"
          :title="$vuetify.locale.t('$vuetify.close')"
        >
        </v-fab>
        <v-card class="d-flex">
          <v-date-picker
            v-if="showDate"
            :color="color!"
            :modelValue="internalValueZoned"
            @update:modelValue="dateChanged"
            density="comfortable"
            scrollable
            :rounded="false"
            :allowedDates="(allowedDates as any)"
            :min="min ? startOfDay(min) : undefined"
            :max="max ? endOfDay(max) : undefined"
            v-bind="datePickerProps"
          >
          </v-date-picker>

          <v-divider vertical></v-divider>

          <c-time-picker
            v-if="showTime"
            :model-value="internalValueZoned"
            @update:model-value="timeChanged"
            :step="step ?? undefined"
            :min="min"
            :max="max"
            :color
          >
            <template #header>
              {{ displayedTime || "&nbsp;" }}
            </template>
          </c-time-picker>
        </v-card>
      </v-menu>
    </template>
  </v-text-field>
</template>

<style lang="scss">
.c-datetime-picker__menu {
  > .v-card {
    @media screen and (max-width: 600px) {
      flex-wrap: wrap;
      > * {
        width: 100%;
        flex-grow: 1;
      }
    }
  }
  .v-date-picker {
    width: 300px;
    overflow-y: auto;
  }
  .v-picker-title {
    display: none;
  }

  .v-date-picker-header {
    height: auto;
    padding: 6px 14px;
    font-size: 32px;
  }

  .v-date-picker-month__day {
    width: 36px;

    .v-btn {
      font-size: 14px !important;
      font-weight: 400;
    }
  }
  .v-date-picker-months {
    .v-date-picker-months__content {
      padding: 8px;
      grid-template-columns: repeat(3, 1fr);
      grid-gap: 4px 8px;
    }
  }
}
</style>

<script lang="ts">
import { VTextField, VDatePicker } from "vuetify/components";
import { TypedValidationRule } from "../../util";

type InheritedProps = Omit<
  VTextField["$props"],
  | InheritExcludePropNames
  | "readonly"
  | "disabled"
  | "rules"
  | "errorMessages"
  | "focused"
  | "onUpdate:focused"
>;

type DatePickerProps = Omit<
  VDatePicker["$props"],
  InheritExcludePropNames | "position"
>;

type _InheritedSlots = Omit<VTextField["$slots"], "default">;
// This useless mapped type prevents vue-tsc from getting confused
// and failing to emit any types at all. When it encountered the mapped type,
// it doesn't know how to handle it and so leaves it un-transformed.
type InheritedSlots = {
  [Property in keyof _InheritedSlots]: _InheritedSlots[Property];
};
</script>

<script
  lang="ts"
  setup
  generic="TModel extends Model | AnyArgCaller | undefined"
>
/*
  This component is designed to work either with <... model="model" for="for" /> or with <... v-model="value" />
*/

import {
  isValid,
  parse,
  setYear,
  setMonth,
  setDate,
  setHours,
  setMinutes,
  startOfDay,
  endOfDay,
  startOfHour,
} from "date-fns";
import { format, toZonedTime, fromZonedTime } from "date-fns-tz";
import {
  getDefaultTimeZone,
  parseDateUserInput,
  DateKind,
  AnyArgCaller,
  Model,
  DateValue,
} from "coalesce-vue";
import { computed, ref, watch } from "vue";
import {
  ForSpec,
  InheritExcludePropNames,
  useCustomInput,
  useMetadataProps,
} from "../c-metadata-component";
import CTimePicker from "./c-time-picker.vue";

defineOptions({
  name: "c-datetime-picker",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in the autocomplete.
  inheritAttrs: false,
});

const props = withDefaults(
  defineProps<
    {
      /** An object owning the value to be edited that is specified by the `for` prop. */
      model?: TModel | null;

      /** A metadata specifier for the value being bound. One of:
       * * A string with the name of the value belonging to `model`. E.g. `"startDate"`.
       * * A direct reference to the metadata object. E.g. `model.$metadata.props.startDate`.
       * * A string in dot-notation that starts with a type name. E.g. `"Person.startDate"`.
       */
      for?: ForSpec<TModel, DateValue>;

      rules?: Array<TypedValidationRule<Date>>;
      /** Specifies whether this input is picking date, time, or both. */
      dateKind?: DateKind | null;
      /** The format of the selected value displayed in the text field.*/
      dateFormat?: string | null;
      readonly?: boolean | null;
      disabled?: boolean | null;
      /** Use native HTML5 date picker rather than Vuetify. */
      native?: boolean | null;
      color?: string | null;
      closeOnDatePicked?: boolean | null;

      /** The IANA time zone name that the user will pick the date/time value in.
       * Falls back to the value configured with `coalesce-vue`'s `setDefaultTimeZone`
       * if the value bound to with `model`/`for` is a `DateTimeOffset`.
       */
      timeZone?: string | null;

      /** The allowed increments, in minutes, of the selectable value.
       * Value should divide 60 evenly, or be multiples of 60 */
      step?: number | null;

      /** The minimum date/time value allowed. */
      min?: Date | null;
      /** The maximum date/time value allowed. */
      max?: Date | null;
      /** An array of permitted dates (items should have a time of midnight),
       * or a function that returns true if a date is allowed for selection.
       * Does not impact time selection. */
      allowedDates?: null | Date[] | ((date: Date) => boolean);
      // Object containing extra props to pass through to `v-date-picker`.
      datePickerProps?: DatePickerProps;
    } & /* @vue-ignore */ InheritedProps
  >(),
  { closeOnDatePicked: null, color: "secondary" }
);

defineSlots<InheritedSlots>();

const modelValue = defineModel<Date | null | undefined>();

const { inputBindAttrs, modelMeta, valueMeta, valueOwner } =
  useMetadataProps(props);

const focused = ref(false);
const error = ref<string[]>([]);
const menu = ref(false);
const internalTextValue = ref<string>();

const { isDisabled, isReadonly, isInteractive } = useCustomInput(props);

const dateMeta = computed(() => {
  const meta = valueMeta.value;
  if (meta && meta.type == "date") {
    return meta;
  }
  return null;
});

/** The value to bind to the `value` of a native HTML5 date input */
const nativeValue = computed(() => {
  return internalValueZoned.value
    ? format(internalValueZoned.value, nativeInternalFormat.value, {
        timeZone: internalTimeZone.value || undefined,
      })
    : null;
});

const displayedValue = computed(() => {
  return internalValueZoned.value
    ? format(internalValueZoned.value, internalFormat.value, {
        timeZone: internalTimeZone.value || undefined,
      })
    : null;
});

const displayedTime = computed(() => {
  return internalValueZoned.value
    ? format(
        internalValueZoned.value,
        "h:mm a" + (internalTimeZone.value ? " z" : ""),
        {
          timeZone: internalTimeZone.value || undefined,
        }
      )
    : null;
});

const internalTimeZone = computed(() => {
  if (props.timeZone) {
    return props.timeZone;
  }
  if (dateMeta.value) {
    if (!dateMeta.value.noOffset) {
      // date is a DateTimeOffset, so TZ conversions are meaningful.
      return (
        getDefaultTimeZone() ?? Intl.DateTimeFormat().resolvedOptions().timeZone
      );
    } else {
      // date is a DateTime, where TZ conversions would actually be harmful. Don't use the default.
      return null;
    }
  }

  // This is a bare v-model usage of the component.
  // Don't use the default since we don't know the user's intent.
  return null;
});

const internalValue = computed((): Date | null | undefined => {
  if (valueOwner.value && dateMeta.value) {
    return (valueOwner.value as any)[dateMeta.value.name];
  }

  return modelValue.value;
});

const internalValueZoned = computed(() => {
  const value = internalValue.value;
  if (!value || !internalTimeZone.value) return value;
  return toZonedTime(value, internalTimeZone.value);
});

const internalDateKind = computed((): DateKind => {
  if (props.dateKind) return props.dateKind;
  if (dateMeta.value) return dateMeta.value.dateKind;
  return "datetime";
});

const nativeInternalFormat = computed(() => {
  // These are the formats expected by HTMLInputElement.value
  // when its type is one of the native date/time types.
  switch (internalDateKind.value) {
    case "date":
      return "yyyy-MM-dd";
    case "time":
      return "HH:mm";
    case "datetime":
    default:
      return "yyyy-MM-dd'T'HH:mm";
  }
});

const internalFormat = computed(() => {
  if (props.native) {
    return nativeInternalFormat.value;
  }

  if (props.dateFormat) return props.dateFormat;
  switch (internalDateKind.value) {
    case "date":
      return "M/d/yyyy";
    case "time":
      return "h:mm a";
    case "datetime":
    default:
      return "M/d/yyyy h:mm a";
  }
});

/** The effective set of validation rules to pass to the text field. Ensures that the real Date value is passed to the rule, rather than the text field's string value. */
const effectiveRules = computed(() => {
  if (props.rules) return props.rules;
  return inputBindAttrs.value.rules?.map(
    (ruleFunc: (value: Date | null | undefined) => string | boolean) => () =>
      ruleFunc(internalValue.value)
  );
});

const showDate = computed(() => {
  return (
    internalDateKind.value == "datetime" || internalDateKind.value == "date"
  );
});

const showTime = computed(() => {
  return (
    internalDateKind.value == "datetime" || internalDateKind.value == "time"
  );
});

function createDefaultDate() {
  const date = new Date();
  if (props.dateKind == "date") {
    const zone = internalTimeZone.value;
    if (!zone) return startOfDay(date);

    return fromZonedTime(startOfDay(toZonedTime(date, zone)), zone);
  } else {
    // Initialing with no minutes/seconds/milliseconds
    // is usually going to be more useful then picking the current time's minutes/seconds.
    return startOfHour(date);
  }
}

function parseUserInput(val: string) {
  var value: Date | null;
  const referenceDate = internalValueZoned.value || createDefaultDate();

  if (!val || !val.trim()) {
    return null;
  } else {
    value = parse(val, internalFormat.value, referenceDate);

    // If failed, try normalizing common separators to the same symbol in
    // both the format string and user input.
    if (!isValid(value)) {
      const separatorRegex = /[\-\\\/\.]/g;
      value = parse(
        val.replace(separatorRegex, "-"),
        internalFormat.value.replace(separatorRegex, "-"),
        referenceDate
      );
    }

    if (
      !isValid(value) ||
      // A year less than 100(0?) is also invalid.
      // This means that the format for the year was "yyyy",
      // but the user only entered "yy" (or entered 3 digits by accident, hence checking 1000 instead of 100).
      value.getFullYear() <= 1000
    ) {
      if (props.native) {
        // HTML 5 native date input keyboard events are a disaster.
        // They'll emit events for any date that is /technically/ valid,
        // including dates like 0002-06-13.
        // To prevent this, we have to assume that users always mean to type in a year > 1000,
        // which is a pretty reasonable assumption to make for most applications unless
        // someone makes a Coalesce app to detail the events of the Viking Age.

        // We can't parse and emit intermediate dates as the user is typing,
        // since doing so will reset the internal state of the HTML5 date input
        // that keeps track of which character of the date segment that the user was typing in.
        // So, we have to just ignore the user's input in this case.

        // Ignore all events if the year isn't fully typed.
        return null;
      }

      // If the input didn't match our format exactly,
      // try parsing user input with general formatting interpretation (trying to be a good citizen).
      // DO NOT do this if the input doesn't have a date part.
      // Behavior of new Date() is generally always Invalid Date if you just give it a time,
      // except if you're on Chrome and give it an invalid time like "8:98 AM" - it'll give you "Thu Jan 01 1998 08:00:00".
      // Since the user wouldn't ever see the date part when only entering a time, there's no chance to detect this error.
      value =
        parseDateUserInput(val, referenceDate, internalDateKind.value) ?? null;
    }

    if (value && internalTimeZone.value) {
      // The date was parsed against the current browser timeZone.
      // This (poorly named) function will shift it into the desired timezone.
      value = fromZonedTime(value, internalTimeZone.value);
    }
  }

  return value;
}

function textInputChanged(val: string | Event, isNative: boolean) {
  if (val instanceof Event) {
    val = (val.target as HTMLInputElement)?.value;
  }

  error.value = [];

  if (val == "" || val == null) {
    // Emptystring is emitted when the user clicks "clear" in the date picker popup,
    // or if they delete all characters from the input.
    internalTextValue.value = "";
    emitInput(null);
    return;
  }

  var value: Date | null;
  const referenceDate = internalValueZoned.value || createDefaultDate();

  if (isNative) {
    value = parse(val, nativeInternalFormat.value, referenceDate);
  } else {
    // Capture the user's intermediate text input
    internalTextValue.value = val;
    value = parseUserInput(val);
  }

  // Only emit an event if the input isn't invalid.
  // If we don't emit an input event, it gives the user a chance to correct their text.
  if (isValid(value)) emitInput(value);
}

function timeChanged(input: Date) {
  error.value = [];

  var value = internalValueZoned.value || createDefaultDate();

  value = setHours(value, input.getHours());
  value = setMinutes(value, input.getMinutes());

  if (internalTimeZone.value) {
    value = fromZonedTime(value, internalTimeZone.value);
  }

  emitInput(value);
}

function dateChanged(input: unknown) {
  // Typed as unknown because of bad types in vuetify
  if (!input || !(input instanceof Date)) return;

  error.value = [];

  var value = internalValueZoned.value || createDefaultDate();

  // Reset this first in case the year/month aren't valid for the current day.
  value = setDate(value, 1);

  value = setYear(value, input.getFullYear());
  value = setMonth(value, input.getMonth());
  value = setDate(value, input.getDate());

  if (internalTimeZone.value) {
    value = fromZonedTime(value, internalTimeZone.value);
  }

  emitInput(value);

  // If closeOnDatePicked isn't specified, auto-close if only picking a date.
  // Otherwise, respect closeOnDatePicked.
  if (
    props.closeOnDatePicked == null
      ? internalDateKind.value == "date"
      : props.closeOnDatePicked
  ) {
    close();
  }
}

function emitInput(value: Date | null) {
  if (value) {
    if (props.allowedDates) {
      // With validation of allowedDates, we have to just return early without emitting
      // since there's no logic we can apply to clamp the date to a valid date.

      if (
        Array.isArray(props.allowedDates) &&
        !props.allowedDates.includes(startOfDay(value))
      ) {
        error.value.push("The selected date is not allowed.");
        return;
      } else if (
        typeof props.allowedDates == "function" &&
        !props.allowedDates(value)
      ) {
        error.value.push("The selected date is not allowed.");
        return;
      }
    }

    if (props.min && value.valueOf() < props.min.valueOf()) {
      value = props.min;
    }

    if (props.max && value.valueOf() > props.max.valueOf()) {
      value = props.max;
    }

    if (props.step) {
      const stepMs = props.step * 60 * 1000;
      let newTime = Math.round(value.valueOf() / stepMs) * stepMs;

      // Applying step rounding may have taken us outside min/max,
      // so shift by one step to bring us within bounds.
      if (props.max && newTime > props.max.valueOf()) newTime -= stepMs;
      else if (props.min && newTime < props.min.valueOf()) newTime += stepMs;

      value = new Date(newTime);
    }
  }

  if (valueOwner.value && dateMeta.value) {
    (valueOwner.value as any)[dateMeta.value.name] = value;
  }

  if (modelValue.value != value) {
    modelValue.value = value;
  }
}

function close() {
  menu.value = false;
}

watch(focused, (focused) => {
  // When the user is no longer typing into the text field,
  // clear the temporary value that stores exactly what they typed
  // so that the text field can fall back to the nicely formatted date.
  if (!focused) {
    if (
      internalTextValue.value &&
      !isValid(parseUserInput(internalTextValue.value))
    ) {
      // TODO: i18n
      error.value = [
        'Invalid value. Try formatting like "' +
          format(new Date(), internalFormat.value) +
          '"',
      ];
    } else {
      internalTextValue.value = undefined;
    }
  }
});
</script>
