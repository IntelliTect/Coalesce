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
      <v-form ref="form">
        <c-loader-status
          :loaders="{
            [!showContent ? 'no-initial-content' : 'no-error-content']: [
              model.$load,
            ],
            '': [model.$save],
          }"
        >
          <template #default>
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
          </template>
        </c-loader-status>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script lang="ts">
import { PropType, defineComponent } from "vue";
import {
  Model,
  ViewModel,
  Property,
  ModelType,
  BehaviorFlags,
  HiddenAreas,
} from "coalesce-vue";

import { isPropReadOnly } from "../../util";

export default defineComponent({
  name: "c-admin-editor",

  mounted() {
    (this.$refs.form as any).validate();
  },

  props: {
    model: {
      required: true,
      type: Object as PropType<ViewModel<Model<ModelType>>>,
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
