
<template>
  <v-menu
    lazy
    :close-on-content-click="false"
    v-model="menu"
    transition="scale-transition"
    offset-y
    min-width="290px"
    full-width>
  <v-text-field
    slot="activator"
    :label="label"
    :value="displayedValue"
    @change="textInputChanged"
  ></v-text-field>
  <v-tabs ref="tabs" v-model="selectedTab" grow icons >
    <v-tab key="date">
    <v-icon>event</v-icon>
    </v-tab>
    <v-tab key="time">
    <v-icon>access_time</v-icon>
    </v-tab>
    <v-tab-item
    key="date">
    <v-date-picker
      :value="datePart"
      scrollable
      actions
      style="padding-bottom: 18px"
      @input="dateChanged"
    ></v-date-picker>
    </v-tab-item>
    <v-tab-item
    key="time">
    <v-time-picker
      :value="timePart"
      scrollable
      format="ampm"
      actions
      @input="timeChanged"
    ></v-time-picker>
    </v-tab-item>
  </v-tabs>
  </v-menu>
</template>


<script lang="ts">
  import * as moment from 'moment';
  import { Vue, Component, Watch, Prop } from 'vue-property-decorator';
  // import CDisplay from './c-display';
  import MetadataComponent from './c-metadata-component'
  import { ModelProperty } from '../core';

  @Component({
  name: 'c-datetime-picker',
  components: {
    // CDisplay
  }
  })
  export default class extends Vue {

  @Prop({required: false})
  public value?: moment.Moment | null ;

  @Prop({type: String})
  public label?: string;

  @Prop({default: 'L LT', type: String}) 
  public dateFormat?: string;

  get displayedValue() {
    return this.value && this.value.format(this.dateFormat) || ''
  }

  
  textInputChanged(val: string) {
    var value = moment(val, this.dateFormat)
    if (!value || !value.isValid()){
      value = moment(this.value || undefined)
    }
    this.$emit('input', value)
  }

  timeChanged(val: string) {
    var value = moment(this.value || undefined)

    var parts = /(\d\d):(\d\d)/.exec(val);
    if (!parts) throw `Time set by vuetify timepicker not in expected format: ${val}`

    value.set({
    'hour': parseInt(parts[1]),
    'minute': parseInt(parts[2]),
    });
    this.$emit('input', value)
  }

  dateChanged(val: string) {
    var value = moment(this.value || undefined)

    var parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
    if (!parts) throw `Date set by vuetify datepicker not in expected format: ${val}`

    value.set({
    'year': parseInt(parts[1]),
    'month': parseInt(parts[2]) - 1,
    'date': parseInt(parts[3])
    });
    this.$emit('input', value)
  }

  menu = false
  selectedTab: "date" | "time" | null = null; // = "date"

  get datePart() { return this.value && this.value.format("YYYY-MM-DD") || null }
  get timePart() { return this.value && this.value.format("HH:mm") || null }
  }
</script>

