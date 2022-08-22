<template>
  <div class="c-list-pagination grey--text">
    <div>
      <c-list-page-size :list="list" :items="pageSizes" />
    </div>

    <div>
      <c-list-range-display :list="list" />
    </div>

    <div>
      <c-list-page :list="list" />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { ListViewModel } from "coalesce-vue";

export default defineComponent({
  name: "c-list-pagination",

  props: {
    list: { required: true, type: Object as PropType<ListViewModel> },
    pageSizes: { required: false, type: Array as PropType<number[]> },
  },
});
</script>

<style lang="scss" scoped>
.c-list-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 16px;
}
</style>
