<template functional>
  <span class="c-list-range-display">
    <!-- These use list.$load explicitly so that the numbers match
    the actual currently displayed data, rather than the state of the parameters. -->
    <span class="c-list-range-display--start">
      {{(props.list.$load.page-1) * props.list.$load.pageSize + 1}}
    </span>
    - 
    <span class="c-list-range-display--end">
      {{props.list.$load.page == props.list.$load.pageCount ? props.list.$load.totalCount : props.list.$load.page * props.list.$load.pageSize}}
    </span>
    
    of
    <span class="c-list-range-display--total">
      {{props.list.$load.totalCount}}
    </span>
  </span>
</template>

<script lang="ts">
import Vue from 'vue'

export default Vue.extend({
  name: 'c-list-range-display',
  props: {
    list: { required: true, type: Object }
  }
})
</script>
