<template>
  <v-toolbar extended class="c-admin-table-toolbar">
    <v-toolbar-title class="c-admin-table-toolbar--model-name hidden-xs-only">
      {{ metadata.displayName }}
    </v-toolbar-title>

    <v-divider class="hidden-xs-only mx-4 my-0" vertical></v-divider>

    <v-btn
      v-if="canCreate"
      class="c-admin-table-toolbar--button-create"
      variant="text"
      :to="createRoute"
    >
      <v-icon :start="$vuetify.display.mdAndUp">$plus</v-icon>
      <span class="hidden-sm-and-down">Create</span>
    </v-btn>

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
      @click="$emit('update:editable', !editable)"
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
        flat
        solo-inverted
        hide-details
        prepend-inner-icon="fa fa-search"
        label="Search"
        v-model="list.$params.search"
        single-line
        clearable
      ></v-text-field>

      <v-divider class="hidden-xs-only mx-4 my-0" vertical></v-divider>

      <c-list-filters :list="list" />

      <v-spacer></v-spacer>

      <c-list-page-size :list="list" />
    </template>
  </v-toolbar>
</template>

<script lang="ts">
import { PropType, defineComponent } from "vue";
import { useRouter } from "vue-router";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";
import {
  ListViewModel,
  ModelType,
  BehaviorFlags,
  mapParamsToDto,
} from "coalesce-vue";

export default defineComponent({
  name: "c-admin-table-toolbar",

  setup(props) {
    return {
      ...useMetadataProps(props),
      router: useRouter(),
    };
  },

  props: {
    ...makeMetadataProps(),
    list: { required: true, type: Object as PropType<ListViewModel> },
    color: { default: "primary", type: String },
    editable: { default: null, required: false, type: Boolean },
  },

  computed: {
    metadata(): ModelType {
      return this.list.$metadata;
    },

    createRoute() {
      // Resolve to an href to allow overriding of admin routes in userspace.
      // If we just gave a named raw location, it would always use the coalesce admin route
      // instead of the user-overridden one (that the user overrides by declaring another
      // route with the same path).
      this.router;
      return this.router.resolve({
        name: "coalesce-admin-item",
        params: {
          type: this.metadata.name,
        },
        query: Object.fromEntries(
          Object.entries(mapParamsToDto(this.list.$params) || {}).filter(
            (entry) => entry[0].startsWith("filter.")
          )
        ),
      }).fullPath;
    },

    canCreate() {
      return (
        this.metadata &&
        (this.metadata.behaviorFlags & BehaviorFlags.Create) != 0
      );
    },

    /** Calculated width for the "Page" text input, such that it fits the max page number. */
    pageInputWidth() {
      const totalCount = this.list.$load.totalCount || 1000;
      return (
        (
          Math.max(this.list.$page, totalCount).toString() +
          " of " +
          totalCount +
          "xx"
        ).length.toString() + "ch"
      );
    },
  },
});
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
}
</style>
