<template>
  <tr>
    <td 
      v-for="prop in props" 
      :key="prop.name" 
      :class="['prop-' + prop.name, /*prop.type == 'number' ? 'text-xs-right' : 'text-xs-left'*/ ]"
      class="text-xs-left" 
    >
      <c-admin-display :model="item" :for="prop" />
    </td>
    <slot
      name="item.append"
      :item="item">
    </slot>
  </tr>
</template>


<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, propDisplay } from 'coalesce-vue';

import CAdminDisplay from './c-admin-display';
    
@Component({
  name: 'c-table-row',
  components: {
    CAdminDisplay
  }
})
export default class extends Vue {

  @Prop({required: true, type: Object})
  public item!: ViewModel<any,any>;

  @Prop({required: true, type: Array})
  public props?: Array<Property>;

  display(prop: Property) {
    return propDisplay(this.item, prop);
  }
}
</script>
