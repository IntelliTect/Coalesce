<template>
  <v-sheet class="c-time-picker" ref="root">
    <v-sheet color="secondary" class="c-time-picker-header">
      <slot name="header">
        {{ isValid(modelValue) ? format(modelValue!, "h:mm a") : "- : - -" }}
      </slot>
    </v-sheet>
    <div class="c-time-picker__columns">
      <div class="c-time-picker__column" aria-label="Hour">
        <button
          v-for="i in hours"
          :key="'h-' + i"
          @click="setHour(i)"
          :disabled="!testHourValid(i)"
          tabindex="0"
          class="c-time-picker__item c-time-picker__item-hour"
          :class="{
            'c-time-picker__item-active ':
              modelValue && modelValue.getHours() % 12 == i % 12,
          }"
        >
          {{ i }}
        </button>
      </div>
      <div class="c-time-picker__column" aria-label="Minute">
        <button
          v-for="i in minutes"
          :key="'m-' + i"
          @click="setMinute(i)"
          :disabled="!testValid({ minutes: i })"
          class="c-time-picker__item c-time-picker__item-minute"
          :class="{
            'c-time-picker__item-active ': modelValue?.getMinutes() == i,
          }"
        >
          {{ i.toString().padStart(2, "0") }}
        </button>
      </div>
      <div class="c-time-picker__column" aria-label="AM/PM">
        <button
          v-for="(item, i) in meridiems"
          :key="item"
          class="c-time-picker__item c-time-picker__item-meridiam"
          @click="setAM(i == 0)"
          :disabled="!testMeridiamValid(i == 0)"
          :class="{
            'c-time-picker__item-active':
              modelValue && (i == 0) == modelValue.getHours() < 12,
          }"
        >
          {{ item }}
        </button>
      </div>
    </div>
  </v-sheet>
</template>

<style lang="scss">
.c-time-picker {
  max-height: calc(100% - 64px);
}

.c-time-picker-header {
  padding: 6px 14px;
  font-size: 32px;
  line-height: 40px;
}

.c-time-picker__columns {
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
  user-select: none;

  &:disabled {
    opacity: 0.2;
  }
  &:not(:disabled):not(.c-time-picker__item-active) {
    &:hover {
      background: rgba(var(--v-theme-on-surface), 0.1);
    }
  }

  &.c-time-picker__item-active {
    color: rgb(var(--v-theme-on-secondary));
    background: rgb(var(--v-theme-secondary));
  }
}
</style>

<script setup lang="ts">
import {
  format,
  getHours,
  setHours,
  setMinutes,
  startOfHour,
  isValid,
  set,
  DateValues,
  endOfHour,
  isSameDay,
} from "date-fns";
import {
  ComponentPublicInstance,
  computed,
  nextTick,
  onMounted,
  ref,
  watch,
} from "vue";

const props = withDefaults(
  defineProps<{
    modelValue?: Date | null;
    /** The increments, in minutes, of the selectable value.
     * Values should divide 60 evenly, or be multiples of 60 */
    step: number;
    min?: Date | null;
    max?: Date | null;
  }>(),
  { step: 1 }
);

const emit = defineEmits<{ "update:modelValue": [arg: Date] }>();

const root = ref<ComponentPublicInstance>();

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

function testValid(parts: DateValues) {
  let value = props.modelValue;
  if (!value || (!props.min && !props.max)) return true;

  value = set(value, parts);
  if (props.min && value < props.min) {
    return false;
  }
  if (props.max && value > props.max) {
    return false;
  }
  return true;
}

function testMeridiamValid(isAM: boolean) {
  let value = props.modelValue;
  if (!value || (!props.min && !props.max)) return true;

  if (props.min && isSameDay(props.min, value) && props.min.getHours() >= 12) {
    // We've selected the minimum date, and the minimum only allows afternoon.
    // Disable AM.
    return isAM == false;
  }

  if (props.max && isSameDay(props.max, value) && props.max.getHours() < 12) {
    // We've selected the max date, and the max only allows morning.
    // Disable PM.
    return isAM == true;
  }
  return true;
}

function testHourValid(hour: number) {
  let value = props.modelValue;
  if (!value || (!props.min && !props.max)) return true;

  hour = (hour % 12) + Math.floor(getHours(value) / 12) * 12;
  value = set(value, { hours: hour, minutes: 0, seconds: 0, milliseconds: 0 });

  // We test the min value against the end of the hour so that the
  // allowed minutes within the cutoff hour can still be selected.
  if (props.min && endOfHour(value) < props.min) {
    return false;
  }
  // And likewise, we test the max against the start of the hour
  if (props.max && startOfHour(value) > props.max) {
    return false;
  }
  return true;
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

onMounted(() => {
  watch(
    [root, () => props.modelValue && format(props.modelValue, "h:mm a")],
    async ([root], [_, oldTime]) => {
      const rootEl: HTMLElement = root?.$el;
      if (!rootEl) return;

      await nextTick();

      const noAnimation =
        // Don't animate when picking a value for the first time.
        !oldTime ||
        // Don't animate for users who don't want animation
        window.matchMedia(`(prefers-reduced-motion: reduce)`)?.matches === true;

      // Scroll the selected numbers into view in the time picker.
      // This is an interval because it has to keep updating as the modal
      // animates in and grows in height.
      rootEl.querySelectorAll(".c-time-picker__item-active").forEach((el) => {
        const totalDuration = 350; // ms
        let scrollTarget = 0;
        const start = new Date().valueOf();
        const parent = el.parentElement;
        if (!parent) return;

        //@ts-expect-error
        clearInterval(parent._scrollAnimInterval);
        //@ts-expect-error
        const interval = (parent._scrollAnimInterval = setInterval(() => {
          const rect = el.getBoundingClientRect();
          const parentRect = parent.getBoundingClientRect();

          // The top of the item relative to the top of the first item in the list.
          const topInScrollFrame = rect.top - parentRect.top + parent.scrollTop;

          scrollTarget =
            topInScrollFrame - parent?.clientHeight / 2 + el.clientHeight / 2;
          scrollTarget = Math.min(scrollTarget, parent.scrollHeight);
          scrollTarget = Math.max(scrollTarget, 0);

          parent.scrollTop = noAnimation
            ? scrollTarget
            : lerp(
                parent.scrollTop,
                scrollTarget,
                (new Date().valueOf() - start) / totalDuration
              );
        }, 15));
        setTimeout(() => clearInterval(interval), totalDuration);
      });
    },
    { immediate: true }
  );
});

function lerp(a: number, b: number, alpha: number) {
  return a + alpha * (b - a);
}
</script>
