<template>
  <div class="c-table theme--light v-data-table">
    <div class="v-data-table__wrapper">
      <table>
        <thead class="v-data-table-header">
          <tr>
            <th 
              v-for="header in headers" 
              :key="'header-' + header.value"
              class="text-left"
              :class="{
                sortable: header.sortable, 
              }"
              @click="header.sortable ? orderByToggle(header.value) : void 0"
            >
              {{header.text}}
              <v-icon v-if="list.$params.orderBy == header.value">
                mdi-arrow-up
              </v-icon>
              <v-icon v-else-if="list.$params.orderByDescending == header.value">
                mdi-arrow-down
              </v-icon>
            </th>
          </tr>
        </thead>
        <thead class="c-table--header-loader">
          <tr>
            <th colspan="99" class="pa-0"> 
              <transition name="fade-transition">
                <v-progress-linear 
                  indeterminate 
                  height="4"
                  v-if="list.$load.isLoading"
                  ></v-progress-linear>
              </transition>
            </th>
          </tr>
        </thead>
        <c-table-body
          :items="list.$items"
          :props="effectiveProps">
          <template #item.append="{item}">
            <slot name="item.append" :item="item" />
          </template>
        </c-table-body>
      </table>
    </div>
  </div>

  <!-- <v-data-table
    class="c-table"
    v-bind="$attrs"
    :headers="$attrs.headers || headers"
    :items="list.$items"
    :sort-by.sync="sortBy"
    :sort-desc.sync="sortDesc"
    :server-items-length="list.$load.totalCount"
    hide-default-footer
    >
    <template #item="{ item, index }">
      <slot 
        name="item"
        :item="item"
        >
        <c-table-row :item="item" :effectiveProps="effectiveProps" :key="index">
          <template #item.append="{item}">
            <slot name="item.append" :item="item" />
          </template>
        </c-table-row>
      </slot>
    </template>
  </v-data-table> -->
</template>


<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ListViewModel, Property, ModelType } from 'coalesce-vue';

import CTableBody from './c-table-body.vue';
    
@Component({
  name: 'c-table',
  components: {
    CTableBody
  }
})
export default class extends Vue {

  @Prop({required: true, type: Object})
  public list!: ListViewModel<any,any>;

  @Prop({required: false, type: Array})
  public props?: Array<string>;

  @Prop({required: false, type: Array})
  public extraHeaders?: Array<string>;
  
  // For v-data-table impl.
  // sortBy: string | string[] = '';
  // sortDesc: boolean | boolean[] = false;

  get metadata(): ModelType {
    return this.list.$metadata
  }
  
  get effectiveProps() { 
    return Object
    .values(this.metadata.props)
    .filter((p: Property) => this.props
      ? this.props.indexOf(p.name) >= 0
      : p.role != "primaryKey" && p.role != "foreignKey"
    ); 
  };

  get headers() { 
    return [
      ...this.effectiveProps.map(o => ({
        text: o.displayName, 
        value: o.name,
        sortable: o.type != "collection",
        align: 'left'
      })),
      ...(this.extraHeaders || []).map(h => ({text: h, value: h, sortable: false }))
    ] 
  };

  // TODO: put orderByToggle on ListViewModel.
  orderByToggle(field: string) {
    const list = this.list;
    const params = list.$params;

    if (params.orderBy == field && !params.orderByDescending) {
      params.orderBy = null;
      params.orderByDescending = field;
    } else if (!params.orderBy && params.orderByDescending == field) {
      params.orderBy = null;
      params.orderByDescending = null;
    } else {
      params.orderBy = field;
      params.orderByDescending = null;
    }
  }

  mounted() {
    // For v-data-table impl
    // this.$watch(() => [this.sortBy, this.sortDesc], () => {
    //     // Always default to null to prevent reloading if this becomes emptystring
    //     // (vuetify will set it to emptystring).
    //     const unwrap = <T>(x: T | T[]) => Array.isArray(x) ? x[0] : x;

    //     const sortBy = unwrap(this.sortBy);
    //     const sortDesc = unwrap(this.sortDesc);

    //     this.list.$params.orderBy = sortDesc 
    //       ? null : sortBy;
          
    //     this.list.$params.orderByDescending = sortDesc 
    //       ? sortBy : null
    //   }
    // )
  }
}
</script>


<style lang="scss">
.c-table {
  word-break: initial;
  th, td {
    padding: 4px 8px;
    font-size: 12px;
  }

  th {
    vertical-align: bottom;
    .v-icon {
      font-size: 16px;
    }
  }

  .c-table--header-loader {
    th {
      height: #{4px + 1px}; // add one px for the border
      border: none !important;
    }
  }
}
</style>