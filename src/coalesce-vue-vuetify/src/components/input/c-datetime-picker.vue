<template>
  <v-menu
    :close-on-content-click="false"
    v-model="menu"
    transition="slide-x-transition"
    offset-y
    offset-x
    :disabled="disabled"
    min-width="290px"
  >
    <template #activator="{ on }">
      <v-text-field
        v-on="inputBindListeners(on)"
        v-bind="inputBindAttrs"
        :value="displayedValue"
        :error-messages="error"
        :readonly="readonly"
        :disabled="disabled"
        autocomplete="off"
        :append-icon="
          internalDateKind == 'time' ? 'fa fa-clock' : 'fa fa-calendar-alt'
        "
        @change="textInputChanged"
        @click:append="menu = !menu"
      ></v-text-field>
    </template>

    <div v-if="menu && interactive">
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
        <v-tab-item key="date" v-if="showDate">
          <v-date-picker
            color="secondary"
            :value="datePart"
            scrollable
            actions
            style="padding-bottom: 18px"
            @input="dateChanged"
          ></v-date-picker>
        </v-tab-item>
        <v-tab-item key="time" v-if="showTime">
          <v-time-picker
            color="secondary"
            ampm-in-title
            :value="timePart"
            scrollable
            format="ampm"
            actions
            @input="timeChanged"
          ></v-time-picker>
        </v-tab-item>
      </v-tabs-items>
    </div>
  </v-menu>
</template>

<script lang="ts">
/*
  This component is designed to work either with <... model="model" for="for" /> or with <... v-model="value" />
*/

// Tedious imports for maximum tree shaking
import {
  isValid,
  format,
  parse,
  setYear,
  setMonth,
  setDate,
  setHours,
  setMinutes,
  lightFormat,
  startOfDay
} from "date-fns";

import { Vue, Component, Watch, Prop } from "vue-property-decorator";
import MetadataComponent from "../c-metadata-component";
import { parseDateUserInput } from "coalesce-vue";

@Component({
  name: "c-datetime-picker",
  components: {}
})
export default class extends MetadataComponent {
  @Prop({ required: false, type: Date })
  public value?: Date | null;

  @Prop({ type: String })
  public dateKind?: string;

  @Prop({ type: String })
  public dateFormat?: string;

  @Prop({ type: Boolean })
  public readonly?: boolean;

  @Prop({ type: Boolean })
  public disabled?: boolean;

  @Prop({ type: Boolean, default: null })
  public closeOnDatePicked?: boolean | null;

  inputBindListeners(on: any) {
    const ret = {
      ...this.$listeners,
      ...on,
    };
    
    // prevent v-model from getting bound to the text field (via $listeners)
    delete ret.input;

    return ret;
  }

  get interactive() {
    return !this.readonly && !this.disabled;
  }

  get dateMeta() {
    const meta = this.valueMeta;
    if (meta && meta.type == "date") {
      return meta;
    }
    return null;
  }

  get displayedValue() {
    if (!this.hasMounted) return null;

    return this.value ? format(this.value, this.internalFormat) : null;
  }

  get internalDateKind() {
    if (this.dateKind) return this.dateKind;
    if (this.dateMeta) return this.dateMeta.dateKind;
    return "datetime";
  }

  get internalFormat() {
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
  }

  get showDate() {
    return (
      this.internalDateKind == "datetime" || this.internalDateKind == "date"
    );
  }

  get showTime() {
    return (
      this.internalDateKind == "datetime" || this.internalDateKind == "time"
    );
  }

  hasMounted = false;
  mounted() {
    // Workaround for bug in vuetify where the label displays wrong
    // when the textbox is styled with 'outlined' due to the textbox being the activator for a v-menu.
    // This will delay the render of the date value by a tick,
    // allowing the label to render properly.
    this.$nextTick(() => {
      this.hasMounted = true;
    });
  }

  error: string[] = [];

  createDefaultDate() {
    const date = new Date();
    if (this.dateKind == "date") {
      return startOfDay(date);
    }
    return date;
  }

  textInputChanged(val: string) {
    this.error = [];
    var value: Date | null | undefined;
    if (!val || !val.trim()) {
      value = null;
    } else {
      value = parse(
        val,
        this.internalFormat,
        startOfDay(this.createDefaultDate())
      );

      // If failed, try normalizing common separators to the same symbol in
      // both the format string and user input.
      if (!isValid(value)) {
        const separatorRegex = /[\-\\\/\.]/g;
        value = parse(
          val.replace(separatorRegex, "-"),
          this.internalFormat.replace(separatorRegex, "-"),
          startOfDay(this.createDefaultDate())
        );
      }

      // If the input didn't match our format exactly,
      // try parsing user input with general formatting interpretation (trying to be a good citizen).
      // DO NOT do this if the input doesn't have a date part.
      // Behavior of new Date() is generally always Invalid Date if you just give it a time,
      // except if you're on Chrome and give it an invalid time like "8:98 AM" - it'll give you "Thu Jan 01 1998 08:00:00".
      // Since the user wouldn't ever see the date part when only entering a time, there's no chance to detect this error.
      if (
        (!isValid(value) ||
          // A year less than 100(0?) is also invalid.
          // This means that the format for the year was "yyyy",
          // but the user only entered "yy" (or entered 3 digits by accident, hence checking 1000 instead of 100).
          value.getFullYear() <= 1000) &&
        this.internalFormat != "time"
      ) {
        value = parseDateUserInput(val, this.createDefaultDate());
      }

      // If that didn't work, don't change the underlying value. Instead, display an error.
      if (!value || !isValid(value)) {
        // TODO: i18n
        this.error = [
          'Invalid Date. Try formatting like "' +
            format(new Date(), this.internalFormat) +
            '"'
        ];
        value = null;
      }
    }

    // Only emit an event if the input isn't invalid.
    // If we don't emit an input event, it gives the user a chance to correct their text.
    if (!this.error.length) this.emitInput(value);
  }

  timeChanged(val: string) {
    this.error = [];

    var value = this.value || this.createDefaultDate();

    var parts = /(\d\d):(\d\d)/.exec(val);
    if (!parts)
      throw `Time set by vuetify timepicker not in expected format: ${val}`;

    value = setHours(value, parseInt(parts[1]));
    value = setMinutes(value, parseInt(parts[2]));

    this.emitInput(value);
  }

  dateChanged(val: string) {
    this.error = [];

    var value = this.value || this.createDefaultDate();

    var parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
    if (!parts)
      throw `Date set by vuetify datepicker not in expected format: ${val}`;

    // Reset this first in case the year/month aren't valid for the current day.
    value = setDate(value, 1);

    value = setYear(value, parseInt(parts[1]));
    value = setMonth(value, parseInt(parts[2]) - 1);
    value = setDate(value, parseInt(parts[3]));

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
  }

  private emitInput(value: any) {
    if (this.model && this.dateMeta) {
      return ((this.model as any)[this.dateMeta.name] = value);
    }

    this.$emit("input", value);
  }

  public close() {
    this.menu = false;
  }

  menu = false;
  selectedTab: number = 0;

  get datePart() {
    return (this.value && lightFormat(this.value, "yyyy-MM-dd")) || null;
  }
  get timePart() {
    return (this.value && lightFormat(this.value, "HH:mm")) || null;
  }
}
</script>
