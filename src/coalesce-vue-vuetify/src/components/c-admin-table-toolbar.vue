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
      class="c-admin-table-toolbar--button-create"
      text
      :to="{name: 'coalesce-admin-item', params: {type: metadata.name}}"
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
      <!-- These use list.$load explicitly so that the numbers match
      the actual currently displayed data, rather than the state of the parameters. -->
      Showing {{(list.$load.page-1) * list.$load.pageSize + 1}}
      - 
      {{list.$load.page == list.$load.pageCount ? list.$load.totalCount : list.$load.page * list.$load.pageSize}}
      of
      {{list.$load.totalCount}}
    </span>

    <v-spacer></v-spacer>
    
    <c-pagination-page
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

      <v-spacer></v-spacer>
    
      <v-select
        class="d-inline-block c-admin-table-toolbar--page-size ml-1"
        label="Page Size"
        outlined
        :items="[10,25,100]"
        v-model="list.$params.pageSize"
        hide-details
        dense
      ></v-select>

    </template>
  </v-toolbar>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags } from 'coalesce-vue';

    
@Component({
  name: 'c-admin-table-toolbar',
  components: { }
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

    .c-admin-table-toolbar--page-size {
      max-width: 100px;
    }

    .v-text-field--outlined .v-input__slot {
      height: 34px;
      min-height: 34px !important;

      .v-input__append-inner {
        margin-top: 4px !important;
      }

      // https://css-tricks.com/snippets/css/turn-off-number-input-spinners/
      input[type=number]::-webkit-inner-spin-button, 
      input[type=number]::-webkit-outer-spin-button { 
        -webkit-appearance: none; 
        margin: 0; 
      }
    }
  }

</style>
