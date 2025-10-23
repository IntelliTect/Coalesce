<template>
  <template v-if="creatableTypes.every((t) => !t.route)">
    <!-- do nothing - no create route -->
  </template>
  <v-menu v-if="creatableTypes.length > 1">
    <template #activator="{ props }">
      <v-btn
        v-bind="{ ...$attrs, ...props }"
        class="c-admin-table-toolbar--button-create"
      >
        <v-icon :start="$vuetify.display.mdAndUp">$plus</v-icon>
        <span class="hidden-sm-and-down">{{ label }}</span>
      </v-btn>
    </template>
    <v-list>
      <v-list-item
        v-for="item in creatableTypes"
        :key="item.metadata.name"
        :title="item.metadata.displayName"
        @click="emit('add', item.metadata, item.route!)"
      ></v-list-item>
    </v-list>
  </v-menu>
  <v-btn
    v-else-if="creatableTypes.length == 1"
    v-bind="$attrs"
    class="c-admin-table-toolbar--button-create"
    @click="emit('add', creatableTypes[0].metadata, creatableTypes[0].route!)"
  >
    <v-icon :start="$vuetify.display.mdAndUp">$plus</v-icon>
    <span class="hidden-sm-and-down">{{ label }}</span>
  </v-btn>
</template>

<script setup lang="ts">
import { toRef } from "vue";
import { ListViewModel, ModelType } from "coalesce-vue";
import { useAdminTable } from "./useAdminTable";

defineOptions({ inheritAttrs: false });

const props = defineProps<{
  list: ListViewModel;
  label: string;
}>();

const emit = defineEmits<{
  add: [metadata: ModelType, route: string];
}>();

const { creatableTypes } = useAdminTable(toRef(props, "list"));
</script>

<style lang="scss"></style>
