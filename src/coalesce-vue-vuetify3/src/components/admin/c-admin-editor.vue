<template>
  <v-card class="c-admin-editor" :class="'type-' + metadata.name">
    <v-toolbar
      class="c-admin-editor--toolbar"
      density="compact"
      :color="color"
      elevation="4"
    >
      <v-toolbar-title style="flex: 0 1 auto">
        <template v-if="showContent">
          <span v-if="!canEdit">View</span>
          <span v-else-if="!hasPk">Create</span>
          <span v-else>Edit</span>
        </template>
        {{ metadata.displayName }}
      </v-toolbar-title>

      <v-divider class="ml-4 my-0" vertical></v-divider>

      <v-toolbar-title v-if="hasPk" class="hidden-xs">
        <c-display :model="model"></c-display>
      </v-toolbar-title>

      <v-spacer></v-spacer>
      <v-btn
        v-if="!model.$isAutoSaveEnabled && showContent && canEdit"
        @click="model.$bulkSave()"
        title="Save"
        :color="isBulkSaveDirty ? 'success' : undefined"
        :variant="isBulkSaveDirty ? 'elevated' : 'text'"
        :loading="model.$bulkSave.isLoading"
        prepend-icon="fa fa-save"
      >
        <span class="hidden-sm-and-down">Save</span>
      </v-btn>
      <v-btn
        v-if="canDelete"
        @click="deleteItemWithConfirmation()"
        variant="text"
        :disabled="!hasPk"
        title="Delete"
        prepend-icon="fa fa-trash-alt"
      >
        <span class="hidden-sm-and-down">Delete</span>
      </v-btn>
      <v-btn
        @click="reload"
        variant="text"
        :disabled="!hasPk"
        title="Reload"
        prepend-icon="$loading"
      >
        <span class="hidden-sm-and-down">Reload</span>
      </v-btn>
    </v-toolbar>

    <div v-if="metadata.description" class="c-admin-page--description">
      <i class="fa fa-info-circle"></i>
      {{ metadata.displayName }}: {{ metadata.description }}
    </div>

    <v-card-text class="pt-2">
      <c-loader-status
        :loaders="{
          [!showContent ? 'no-initial-content' : 'no-error-content']: [
            model.$load,
          ],
          '': [model.$save, model.$bulkSave, model.$delete],
        }"
      >
        <v-form ref="form">
          <v-row
            v-for="prop in showProps"
            :key="prop.name"
            class="c-admin-editor--row"
            :class="'prop-' + prop.name"
            no-gutters
          >
            <v-col cols="12" md="2" class="c-admin-editor--label-col">
              <i
                v-if="prop.createOnly && !isPropReadOnly(prop, model)"
                class="fa fa-asterisk pr-1"
                title="Field cannot be changed after save"
              ></i>
              {{ prop.displayName }}
            </v-col>
            <v-col class="py-0" align-self="start" style="flex-basis: 1px">
              <div class="c-admin-editor--input-row">
                <div class="c-admin-editor--input-col">
                  <c-input
                    :model="model"
                    :for="prop"
                    v-bind="propInputBinds(prop)"
                    label=""
                    :aria-label="prop.displayName"
                    :aria-description="prop.description"
                    hide-details="auto"
                    density="compact"
                    variant="outlined"
                    @deleted="
                      // Reload when a c-select-many-to-many item is deleted,
                      // since delete responses have no payloads and the delete
                      // might have affected a computed property on the model
                      model.$load()
                    "
                  >
                    <c-admin-display :model="model" :for="prop" />
                  </c-input>
                </div>
                <div
                  v-if="prop.role == 'referenceNavigation'"
                  class="c-admin-editor--aux-col"
                >
                  <v-btn
                    class="c-admin-editor--ref-nav-link"
                    variant="outlined"
                    tabindex="-1"
                    title="View selected item"
                    :disabled="!getRefNavRoute($router, model, prop)"
                    :to="getRefNavRoute($router, model, prop)"
                  >
                    <v-icon>fa fa-ellipsis-h</v-icon>
                  </v-btn>
                </div>
                <div
                  v-if="
                    prop.type == 'string' &&
                    (prop.subtype == 'url' ||
                      prop.subtype == 'email' ||
                      prop.subtype == 'tel')
                  "
                  class="c-admin-editor--aux-col"
                >
                  <v-btn
                    class="c-admin-editor--href-link"
                    variant="outlined"
                    tabindex="-1"
                    :disabled="!(model as any)[prop.name]"
                    :href="
                      (prop.subtype == 'email'
                        ? 'mailto:'
                        : prop.subtype == 'tel'
                          ? 'tel:'
                          : '') + (model as any)[prop.name]
                    "
                  >
                    <v-icon class="black--text"
                      >fa fa-external-link-alt
                    </v-icon>
                  </v-btn>
                </div>
                <div
                  v-if="prop.type == 'string' && prop.subtype == 'url-image'"
                  class="c-admin-editor--aux-col"
                >
                  <v-card outlined rounded class="c-admin-editor--img-preview">
                    <c-display :model="model" :for="prop"></c-display>
                  </v-card>
                </div>
              </div>
            </v-col>
          </v-row>
        </v-form>
      </c-loader-status>
    </v-card-text>

    <v-card-actions v-if="canEdit && showContent">
      <v-spacer></v-spacer>
      <v-btn
        v-if="!model.$isAutoSaveEnabled"
        @click="model.$bulkSave()"
        title="Save"
        :color="isBulkSaveDirty ? 'success' : undefined"
        :variant="isBulkSaveDirty ? 'elevated' : 'text'"
        :loading="model.$bulkSave.isLoading"
        prepend-icon="fa fa-save"
      >
        <span class="hidden-sm-and-down">Save</span>
      </v-btn>
      <v-btn
        v-else
        disabled
        prepend-icon="fa fa-save"
        :loading="model.$save.isLoading"
      >
        Auto-save enabled
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import {
  ViewModel,
  Property,
  ModelType,
  BehaviorFlags,
  HiddenAreas,
} from "coalesce-vue";

import { getRefNavRoute } from "./util";
import { isPropReadOnly } from "../../util";
import { watch, computed, useTemplateRef } from "vue";

defineOptions({
  name: "c-admin-editor",
});

const props = withDefaults(
  defineProps<{
    model: ViewModel;
    color?: string;
    /** Whether or not a delete button is shown. Default true if the provided model allows deletes. */
    deletable?: boolean;
  }>(),
  {
    deletable: true,
  },
);

const emit = defineEmits<{
  deleted: [];
}>();

const form = useTemplateRef("form");

// Validate the form when it is rendered to trigger all validation messages.
// This will either be immediate for a create scenario, or delayed until load for an edit.
watch(form, (form) => {
  form?.validate?.();
});

function propInputBinds(p: Property) {
  let readonly = isPropReadOnly(p, props.model);

  return {
    readonly,
  };
}

async function deleteItemWithConfirmation() {
  if (confirm("Are you sure you wish to delete this item?")) {
    await props.model.$delete();
    emit("deleted");
  }
}

async function reload() {
  props.model.$save.wasSuccessful = null;
  props.model.$delete.wasSuccessful = null;
  props.model.$bulkSave.wasSuccessful = null;
  props.model.$load();
}
const metadata = computed((): ModelType => {
  if (props.model) {
    return props.model.$metadata;
  }
  throw `No metadata available.`;
});

const showContent = computed(() => {
  const model = props.model;
  return (
    // If we have loaded at least once, we're in an edit scenario and the object is loaded.
    model.$load.wasSuccessful ||
    // If we're not loading now, we're in a create scenario.
    !model.$load.isLoading
  );
});

const hasPk = computed(() => {
  return props.model.$primaryKey != null;
});

const canEdit = computed(() => {
  const metadataValue = metadata.value;
  if (!metadataValue) return false;

  return (
    (metadataValue.behaviorFlags &
      (hasPk.value ? BehaviorFlags.Edit : BehaviorFlags.Create)) !=
    0
  );
});

const canDelete = computed(() => {
  return (
    props.deletable &&
    (metadata.value?.behaviorFlags & BehaviorFlags.Delete) != 0
  );
});

const showProps = computed(() => {
  if (!props.model) return [];

  return Object.values(metadata.value.props).filter(
    (p: Property) =>
      p.hidden === undefined || (p.hidden & HiddenAreas.Edit) == 0,
    // && (!p.dontSerialize || p.role == "referenceNavigation" || p.role == "collectionNavigation")
  );
});

const isBulkSaveDirty = computed(() => {
  return props.model.$bulkSavePreview().isDirty;
});
</script>

<style lang="scss">
.c-admin-editor--row {
  // Center each row so that things are nicely aligned,
  // especially in the case where labels are long enough to have to wrap.
  align-items: center;
  padding: 6px 0;

  // Remove extra padding on the top of v-switch components
  // so they align nicely with other components.
  .v-input--switch,
  .v-input--checkbox {
    margin-top: 0;
    padding-top: 0px;
    td & {
      padding-top: 0;
    }
  }
}

$row-height: 40px;
$text-height: 10px;
.c-admin-editor--label-col {
  padding: 0 12px 0 0 !important;
  font-weight: bold;
  align-self: start;
  @media (min-width: 960px) {
    min-height: $row-height;
    padding-top: ($row-height/2 - $text-height) !important;
    text-align: right;
  }
}

.c-admin-editor--input-row {
  display: flex;
  min-height: $row-height;
  width: 100%;
}
.c-admin-editor--input-col {
  flex-grow: 1;

  @media (min-width: 960px) {
    > a,
    > span {
      display: inline-block;
      padding-top: ($row-height/2 - $text-height) !important;
    }
  }
}
.c-admin-editor--aux-col {
  align-self: flex-start;
  padding-top: 2px;
  padding-left: 12px !important;
}
.c-admin-editor--img-preview img {
  max-width: 100px;
  display: block;
}
</style>
