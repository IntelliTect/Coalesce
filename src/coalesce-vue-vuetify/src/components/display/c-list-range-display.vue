<template>
  <span class="c-list-range-display">
    <!-- These use list.$load explicitly so that the numbers match
    the actual currently displayed data, rather than the state of the parameters. -->
    <span class="c-list-range-display--start">
      {{rangeStart}}
    </span>
    - 
    <span class="c-list-range-display--end">
      {{rangeEnd}}
    </span>
    
    <template v-if="list.$load.totalCount !== -1">
      of
      <span class="c-list-range-display--total">
        {{list.$load.totalCount}}
      </span>
    </template>
  </span>
</template>

<script lang="ts">
import Vue from 'vue'
import type { ListViewModel } from 'coalesce-vue'

export default Vue.extend({
  name: 'c-list-range-display',
  props: {
    list: { required: true, type: Object }
  },
  computed: {
    rangeStart(): string | number | null {
      const list = this.list as ListViewModel;
      const page = list.$load.page ?? 0;
      const pageSize = list.$load.pageSize ?? 0;

      if (!list.$load.totalCount) {
        return 0;
      }

      return (page-1) * pageSize + 1
    },

    rangeEnd(): string | number | null {
      const list = this.list as ListViewModel;
      const page = list.$load.page ?? 0;
      const pageSize = list.$load.pageSize ?? 0;

      if (!list.$load.totalCount) {
        return 0;
      }

      if (page == list.$load.pageCount) {
        // We're on the last page. Cap the range at the total count.
        return list.$load.totalCount;
      }

      return page * pageSize;
    }
  }
})
</script>
