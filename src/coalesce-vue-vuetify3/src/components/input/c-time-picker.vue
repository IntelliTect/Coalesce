<template>
  <v-sheet class="c-time-picker">
    <div class="c-time-picker__column" aria-label="Hour">
      <button
        v-for="i in hours"
        :key="'h-' + i"
        @click="setHour(i)"
        role="button"
        tabindex="0"
        class="c-time-picker__item c-time-picker__item-hour"
        :class="{
          'c-time-picker__item-active ':
            modelValue && modelValue.getHours() % 12 == i % 12,
        }"
      >
        {{ i.toString() }}
      </button>
    </div>
    <div class="c-time-picker__column" aria-label="Minute">
      <button
        v-for="i in minutes"
        :key="'m-' + i"
        @click="setMinute(i)"
        role="button"
        class="c-time-picker__item c-time-picker__item-minute"
        :class="{
          'c-time-picker__item-active ': modelValue?.getMinutes() == i,
        }"
      >
        {{ i.toString().padStart(2, "0") }}
      </button>
    </div>
    <div class="c-time-picker__column" aria-label="AM/PM">
      <div
        v-for="(item, i) in meridiems"
        :key="item"
        class="c-time-picker__item c-time-picker__item-meridiam"
        @click="setAM(i == 0)"
        role="button"
        :class="{
          'c-time-picker__item-active':
            modelValue && (i == 0) == modelValue.getHours() < 12,
        }"
      >
        {{ item }}
      </div>
    </div>
  </v-sheet>
</template>

<style lang="scss">
.c-time-picker {
  display: flex;
  padding: 8px;
  justify-content: space-around;
}
.c-time-picker__column {
  max-height: 328px;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 8px 9px;
  border-radius: 8px;
  background-color: rgba(var(--v-theme-surface), 0.3);

  &::-webkit-scrollbar {
    width: 4px;
  }

  &::-webkit-scrollbar-track {
    border-radius: 8px;
    border: 1px solid rgba(var(--v-theme-on-surface), 0.15);
    box-shadow: inset 0 0 6px rgba(var(--v-theme-on-surface), 0.2);
  }

  &::-webkit-scrollbar-thumb {
    border-radius: 8px;
    background-color: rgba(var(--v-theme-on-surface), 0.3);
  }
}
.c-time-picker__item {
  display: block;
  padding: 5px 14px;
  min-width: 46px;
  margin: 0 4px;
  text-align: center;
  text-justify: center;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  border-radius: 8px;
  cursor: pointer;

  &:hover {
    background: rgba(var(--v-theme-on-surface), 0.1);
  }
  &.c-time-picker__item-active {
    color: rgb(var(--v-theme-on-secondary));
    background: rgb(var(--v-theme-secondary));
  }
}
</style>

<script setup lang="ts">
import { format, getHours, setHours, setMinutes, startOfHour } from "date-fns";
import { computed } from "vue";

const props = withDefaults(
  defineProps<{
    modelValue?: Date | null;
    /** The increments, in minutes, of the selectable value.
     * Values should divide 60 evenly, or be multiples of 60 */
    step: number;
  }>(),
  { step: 1 }
);

const emit = defineEmits<{ "update:modelValue": [arg: Date] }>();

const hours = computed(() => {
  return [12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11].filter(
    (h) => props.step <= 60 || (h * 60) % props.step == 0
  );
});

const minutes = computed(() => {
  return Array.from(Array(60).keys()).filter((m) => m % props.step == 0);
});

const meridiems = [
  format(new Date(0).setHours(0), "a"),
  format(new Date(0).setHours(12), "a"),
];

function getDateToModify() {
  return props.modelValue ?? startOfHour(new Date());
}

function setMinute(minute: number) {
  const value = getDateToModify();
  emitInput(setMinutes(value, minute));
}

function setHour(hour: number) {
  const value = getDateToModify();
  hour = (hour % 12) + Math.floor(getHours(value) / 12) * 12;
  emitInput(setHours(value, hour));
}

function setAM(isAM: boolean) {
  const value = getDateToModify();
  const hour = (getHours(value) % 12) + (isAM ? 0 : 12);
  emitInput(setHours(value, hour));
}

function emitInput(value: Date) {
  emit("update:modelValue", value);
}
</script>
