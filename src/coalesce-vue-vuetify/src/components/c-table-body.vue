<template>
  <tbody>
    <c-table-row  
      v-for="(item, index) in items"
      :key="index"
      :item="item" 
      :props="props"
    >
      <template #item.append="{item}">
        <slot name="item.append" :item="item" />
      </template>
    </c-table-row>
  </tbody>
</template>


<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel } from 'coalesce-vue';

import CTableRow from './c-table-row.vue';
    
@Component({
  name: 'c-table-body',
  components: {
    CTableRow
  }
})
export default class extends Vue {
  @Prop({required: false, type: Array})
  public items?: ViewModel[];

  @Prop({required: false, type: Array})
  public props?: Array<string>;
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