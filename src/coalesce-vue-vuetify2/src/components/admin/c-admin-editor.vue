<template>
  <v-card class="c-admin-editor">
    <v-toolbar
      class="c-admin-editor-page--toolbar"
      dense
      color="primary darken-1"
      dark
    >
      <v-toolbar-title>
        <template v-if="showContent">
          <span v-if="!canEdit">View</span>
          <span v-else-if="!hasPk">Create</span>
          <span v-else>Edit</span>
        </template>
        {{ metadata.displayName }}
      </v-toolbar-title>

      <v-divider class="mx-4 my-0" vertical></v-divider>

      <v-toolbar-title v-if="hasPk" class="hidden-xs-only">
        <c-display :model="model"></c-display>
      </v-toolbar-title>

      <v-spacer></v-spacer>
      <v-btn @click="model.$load()" text :disabled="!hasPk">
        <v-icon left>fa fa-sync-alt</v-icon>
        <span class="hidden-sm-and-down">Reload</span>
      </v-btn>
    </v-toolbar>

    <v-card-text>
      <c-loader-status
        :loaders="{
          [!showContent ? 'no-initial-content' : 'no-error-content']: [
            model.$load,
          ],
          '': [model.$save],
        }"
      >
        <template #default>
          <v-form ref="form">
            <v-row
              v-for="prop in showProps"
              :key="prop.name"
              class="c-admin-editor--row"
            >
              <v-col
                cols="12"
                md="2"
                class="py-0 py-md-3 font-weight-bold text-md-right"
                align-self="start"
              >
                {{ prop.displayName }}
              </v-col>
              <v-col class="py-0">
                <c-input
                  :model="model"
                  :for="prop"
                  v-bind="propInputBinds(prop)"
                  label=""
                  hide-details="auto"
                  @deleted="
                    // Reload when a c-select-many-to-many item is deleted,
                    // since delete responses have no payloads and the delete
                    // might have affected a computed property on the model
                    model.$load()
                  "
                >
                  <div class="pt-md-3">
                    <c-admin-display :model="model" :for="prop" />
                  </div>
                </c-input>
              </v-col>
              <v-col
                v-if="prop.role == 'referenceNavigation'"
                class="py-0 flex-grow-0"
              >
                <v-btn
                  class="c-admin-editor--ref-nav-link"
                  outlined
                  color="grey"
                  :disabled="!model[prop.foreignKey.name]"
                  :to="{
                    name: 'coalesce-admin-item',
                    params: {
                      type: prop.typeDef.name,
                      id: model[prop.foreignKey.name],
                    },
                  }"
                >
                  <v-icon class="black--text">fa fa-ellipsis-h</v-icon>
                </v-btn>
              </v-col>
              <v-col
                v-if="
                  prop.type == 'string' &&
                  (prop.subtype == 'url' ||
                    prop.subtype == 'email' ||
                    prop.subtype == 'tel')
                "
                class="py-0 flex-grow-0"
              >
                <v-btn
                  class="c-admin-editor--href-link"
                  outlined
                  color="grey"
                  :disabled="!model[prop.name]"
                  :href="
                    (prop.subtype == 'email'
                      ? 'mailto:'
                      : prop.subtype == 'tel'
                      ? 'tel:'
                      : '') + model[prop.name]
                  "
                >
                  <v-icon class="black--text">fa fa-external-link-alt </v-icon>
                </v-btn>
              </v-col>
              <v-col
                v-if="prop.type == 'string' && prop.subtype == 'url-image'"
                class="py-0 flex-grow-0"
              >
                <v-card outlined rounded>
                  <c-display
                    :model="model"
                    :for="prop"
                    style="max-width: 100px; display: block"
                  ></c-display>
                </v-card>
              </v-col>
            </v-row>
          </v-form>
        </template>
      </c-loader-status>
    </v-card-text>
  </v-card>
</template>

<script lang="ts">
import { PropType, defineComponent, ref, watch } from "vue";
import {
  Model,
  ViewModel,
  Property,
  ModelType,
  BehaviorFlags,
  HiddenAreas,
  Indexable,
} from "coalesce-vue";

import { isPropReadOnly } from "../../util";

export default defineComponent({
  name: "c-admin-editor",

  setup() {
    const form = ref<any>();
    // Validate the form when it is rendered to trigger all validation messages.
    // This will either be immediate for a create scenario, or delayed until load for an edit.
    watch(form, (form) => {
      form?.validate?.();
    });
    return { form };
  },

  props: {
    model: {
      required: true,
      type: Object as PropType<Indexable<ViewModel<Model<ModelType>>>>,
    },
  },

  methods: {
    propInputBinds(p: Property) {
      let readonly = isPropReadOnly(p, this.model);

      return {
        readonly,
      };
    },
  },

  computed: {
    metadata(): ModelType {
      if (this.model) {
        return this.model.$metadata;
      }
      throw `No metadata available.`;
    },

    showContent() {
      const model = this.model;
      return (
        // If we have loaded at least once, we're in an edit scenario and the object is loaded.
        model.$load.wasSuccessful ||
        // If we're not loading now, we're in a create scenario.
        !model.$load.isLoading
      );
    },

    hasPk() {
      return this.model.$primaryKey != null;
    },

    canEdit() {
      const metadata = this.metadata;
      if (!metadata) return false;

      return (
        (metadata.behaviorFlags &
          (this.hasPk ? BehaviorFlags.Edit : BehaviorFlags.Create)) !=
        0
      );
    },

    showProps() {
      if (!this.model) return [];

      return Object.values(this.metadata.props).filter(
        (p: Property) =>
          p.hidden === undefined || (p.hidden & HiddenAreas.Edit) == 0
        // && (!p.dontSerialize || p.role == "referenceNavigation" || p.role == "collectionNavigation")
      );
    },
  },
});
</script>

<style lang="scss">
.c-admin-editor--ref-nav-link {
  height: 40px !important;
}
.c-admin-editor--row {
  align-items: flex-start;
  padding: 4px 0;

  // Remove extra padding on the top of v-switch components
  // so they align nicely with other components.
  .v-input--switch,
  .v-input--checkbox {
    margin-top: 12px;
    padding-top: 0px;
    td & {
      padding-top: 0;
    }
  }
}
</style>
