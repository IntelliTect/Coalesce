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
    :value="displayedValue"
    v-bind="inputBindAttrs"
    :error-messages="error"
    :readonly="isReadonly"
    :disabled="isDisabled"
    autocomplete="off"
    @change="textInputChanged"
    @click:append="menu = !menu"
  ></v-text-field>

  <v-menu
    v-else
    :close-on-content-click="false"
    v-model="menu"
    transition="slide-x-transition"
    offset-y
    offset-x
    :disabled="!interactive"
    min-width="290px"
    content-class="c-datetime-picker--menu"
  >
    <template #activator="{ on }">
      <v-text-field
        class="c-datetime-picker"
        v-on="inputBindListeners(on)"
        v-bind="inputBindAttrs"
        :value="displayedValue"
        :error-messages="error"
        :readonly="isReadonly"
        :disabled="isDisabled"
        autocomplete="off"
        :append-icon="
          internalDateKind == 'time' ? 'fa fa-clock' : 'fa fa-calendar-alt'
        "
        @change="textInputChanged"
        @click:append="if (interactive) menu = !menu;"
      ></v-text-field>
    </template>

    <div v-if="!sideBySide && menu && interactive">
      <v-tabs
        v-model="selectedTab"
        grow
        centered
        color="primary"
        v-show="showDate && showTime"
      >
        <v-tab key="date" v-if="showDate">
          <v-icon left>fa fa-calendar-alt</v-icon> Date
        </v-tab>
        <v-tab key="time" v-if="showTime">
          <v-icon left>fa fa-clock</v-icon> Time
        </v-tab>
      </v-tabs>

      <v-tabs-items v-model="selectedTab">
        <v-tab-item key="date" v-if="showDate" eager>
          <v-date-picker
            color="secondary"
            :value="datePart"
            scrollable
            :allowed-dates="$attrs.allowedDates || $attrs['allowed-dates']"
            @input="dateChanged"
          ></v-date-picker>
        </v-tab-item>
        <v-tab-item key="time" v-if="showTime" eager>
          <v-time-picker
            color="secondary"
            ampm-in-title
            :value="timePart"
            scrollable
            format="ampm"
            :allowed-minutes="
              $attrs.allowedMinutes || $attrs['allowed-minutes']
            "
            :allowed-hours="$attrs.allowedHours || $attrs['allowed-hours']"
            @input="timeChanged"
          ></v-time-picker>
        </v-tab-item>
      </v-tabs-items>
    </div>

    <div v-else-if="sideBySide && menu && interactive" class="d-flex">
      <v-date-picker
        color="secondary"
        :value="datePart"
        scrollable
        :allowed-dates="$attrs.allowedDates || $attrs['allowed-dates']"
        @input="dateChanged"
      ></v-date-picker>
      <v-time-picker
        color="secondary"
        ampm-in-title
        :value="timePart"
        scrollable
        format="ampm"
        :allowed-minutes="$attrs.allowedMinutes || $attrs['allowed-minutes']"
        :allowed-hours="$attrs.allowedHours || $attrs['allowed-hours']"
        @input="timeChanged"
      ></v-time-picker>
    </div>
  </v-menu>
</template>

<style lang="scss">
.c-datetime-picker--menu {
  .v-picker {
    // Remove the border radius. It looks bad since the header is not at the top of a card.
    border-radius: 0;
  }
  .v-picker--date {
    padding-bottom: 10px;
  }
  .v-picker--time .v-picker__title {
    // Make the height of the clock header match the calendar (normally its a bit taller)
    padding-top: 9px;
    padding-bottom: 9px;
  }

  .v-time-picker-title__ampm {
    // Make these look more like the material3 spec,
    // which is WAY more intuitive than the default vuetify2 appearance.
    position: relative;
    margin-bottom: 2px;
    margin-left: 12px;

    border: 1px solid rgba(255, 255, 255, 0.5);
    border-radius: 4px;

    .v-picker__title__btn {
      border-radius: 4px;
      &:first-child {
        border-bottom: 1px solid rgba(255, 255, 255, 0.5);
        border-radius: 4px 4px 0 0;
      }
      &:last-child {
        border-radius: 0 0 4px 4px;
      }
      &.v-picker__title__btn--active {
        background: rgba(0, 0, 0, 0.25);
      }
      padding: 8px;
    }
  }
}
</style>

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
import { getDefaultTimeZone, parseDateUserInput } from "coalesce-vue";
import { defineComponent, PropType } from "vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

export default defineComponent({
  name: "c-datetime-picker",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,

  setup(props) {
    return { ...useMetadataProps(props) };
  },

  props: {
    ...makeMetadataProps(),
    value: { required: false, type: Date as PropType<Date | null | undefined> },
    dateKind: { type: String },
    dateFormat: { type: String },
    readonly: { type: Boolean },
    disabled: { type: Boolean },
    closeOnDatePicked: { type: Boolean, default: null },
    /** Use native HTML5 date picker rather than Vuetify. */
    native: { type: Boolean, default: false },
    timeZone: { type: String, required: false },
    /** Show the calendar and clock side by side in the picker menu. */
    sideBySide: { type: Boolean, default: false },
    // TODO: Formally support allowed-minutes/hours/dates (need to handle them on the text input)
  },

  inject: {
    // Read `readonly` and `disabled` from Vuetify form.
    // TODO: Port to Vue3
    form: { default: null },
  },

  data() {
    return {
      hasMounted: false,
      error: [] as string[],
      menu: false,
      selectedTab: 0,
    };
  },

  computed: {
    isReadonly() {
      return (
        this.readonly ||
        //@ts-expect-error `inject` is not typed in options API
        this.form?.readonly
      );
    },

    isDisabled() {
      return (
        this.disabled ||
        //@ts-expect-error `inject` is not typed in options API
        this.form?.disabled
      );
    },

    interactive() {
      return !this.isReadonly && !this.isDisabled;
    },

    dateMeta() {
      const meta = this.valueMeta;
      if (meta && meta.type == "date") {
        return meta;
      }
      return null;
    },

    displayedValue() {
      if (!this.hasMounted) return null;

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
      if (this.model && this.dateMeta) {
        return (this.model as any)[this.dateMeta.name];
      }

      return this.value;
    },

    internalValueZoned() {
      const value = this.internalValue;
      if (!value || !this.internalTimeZone) return value;
      return utcToZonedTime(value, this.internalTimeZone);
    },

    internalDateKind() {
      if (this.dateKind) return this.dateKind;
      if (this.dateMeta) return this.dateMeta.dateKind;
      return "datetime";
    },

    internalFormat() {
      if (this.native) {
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
    inputBindListeners(on: any) {
      const ret = {
        ...this.$listeners,
        ...on,
      };

      // prevent v-model from getting bound to the text field (via $listeners)
      delete ret.input;

      return ret;
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

    textInputChanged(val: string) {
      this.error = [];
      var value: Date | null | undefined;
      const referenceDate = this.internalValueZoned || this.createDefaultDate();

      if (!val || !val.trim()) {
        value = null;
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
          value = parseDateUserInput(val, referenceDate);
        }

        // If that didn't work, don't change the underlying value. Instead, display an error.
        if (!value || !isValid(value)) {
          // TODO: i18n
          this.error = [
            'Invalid Date. Try formatting like "' +
              format(new Date(), this.internalFormat) +
              '"',
          ];
          value = null;
        }

        if (value && this.internalTimeZone) {
          // The date was parsed against the current browser timeZone.
          // This (poorly named) function will shift it into the desired timezone.
          value = zonedTimeToUtc(value, this.internalTimeZone);
        }
      }

      // Only emit an event if the input isn't invalid.
      // If we don't emit an input event, it gives the user a chance to correct their text.
      if (!this.error.length) this.emitInput(value);
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

    emitInput(value: any) {
      if (this.model && this.dateMeta) {
        (this.model as any)[this.dateMeta.name] = value;
      }

      this.$emit("input", value);
    },

    close() {
      this.menu = false;
    },
  },

  mounted() {
    // Workaround for bug in vuetify where the label displays wrong
    // when the textbox is styled with 'outlined' due to the textbox being the activator for a v-menu.
    // This will delay the render of the date value by a tick,
    // allowing the label to render properly.
    this.$nextTick(() => {
      this.hasMounted = true;
    });
  },
});
</script>
