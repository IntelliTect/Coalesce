<template>
  <tbody>
    <tr
      v-for="(item, index) in items"
      :key="index"
    >
      <td 
        v-for="prop in props" 
        :key="prop.name" 
        :class="['prop-' + prop.name,]"
        class="text-xs-left" 
      >
        <c-admin-display :model="item" :for="prop" />
      </td>
      <slot name="item.append" :item="item" />
    </tr>
  </tbody>
</template>


<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel } from 'coalesce-vue';

import CAdminDisplay from './c-admin-display';
    
@Component({
  name: 'c-table-body',
  components: { CAdminDisplay }
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