<template>
  <v-container style="max-width: 900px">
    <v-card>
      <v-card-title class="d-flex">
        Coalesce Example
        <v-spacer />
        <v-btn to="/admin/Widget" color="primary"> Widget Admin Table </v-btn>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-form ref="form">
          Below is a very simple example of using components from
          coalesce-vue-vuetify to display and edit properties of a model.
          Autosave is enabled.

          <c-loader-status
            :loaders="{
              'no-error-content no-initial-content': id ? [item.$load] : [],
              '': [item.$save],
            }"
          >
            <h2 class="py-2">
              Editing Widget: <c-display :model="item" for="widgetId" />
            </h2>
            <c-input :model="item" for="name" />
            <c-input :model="item" for="category" />
            <c-input :model="item" for="inventedOn" />
          </c-loader-status>

          <v-alert
            type="error"
            v-if="
              (item.$save.message || item.$load.message)?.match(
                /invalid object name/i,
              )
            "
          >
            The error above indicates that you have not yet created an Entity
            Framework Migration. You can add one by running
            <code>dotnet ef migrations add Init</code> in the Data project.
          </v-alert>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { WidgetViewModel } from "@/viewmodels.g";
import { useBindKeyToRouteOnCreate } from "coalesce-vue";

const props = defineProps<{ id?: number }>();

useTitle(() => item.name);
const form = useForm();

// The properties on the generated ViewModels are already reactive.
// ViewModels and ListViewModels don't need to be wrapped in ref/reactive.
const item = new WidgetViewModel();
item.$useAutoSave({
  wait: 500,
  debounce: { maxWait: 3000 },
});

(async function onCreated() {
  if (props.id) {
    await item.$load(props.id);
  } else {
    useBindKeyToRouteOnCreate(item);
  }
})();
</script>
