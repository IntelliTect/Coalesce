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

<script lang="ts">
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
import { getDefaultTimeZone, parseDateUserInput, DateKind } from "coalesce-vue";
import { defineComponent, PropType, ref } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

export default defineComponent({
  name: "c-datetime-picker",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in the autocomplete.
  inheritAttrs: false,

  setup(props) {
    return { ...useMetadataProps(props), nativeInput: ref() };
  },

  props: {
    ...makeMetadataProps(),
    modelValue: {
      required: false,
      type: Date as PropType<Date | null | undefined>,
    },
    dateKind: { type: String as PropType<DateKind | null | undefined> },
    dateFormat: { type: String },
    readonly: { type: Boolean },
    disabled: { type: Boolean },
    closeOnDatePicked: { type: Boolean, default: null },
    /** Use native HTML5 date picker rather than Vuetify. */
    native: { type: Boolean, default: false },
    timeZone: { type: String, default: () => getDefaultTimeZone() },
  },

  data() {
    return {
      focused: false,
      error: [] as string[],
      menu: false,
      selectedTab: 0,
      internalTextValue: null as string | null,
    };
  },

  watch: {
    focused(focused) {
      // When the user is no longer typing into the text field,
      // clear the temporary value that stores exactly what they typed
      // so that the text field can fall back to the nicely formatted date.
      if (!focused) {
        if (
          this.internalTextValue &&
          !isValid(this.parseUserInput(this.internalTextValue))
        ) {
          // TODO: i18n
          this.error = [
            'Invalid value. Try formatting like "' +
              format(new Date(), this.internalFormat) +
              '"',
          ];
        } else {
          this.internalTextValue = null;
        }
      }
    },
  },

  computed: {
    interactive() {
      return !this.readonly && !this.disabled;
    },

    dateMeta() {
      const meta = this.valueMeta;
      if (meta && meta.type == "date") {
        return meta;
      }
      return null;
    },

    /** The value to bind to the `value` of a native HTML5 date input */
    nativeValue() {
      return this.internalValueZoned
        ? format(this.internalValueZoned, this.nativeInternalFormat, {
            timeZone: this.internalTimeZone || undefined,
          })
        : null;
    },

    displayedValue() {
      return this.internalValueZoned
        ? format(this.internalValueZoned, this.internalFormat, {
            timeZone: this.internalTimeZone || undefined,
          })
        : null;
    },

    internalTimeZone() {
      if (this.timeZone) {
        return this.timeZone;
      }
      if (this.dateMeta) {
        if (!this.dateMeta.noOffset) {
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
    },

    internalValue() {
      if (this.valueOwner && this.dateMeta) {
        return (this.valueOwner as any)[this.dateMeta.name];
      }

      return this.modelValue;
    },

    internalValueZoned() {
      const value = this.internalValue;
      if (!value || !this.internalTimeZone) return value;
      return utcToZonedTime(value, this.internalTimeZone);
    },

    internalDateKind(): DateKind {
      if (this.dateKind) return this.dateKind;
      if (this.dateMeta) return this.dateMeta.dateKind;
      return "datetime";
    },

    nativeInternalFormat() {
      // These are the formats expected by HTMLInputElement.value
      // when its type is one of the native date/time types.
      switch (this.internalDateKind) {
        case "date":
          return "yyyy-MM-dd";
        case "time":
          return "HH:mm";
        case "datetime":
        default:
          return "yyyy-MM-dd'T'HH:mm";
      }
    },

    internalFormat() {
      if (this.native) {
        return this.nativeInternalFormat;
      }

      if (this.dateFormat) return this.dateFormat;
      switch (this.internalDateKind) {
        case "date":
          return "M/d/yyyy";
        case "time":
          return "h:mm a";
        case "datetime":
        default:
          return "M/d/yyyy h:mm a";
      }
    },

    showDate() {
      return (
        this.internalDateKind == "datetime" || this.internalDateKind == "date"
      );
    },

    showTime() {
      return (
        this.internalDateKind == "datetime" || this.internalDateKind == "time"
      );
    },

    datePart() {
      return (
        (this.internalValueZoned &&
          format(this.internalValueZoned, "yyyy-MM-dd", {
            timeZone: this.internalTimeZone || undefined,
          })) ||
        null
      );
    },

    timePart() {
      return (
        (this.internalValueZoned &&
          format(this.internalValueZoned, "HH:mm", {
            timeZone: this.internalTimeZone || undefined,
          })) ||
        null
      );
    },
  },

  methods: {
    showPickerMobile(event: MouseEvent) {
      if (/android|iphone/i.test(navigator.userAgent)) {
        this.showPicker(event);
      }
    },

    showPicker(event: MouseEvent) {
      // Firefox Desktop only has pickers for date-only inputs.
      // It has no time picker, which makes this essentially useless on firefox for those cases.
      if (
        this.internalDateKind !== "date" &&
        /firefox/i.test(navigator.userAgent) &&
        !/android|iphone/i.test(navigator.userAgent)
      ) {
        return;
      }

      if (this.nativeInput?.showPicker) {
        this.nativeInput.showPicker();
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
    },

    createDefaultDate() {
      const date = new Date();
      if (this.dateKind == "date") {
        const zone = this.internalTimeZone;
        if (!zone) return startOfDay(date);

        return zonedTimeToUtc(startOfDay(utcToZonedTime(date, zone)), zone);
      }
      return date;
    },

    parseUserInput(val: string) {
      var value: Date | null;
      const referenceDate = this.internalValueZoned || this.createDefaultDate();

      if (!val || !val.trim()) {
        return null;
      } else {
        value = parse(val, this.internalFormat, referenceDate);

        // If failed, try normalizing common separators to the same symbol in
        // both the format string and user input.
        if (!isValid(value)) {
          const separatorRegex = /[\-\\\/\.]/g;
          value = parse(
            val.replace(separatorRegex, "-"),
            this.internalFormat.replace(separatorRegex, "-"),
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
          if (this.native) {
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
            parseDateUserInput(val, referenceDate, this.internalDateKind) ??
            null;
        }

        if (value && this.internalTimeZone) {
          // The date was parsed against the current browser timeZone.
          // This (poorly named) function will shift it into the desired timezone.
          value = zonedTimeToUtc(value, this.internalTimeZone);
        }
      }

      return value;
    },

    textInputChanged(val: string | Event, isNative: boolean) {
      if (val instanceof Event) {
        val = (val.target as HTMLInputElement)?.value;
      }

      this.error = [];

      if (val == "" || val == null) {
        // Emptystring is emitted when the user clicks "clear" in the date picker popup,
        // or if they delete all characters from the input.
        this.internalTextValue = "";
        this.emitInput(null);
        return;
      }

      var value: Date | null;
      const referenceDate = this.internalValueZoned || this.createDefaultDate();

      if (isNative) {
        value = parse(val, this.nativeInternalFormat, referenceDate);
      } else {
        // Capture the user's intermediate text input
        this.internalTextValue = val;
        value = this.parseUserInput(val);
      }

      // Only emit an event if the input isn't invalid.
      // If we don't emit an input event, it gives the user a chance to correct their text.
      if (isValid(value)) this.emitInput(value);
    },

    timeChanged(val: string) {
      this.error = [];

      var value = this.internalValueZoned || this.createDefaultDate();

      var parts = /(\d\d):(\d\d)/.exec(val);
      if (!parts)
        throw `Time set by vuetify timepicker not in expected format: ${val}`;

      value = setHours(value, parseInt(parts[1]));
      value = setMinutes(value, parseInt(parts[2]));

      if (this.internalTimeZone) {
        value = zonedTimeToUtc(value, this.internalTimeZone);
      }

      this.emitInput(value);
    },

    dateChanged(val: string) {
      this.error = [];

      var value = this.internalValueZoned || this.createDefaultDate();

      var parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
      if (!parts)
        throw `Date set by vuetify datepicker not in expected format: ${val}`;

      // Reset this first in case the year/month aren't valid for the current day.
      value = setDate(value, 1);

      value = setYear(value, parseInt(parts[1]));
      value = setMonth(value, parseInt(parts[2]) - 1);
      value = setDate(value, parseInt(parts[3]));

      if (this.internalTimeZone) {
        value = zonedTimeToUtc(value, this.internalTimeZone);
      }

      this.emitInput(value);

      // If closeOnDatePicked isn't specified, auto-close if only picking a date.
      // Otherwise, respect closeOnDatePicked.
      if (
        this.closeOnDatePicked == null
          ? this.internalDateKind == "date"
          : this.closeOnDatePicked
      ) {
        this.close();
      }
    },

    emitInput(value: Date | null) {
      if (this.valueOwner && this.dateMeta) {
        (this.valueOwner as any)[this.dateMeta.name] = value;
      }

      if (this.modelValue != value) {
        this.$emit("update:modelValue", value);
      }
    },

    close() {
      this.menu = false;
    },
  },
});
</script>
