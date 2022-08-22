
<template>
  <v-dialog v-model="dialogOpen">
    
    <!-- Trick: Passes the 'activator' slot to the dialog only if it was provided to c-methods-dialog.
      Otherwise, won't provide the slot at all so that vuetify won't freak about being given an empty activator. -->
    <template 
      v-for="(_, slot) of ($scopedSlots['activator'] ? {activator: $scopedSlots['activator']} : {})" 
      v-slot:[slot]="scope">
      <slot :name="slot" :open="open"/>
    </template>
    
    <v-card v-if="dialogOpen && model">
      <v-card-title class="headline">
       Methods &mdash; 
       <c-display :model="model" />
      </v-card-title>
      <v-card-text>
        <c-admin-methods
          :model="model"
        >
        </c-admin-methods>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn 
          color="primary" 
          text 
          @click.native="close()"
          >Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ViewModel, Property, Method, ModelType } from 'coalesce-vue';

import CAdminMethods from './c-admin-methods.vue'

@Component({
  name: 'c-methods-dialog',
  components: {
    CAdminMethods
  }
})
export default class extends Vue {
  dialogOpen = false;
  private model: ViewModel<Model<ModelType>> | null = null;

  open(viewModel: ViewModel) {
    this.model = viewModel;
    this.dialogOpen = true;
  }

  close() {
    this.dialogOpen = false;
    this.model = null;
  }
}

</script>