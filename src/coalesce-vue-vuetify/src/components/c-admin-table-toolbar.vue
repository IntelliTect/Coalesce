<template>
  <v-toolbar
    extended
    class="c-admin-table-toolbar"
    dense :color="color" dark>

    <v-toolbar-title class="white--text">
      {{metadata.displayName}}
    </v-toolbar-title>

    <v-divider class="mx-4 my-0" vertical></v-divider>
    
    <v-select
      label="Page Size"
      outlined
      class="d-inline-block c-admin-table-toolbar--page-size"
      :items="[10,25,100]"
      v-model="list.$params.pageSize"
      hide-details
      dense
    ></v-select>

    <v-divider class="ml-4 my-0" vertical></v-divider>

    <c-pagination-page
      :list="list"
      class="c-admin-table-toolbar--pagination"
    />

    <v-divider class="mr-4 my-0" vertical></v-divider>

    <span v-if="list" class="c-admin-table-toolbar--range">
      <!-- These use list.$load explicitly so that the numbers match
      the actual currently displayed data, rather than the state of the parameters. -->
      {{(list.$load.page-1) * list.$load.pageSize + 1}}
      - 
      {{list.$load.page == list.$load.pageCount ? list.$load.totalCount : list.$load.page * list.$load.pageSize}}
      of
      {{list.$load.totalCount}}
    </span>


    <v-spacer></v-spacer>
    <!-- <v-divider class="mx-3 my-0" vertical></v-divider> -->

    <v-text-field
      flat
      solo-inverted
      hide-details
      prepend-inner-icon="mdi-magnify"
      label="Search"
      v-model="list.$params.search"
      single-line
      clearable
    ></v-text-field>
        
    <template v-slot:extension>
      <c-editor-dialog 
        v-if="canCreate"
        :model-name="metadata.name"
        @saved="list.$load()"
        >
        
        <template #activator="{ on, create, edit }">
          <v-btn
            v-on="on"
            text
            @click="create()">
            <v-icon :left="$vuetify.breakpoint.mdAndUp">mdi-plus</v-icon>
            <span class="hidden-sm-and-down">Create</span>
          </v-btn>
        </template>

      </c-editor-dialog>
<!-- 
      <v-divider
        v-if="canCreate"
        class="mx-2"
        vertical
      ></v-divider> -->

      <v-spacer></v-spacer>
      <v-btn @click="list.$load()" text>
        <v-icon left>mdi-reload</v-icon> 
        <span class="hidden-sm-and-down">Reload</span>
      </v-btn>
    </template>
  </v-toolbar>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags } from 'coalesce-vue';

import CDisplay from './c-display'
import CEditorDialog from './c-editor-dialog.vue';
    
@Component({
  name: 'c-admin-table-toolbar',
  components: {
    CDisplay, CEditorDialog
  }
})
export default class extends MetadataComponent {
  @Prop({required: true, type: Object})
  public list!: ListViewModel<any,any>;

  @Prop({type: String, default: "primary"})
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
      min-height: 34px;

      .v-input__append-inner {
        margin-top: 4px;
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
