<template>
  <v-toolbar
    extended
    class="c-admin-table-toolbar"
    density="comfortable"
    :color="color"
    elevation="4"
  >
    <v-toolbar-title class="c-admin-table-toolbar--model-name hidden-xs-only">
      {{ metadata.displayName }}
    </v-toolbar-title>

    <v-divider class="hidden-xs-only mx-4" vertical></v-divider>

    <c-admin-create-btn
      :list
      @add="addItem"
      variant="text"
      label="Create"
    ></c-admin-create-btn>

    <v-btn
      class="c-admin-table-toolbar--button-reload"
      variant="text"
      @click="list.$load()"
    >
      <v-icon start>$loading</v-icon>
      <span class="hidden-sm-and-down">Reload</span>
    </v-btn>

    <v-btn
      v-if="editable !== null"
      class="c-admin-table-toolbar--button-editable"
      variant="text"
      @click="editable = !editable"
      :title="editable ? 'Make Read-only' : 'Make Editable'"
    >
      <template v-if="!editable">
        <v-icon start>$edit</v-icon>
        <span class="hidden-sm-and-down">Edit</span>
      </template>
      <template v-else>
        <v-icon start>fa fa-lock</v-icon>
        <span class="hidden-sm-and-down">Lock</span>
      </template>
    </v-btn>

    <v-spacer></v-spacer>

    <span v-if="list" class="c-admin-table-toolbar--range hidden-sm-and-down">
      Showing <c-list-range-display :list="list" />
    </span>

    <v-spacer></v-spacer>

    <c-list-page class="c-admin-table-toolbar--page" :list="list" />

    <template v-slot:extension>
      <v-text-field
        class="c-admin-table-toolbar--search"
        hide-details
        prepend-inner-icon="fa fa-search"
        label="Search"
        v-model="list.$params.search"
        density="comfortable"
        single-line
        clearable
      ></v-text-field>

      <v-divider class="hidden-xs-only mx-4 my-0" vertical></v-divider>

      <c-list-filters
        v-model:selected-columns="selectedColumns"
        :list="list"
        column-selection
      />

      <v-spacer></v-spacer>

      <c-list-page-size :list="list" :items="pageSizes" />
    </template>
  </v-toolbar>
</template>

<script setup lang="ts">
import { PropType, toRef } from "vue";
import { ListViewModel, ModelType, Property } from "coalesce-vue";
import { useAdminTable } from "./useAdminTable";

import CAdminCreateBtn from "./c-admin-create-btn.vue";
import { useRouter } from "vue-router";

const props = defineProps({
  list: { required: true, type: Object as PropType<ListViewModel> },
  pageSizes: { required: false, type: Array as PropType<number[]> },
  color: { required: false, type: String, default: null },
});

const editable = defineModel<boolean>("editable", {
  default: null,
  required: false,
});

const selectedColumns = defineModel<string[] | null>("selectedColumns", {
  required: false,
  default: null,
});

const { metadata } = useAdminTable(toRef(props, "list"));

const router = useRouter();
function addItem(meta: ModelType, route: string) {
  router.push(route);
}
</script>

<style lang="scss">
.c-admin-table-toolbar {
  .c-admin-table-toolbar--model-name {
    flex: 0 1 auto;
  }

  .v-btn {
    min-width: auto;
  }

  // Workaround a vuetify bug where the caret will be white.
  input[type="text"] {
    caret-color: currentColor !important;
  }
  .v-toolbar__extension {
    height: 49px !important;
  }

  .c-list-page,
  .c-list-page-size {
    margin-right: 8px;
  }
}
</style>
