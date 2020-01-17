<template>
  <tr>
    <td 
      v-for="prop in props" 
      :key="prop.name" 
      :class="['prop-' + prop.name, /*prop.type == 'number' ? 'text-xs-right' : 'text-xs-left'*/ ]"
      class="text-xs-left" >
      <!-- Calling propDisplay is faster than rendering a c-display. -->
      <c-display :model="item" :for="prop"></c-display>
      <!-- {{display(prop)}} -->
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

import CDisplay from './c-display';
    
@Component({
  name: 'c-table-row',
  components: {
    CDisplay
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
