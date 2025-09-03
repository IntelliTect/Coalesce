<template>
  <span class="c-list-range-display">
    <!-- These use list.$load explicitly so that the numbers match
    the actual currently displayed data, rather than the state of the parameters. -->
    <span class="c-list-range-display--start">
      {{ rangeStart }}
    </span>
    -
    <span class="c-list-range-display--end">
      {{ rangeEnd }}
    </span>

    <template v-if="list.$load.totalCount !== -1">
      of
      <span class="c-list-range-display--total">
        {{ list.$load.totalCount }}
      </span>
    </template>
  </span>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ListViewModel } from "coalesce-vue";

defineOptions({
  name: "c-list-range-display",
});

const props = defineProps<{
  list: ListViewModel;
}>();

const rangeStart = computed((): number => {
  const list = props.list!;
  const page = list.$load.page ?? 0;
  const pageSize = list.$load.pageSize ?? 0;

  if (!list.$load.totalCount) {
    return 0;
  }

  return (page - 1) * pageSize + 1;
});

const rangeEnd = computed((): number => {
  const list = props.list!;

  if (!list.$load.totalCount) {
    return 0;
  }

  return (
    rangeStart.value +
    (list.$modelOnlyMode ? list.$modelItems : list.$items).length -
    1
  );
});
</script>
