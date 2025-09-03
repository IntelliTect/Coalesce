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

<script setup lang="ts">
import { ListViewModel } from "coalesce-vue";

defineOptions({
  name: "c-list-pagination",
});

defineProps<{
  list: ListViewModel;
  pageSizes?: number[];
}>();
</script>

<style lang="scss">
.c-list-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 16px;
}
</style>
