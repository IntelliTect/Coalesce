<template>
  <v-menu
    ref="menu"
    :close-on-content-click="false"
    v-model="menuOpen"
    :nudge-right="40"
    transition="scale-transition"
    offset-y offset-x
    full-width
    min-width="290px"
  >
  
    <template #activator="{ on }">
      <v-text-field
        v-on="on"
        :key="textFieldRerenderKey"
        ref="text"
        :value="displayString"
        @change="textChanged"
        :rules="effectiveRules"
        v-bind="$attrs"
        autocomplete="off"
        :mask="mask"
        return-masked-value
        :error-messages="errorMessages"
        placeholder="mm/dd/yyyy"
        append-icon="fal fa-calendar-alt"
      ></v-text-field>
    </template>
    
    <v-date-picker
      :value="dateString"
      @input="pickerChanged"
      v-bind="pickerProps"
    ></v-date-picker>

  </v-menu>
</template>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { 
  format, parse, isValid, addDays, subDays, isLastDayOfMonth, endOfDay, startOfDay
} from 'date-fns'


@Component({
  components: {
  },
})
export default class DateSelect extends Vue {
  menuOpen = false;

  @Prop({type: Date })
  value!: Date | null;

  @Prop({default: null})
  pickerProps!: any | null

  @Prop({default: null})
  rules!: (() => boolean | string)[] | null

  @Prop({default: null, type: Number})
  discourageUpcomingDays!: number | null;


  get mask() {
    // TODO: mask removed in Vuetify 2.0 https://github.com/vuetifyjs/vuetify/releases#user-content-upgrade-guide
    return null;

    if (/Edge\/|Trident\/|MSIE /.test(window.navigator.userAgent)) {
      // IE has really, really bizarre issues when we use the mask. 
      // You can't blur the text input without entering a value.
      // See #244887 Resource Request: In IE, New Contractor Position Title Cannot Be Set Until End Date is Selected
      return null;
    }
    return "##/##/####"
  }
  get startDate() {
    // Don't use a default start date if a date is already selected.
    if (this.value) return null;
    
    var date = new Date();
    if (isLastDayOfMonth(date)) {
      date = addDays(date, 1);
    }
    return format(date, 'yyyy-MM')
  }

  format = "MM/dd/yyyy";

  // Hack to clear out the input of the text field when it is invalid.
  // $forceUpdate() isn't good enough, for some reason.
  textFieldRerenderKey = 0;

  get errorMessages() {
    const days = this.discourageUpcomingDays;
    if (this.value && days) {
      if (!this.isDateAllowed(this.value)){
        // Don't return this error message if the out-of-range validation fails.
        // Otherwise, this one would take priority, even though it is less important.
        return null
      }
      if (subDays(this.value, days) < endOfDay(new Date())) {
          return [`Request might take at least ${days} days - you are encouraged to pick a later date.`]
      }
    }
    return [];
  }

  isDateAllowed(date: Date | null){
    return this.pickerProps && this.pickerProps.allowedDates && date
        ? (this.pickerProps.allowedDates( format(date, 'yyyy-MM-dd')) ? true : false)
        : true
  }

  get effectiveRules() {
    return [
      this.isDateAllowed(this.value) || 'Date is not in the allowed range',
      ...(this.rules || [])
    ]
  }

  textChanged(value: string) {
    // Base date is noonish, to help avoid timezone adjustment errors
    // that would cause the date to be a day earlier/later than expected.
    var date: Date | null = parse(value, this.format, new Date('2000-01-01T12:00:00-08:00'));

    if (!isValid(date)) {
      date = null;
      this.textFieldRerenderKey++;
      this.$forceUpdate();
    }

    this.$emit("input", date);

    // Force an update next cycle so the validation will run, 
    // the input will clear if the user typed something invalid, etc.
    this.$nextTick(() => {
      (this.$refs.text as any).validate()
    });
    
  }

  pickerChanged(value: string) {
    if (value) {
      this.$emit("input", this.parseString(value));
    }
    this.menuOpen = false;
  }

  get displayString() {
    if (!this.value || !isValid(this.value)) return ""
    return format(this.value, this.format);
  }

  get dateString() {
    if (this.startDate) return this.startDate;

    if (!this.value || !isValid(this.value)) return ""

    return format(this.value, 'yyyy-MM-dd')
  }

  set dateString(val) {
    this.value = this.parseString(val);
  }

  parseString(val: string) {
    const parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
    if (!parts) throw `Date set by vuetify datepicker not in expected format: ${val}`

    const newDate = new Date(this.value || new Date());

    newDate.setDate(1)
    newDate.setFullYear(parseInt(parts[1]))
    newDate.setMonth(parseInt(parts[2]) - 1)
    newDate.setDate(parseInt(parts[3]))
    return newDate;
  }
}
</script>
