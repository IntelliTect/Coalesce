
<template>
  <v-container grid-list-lg >
    <v-layout wrap >
      <v-flex 
        sm6 md6 xsl2
        v-for="prop in showProps" 
        :key="prop.name" 
        class="py-0">
        <c-input :model="model" :for="prop"/>
      </v-flex>
    </v-layout>
  </v-container>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ViewModel, Property, ModelType } from 'coalesce-vue';

import CInput from './c-input'
    
@Component({
  name: 'c-editor',
  components: {
    CInput
  }
})
export default class extends Vue {
  @Prop({required: true})
  public model!: Model | null;
  
  get showProps() { 
    if (!this.model) return [];

    return Object
    .values(this.model.$metadata.props)
    .filter((p: Property) => 
      p.role != "primaryKey" 
      && p.role != "foreignKey"
      && (!p.dontSerialize || p.role == "referenceNavigation" || p.role == "collectionNavigation")
    ); 
  };
}

</script>