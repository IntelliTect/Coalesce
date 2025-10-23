<template>
  <div class="c-list-page-size">
    <span class="d-none d-sm-inline-block"> Page Size </span>

    <!-- Title on its own element because vuetify doesn't 
    seem to want to accept a title on v-select? -->
    <span title="Current Page Size">
      <!-- Vuetify is glitchy when removing/adding a label dynamically,
      so we render two different components - one with the label (small screens), and one without (large screens). -->
      <v-select
        v-if="$vuetify.display.smAndUp"
        key="no-label"
        v-bind="selectBinds"
        v-model="list.$params.pageSize"
      ></v-select>
      <v-select
        v-else
        key="with-label"
        v-bind="selectBinds"
        v-model="list.$params.pageSize"
        label="Page Size"
      ></v-select>
    </span>
  </div>
</template>

<script setup lang="ts">
import { ListViewModel } from "coalesce-vue";
import { computed } from "vue";

defineOptions({
  name: "c-list-page-size",
});

const props = withDefaults(
  defineProps<{
    list: ListViewModel;
    items?: number[];
  }>(),
  {
    items: () => [10, 25, 100],
  },
);

const selectBinds = computed((): any => {
  return {
    variant: "outlined",
    "hide-details": true,
    density: "compact",
    items: props.items,
    class: "c-list-page-size--dropdown",
  };
});
</script>

<style lang="scss">
.c-list-page-size {
  display: flex;
  align-items: center;
  font-size: 16px;

  > * {
    margin: 0 0.5em !important;
  }

  .c-list-page-size--dropdown {
    max-width: 110px;

    .v-select__selections {
      padding: 0 !important;
    }

    .v-input__append-inner {
      margin-top: 2px !important;
    }

    .v-input__slot {
      height: unset !important;
      min-height: unset !important;
    }
  }
}
</style>
