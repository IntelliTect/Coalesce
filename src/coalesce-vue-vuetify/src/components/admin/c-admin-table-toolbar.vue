<template>
  <v-toolbar
    extended
    class="c-admin-table-toolbar"
    dense :color="color" dark
  >

    <v-toolbar-title class="c-admin-table-toolbar--model-name hidden-xs-only">
      {{metadata.displayName}}
    </v-toolbar-title>

    <v-divider class="hidden-xs-only mx-4 my-0" vertical></v-divider>
    
    <v-btn
      v-if="canCreate"
      class="c-admin-table-toolbar--button-create"
      text
      :to="createRoute"
    >
      <v-icon :left="$vuetify.breakpoint.mdAndUp">fa fa-plus</v-icon>
      <span class="hidden-sm-and-down">Create</span>
    </v-btn>

    <v-btn 
      class="c-admin-table-toolbar--button-reload"
      text
      @click="list.$load()"
    >
      <v-icon left>fa fa-sync-alt</v-icon> 
      <span class="hidden-sm-and-down">Reload</span>
    </v-btn>

    <v-spacer></v-spacer>

    <span 
      v-if="list" 
      class="c-admin-table-toolbar--range hidden-sm-and-down"
    >
      Showing <c-list-range-display :list="list" />
    </span>

    <v-spacer></v-spacer>
    
    <c-list-page
      class="c-admin-table-toolbar--page"
      :list="list"
    />
        
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
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from '../c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags, mapParamsToDto } from 'coalesce-vue';

import CListRangeDisplay from '../display/c-list-range-display.vue';
import CListPage from '../input/c-list-page.vue';
import CListPageSize from '../input/c-list-page-size.vue';
import CListFilters from '../input/c-list-filters.vue';
    
@Component({
  name: 'c-admin-table-toolbar',
  components: { CListRangeDisplay, CListPage, CListPageSize, CListFilters }
})
export default class extends MetadataComponent {
  @Prop({required: true, type: Object})
  public list!: ListViewModel<any,any>;

  @Prop({type: String, default: "primary darken-1"})
  public color!: string;

  get metadata(): ModelType {
    return this.list.$metadata
  }

  get canCreate() {
    return this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Create) != 0
  }

  get createRoute() {
    // Resolve to an href to allow overriding of admin routes in userspace.
    // If we just gave a named raw location, it would always use the coalesce admin route
    // instead of the user-overridden one (that the user overrides by declaring another
    // route with the same path).
    return this.$router.resolve({
      name: 'coalesce-admin-item', 
      params: {
        type: this.metadata.name,
      },
      query: Object.fromEntries(Object
        .entries(mapParamsToDto(this.list.$params) || {})
        .filter(entry => entry[0].startsWith("filter."))
      )
    }).resolved.fullPath
  }

  /** Calculated width for the "Page" text input, such that it fits the max page number. */
  get pageInputWidth() {
    const totalCount = this.list.$load.totalCount || 1000
    return (Math.max(this.list.$page, totalCount).toString() + " of " + totalCount + "xx").length.toString() + "ch";
  }

}
</script>


<style lang="scss">

  .c-admin-table-toolbar {
    .v-btn {
      min-width: auto;
    }

    // Workaround a vuetify bug where the caret will be white.
    input[type=text] {
      caret-color: currentColor !important
    }

  }

</style>
