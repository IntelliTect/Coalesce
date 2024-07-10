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
    :readonly="readonly"
    :disabled="disabled"
    autocomplete="off"
    v-model:focused="focused"
    @change="textInputChanged($event, true)"
  ></v-text-field>

  <v-text-field
    v-else
    class="c-datetime-picker"
    v-bind="inputBindAttrs"
    :rules="effectiveRules"
    :modelValue="internalTextValue == null ? displayedValue : internalTextValue"
    :error-messages="error"
    :readonly="readonly"
    :disabled="disabled"
    autocomplete="off"
    :placeholder="internalFormat"
    v-model:focused="focused"
    @keydown.enter="focused = false"
    @keydown.escape="focused = false"
    @update:model-value="textInputChanged($event, false)"
    @click.capture="showPickerMobile($event)"
  >
    <template #append-inner>
      <v-icon
        :icon="
          internalDateKind == 'time' ? 'fa fa-clock' : 'fa fa-calendar-alt'
        "
        @click="showPicker"
      ></v-icon>

      <!-- 
        Since vuetify3 doesn't have datepickers, 
        use a native HTML5 date input to provide the calendar and clock input. 
        We don't want to use a native HTML5 input as the root input element, because
        the way they handle events is way too strange and finnicky.
        See https://stackoverflow.com/questions/40762549/html5-input-type-date-onchange-event 
      -->
      <input
        ref="nativeInput"
        style="width: 0px; height: 0; position: absolute"
        tabindex="-1"
        aria-hidden="true"
        :type="
          internalDateKind == 'time'
            ? 'time'
            : internalDateKind == 'date'
            ? 'date'
            : 'datetime-local'
        "
        :value="nativeValue"
        :readonly="readonly"
        :disabled="disabled"
        autocomplete="off"
        @input="textInputChanged($event, true)"
      />
    </template>
  </v-text-field>
</template>

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
} from "date-fns";
import { format, utcToZonedTime, zonedTimeToUtc } from "date-fns-tz";
import {
  getDefaultTimeZone,
  parseDateUserInput,
  DateKind,
  AnyArgCaller,
  Model,
  DateValue,
} from "coalesce-vue";
import { computed, ref } from "vue";
import { ForSpec, useMetadataProps } from "../c-metadata-component";
import { watch } from "vue";

defineOptions({
  name: "c-datetime-picker",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in the autocomplete.
  inheritAttrs: false,
});

const props = withDefaults(
  defineProps<{
    /** An object owning the value to be edited that is specified by the `for` prop. */
    model?: TModel | null;

    /** A metadata specifier for the value being bound. One of:
     * * A string with the name of the value belonging to `model`. E.g. `"startDate"`.
     * * A direct reference to the metadata object. E.g. `model.$metadata.props.startDate`.
     * * A string in dot-notation that starts with a type name. E.g. `"Person.startDate"`.
     */
    for?: ForSpec<TModel, DateValue>;

    dateKind?: DateKind;
    dateFormat?: string;
    readonly?: boolean;
    disabled?: boolean;
    /** Use native HTML5 date picker rather than Vuetify. */
    native?: boolean;
    closeOnDatePicked?: boolean | null;
    timeZone?: string;
  }>(),
  {
    timeZone: getDefaultTimeZone() || undefined,
  }
);

const modelValue = defineModel<Date | null | undefined>();

const { inputBindAttrs, modelMeta, valueMeta, valueOwner } =
  useMetadataProps(props);

const nativeInput = ref<HTMLInputElement>();

const focused = ref(false);
const error = ref<string[]>([]);
const menu = ref(false);
const internalTextValue = ref<string>();

const interactive = computed(() => {
  // TODO: read state from any VForm that wraps us
  return !props.readonly && !props.disabled;
});

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

const internalTimeZone = computed(() => {
  if (props.timeZone) {
    return props.timeZone;
  }
  if (dateMeta.value) {
    if (!dateMeta.value.noOffset) {
      // date is a DateTimeOffset, so TZ conversions are meaningful.
      return getDefaultTimeZone();
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
  return utcToZonedTime(value, internalTimeZone.value);
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

const datePart = computed(() => {
  return (
    (internalValueZoned.value &&
      format(internalValueZoned.value, "yyyy-MM-dd", {
        timeZone: internalTimeZone.value || undefined,
      })) ||
    null
  );
});

const timePart = computed(() => {
  return (
    (internalValueZoned.value &&
      format(internalValueZoned.value, "HH:mm", {
        timeZone: internalTimeZone.value || undefined,
      })) ||
    null
  );
});

function showPickerMobile(event: MouseEvent) {
  if (/android|iphone/i.test(navigator.userAgent)) {
    showPicker(event);
  }
}

function showPicker(event: MouseEvent) {
  // Firefox Desktop only has pickers for date-only inputs.
  // It has no time picker, which makes this essentially useless on firefox for those cases.
  if (
    internalDateKind.value !== "date" &&
    /firefox/i.test(navigator.userAgent) &&
    !/android|iphone/i.test(navigator.userAgent)
  ) {
    return;
  }

  if (nativeInput.value?.showPicker) {
    nativeInput.value.showPicker();
    event.stopPropagation();
    event.preventDefault();

    // Immediately unfocus the input element,
    // because vuetify will have focused it in its own handler.
    // On mobile, we don't want the mobile keyboard to flash open for a few milliseconds,
    // and on desktop when the native picker is open it intercepts ALL keyboard input,
    // so having the text field is deceptive for the user.
    (document.activeElement as HTMLElement)?.blur?.();

    return true;
  }
}

function createDefaultDate() {
  const date = new Date();
  if (props.dateKind == "date") {
    const zone = internalTimeZone.value;
    if (!zone) return startOfDay(date);

    return zonedTimeToUtc(startOfDay(utcToZonedTime(date, zone)), zone);
  }
  return date;
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
      value = zonedTimeToUtc(value, internalTimeZone.value);
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

function timeChanged(val: string) {
  error.value = [];

  var value = internalValueZoned.value || createDefaultDate();

  var parts = /(\d\d):(\d\d)/.exec(val);
  if (!parts)
    throw `Time set by vuetify timepicker not in expected format: ${val}`;

  value = setHours(value, parseInt(parts[1]));
  value = setMinutes(value, parseInt(parts[2]));

  if (internalTimeZone.value) {
    value = zonedTimeToUtc(value, internalTimeZone.value);
  }

  emitInput(value);
}

function dateChanged(val: string) {
  error.value = [];

  var value = internalValueZoned.value || createDefaultDate();

  var parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
  if (!parts)
    throw `Date set by vuetify datepicker not in expected format: ${val}`;

  // Reset this first in case the year/month aren't valid for the current day.
  value = setDate(value, 1);

  value = setYear(value, parseInt(parts[1]));
  value = setMonth(value, parseInt(parts[2]) - 1);
  value = setDate(value, parseInt(parts[3]));

  if (internalTimeZone.value) {
    value = zonedTimeToUtc(value, internalTimeZone.value);
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
