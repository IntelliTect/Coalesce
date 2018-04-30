
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
    :error-messages="error"
    append-icon="date_range"
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


<script lang="ts" src="./c-datetime-picker.ts"/>