<template>
  <div class="c-time-picker">
    <div class="c-time-picker__column">
      <div class="c-time-picker__column-items" aria-label="Hour">
        <div
          v-for="i in hours"
          :key="'h-' + i"
          @click="setHour(i)"
          role="button"
          class="c-time-picker__item c-time-picker__item-hour"
          :class="{
            'c-time-picker__item-active ':
              model && model.getHours() % 12 == i % 12,
          }"
        >
          {{ i.toString() }}
        </div>
      </div>
    </div>
    <div class="c-time-picker__column">
      <div class="c-time-picker__column-items" aria-label="Minute">
        <div
          v-for="i in minutes"
          :key="'m-' + i"
          @click="setMinute(i)"
          role="button"
          class="c-time-picker__item c-time-picker__item-minute"
          :class="{
            'c-time-picker__item-active ': model?.getMinutes() == i,
          }"
        >
          {{ i.toString().padStart(2, "0") }}
        </div>
      </div>
    </div>
    <div class="c-time-picker__column">
      <div class="c-time-picker__column-items" aria-label="AM/PM">
        <div
          v-for="item in ['AM', 'PM']"
          :key="item"
          class="c-time-picker__item c-time-picker__item-meridiam"
          @click="setAM(item == 'AM')"
          role="button"
          :class="{
            'c-time-picker__item-active ':
              model && (item == 'AM') == model.getHours() < 12,
          }"
        >
          {{ item }}
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss">
.c-time-picker {
  display: flex;
}
.c-time-picker__column {
  margin: 8px 0px;
  text-align: center;
}
.c-time-picker__column-items {
  max-height: 320px;
  overflow-y: auto;
  overflow-x: hidden;
  margin: 4px;
  border-radius: 8px;
  background-color: rgba(var(--v-theme-surface), 0.3);

  &::-webkit-scrollbar {
    width: 6px;
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
  padding: 6px 14px;
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
import { getHours, setHours, setMinutes } from "date-fns";
import { computed } from "vue";

const model = defineModel<Date>();

const props = withDefaults(
  defineProps<{
    /** The increments, in minutes, of the selectable value.
     * Values should divide 60 evenly, or be multiples of 60 */
    step: number;
  }>(),
  { step: 1 }
);

const hours = computed(() => {
  return [12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11].filter(
    (h) => props.step <= 60 || (h * 60) % props.step == 0
  );
});

const minutes = computed(() => {
  return Array.from(Array(60).keys()).filter((m) => m % props.step == 0);
});

function setMinute(minute: number) {
  model.value = setMinutes(model.value, minute);
}

function setHour(hour: number) {
  hour = (hour % 12) + Math.floor(getHours(model.value) / 12) * 12;
  model.value = setHours(model.value, hour);
}

function setAM(isAM: boolean) {
  const hour = (getHours(model.value) % 12) + (isAM ? 0 : 12);
  model.value = setHours(model.value, hour);
}
</script>
