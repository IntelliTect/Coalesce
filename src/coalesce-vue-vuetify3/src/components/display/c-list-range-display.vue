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

<script lang="ts">
import { defineComponent, PropType } from "vue";
import type { ListViewModel } from "coalesce-vue";

export default defineComponent({
  name: "c-list-range-display",
  props: {
    list: { required: true, type: Object as PropType<ListViewModel> },
  },
  computed: {
    rangeStart(): number {
      const list = this.list!;
      const page = list.$load.page ?? 0;
      const pageSize = list.$load.pageSize ?? 0;

      if (!list.$load.totalCount) {
        return 0;
      }

      return (page - 1) * pageSize + 1;
    },

    rangeEnd(): number {
      const list = this.list!;

      if (!list.$load.totalCount) {
        return 0;
      }

      return (
        this.rangeStart +
        (list.$modelOnlyMode ? list.$modelItems : list.$items).length -
        1
      );
    },
  },
});
</script>
