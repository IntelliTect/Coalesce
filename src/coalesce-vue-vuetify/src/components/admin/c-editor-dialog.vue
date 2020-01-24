
<template>
  <v-dialog v-model="dialogOpen">
    
    <!-- Trick: Passes the 'activator' slot to the dialog only if it was provided to c-editor-dialog.
      Otherwise, won't provide the slot at all so that vuetify won't freak about being given an empty activator. -->
    <template 
      v-for="(_, slot) of ($scopedSlots['activator'] ? {activator: $scopedSlots['activator']} : {})" 
      v-slot:[slot]="scope">
      <slot :name="slot" :create="create" :edit="edit"/>
    </template>

    <v-card v-if="dialogOpen && modelName">
      <v-card-title class="headline">
        <slot name="title" :model="viewModel">
          <span v-if="isCreate">
            Create {{viewModel.$metadata.displayName}}
          </span>
          <span v-else>
            Edit {{viewModel.$metadata.displayName}}:
            <c-display :model="viewModel" />
          </span>
        </slot>
      </v-card-title>
      <v-card-text>
        <c-admin-editor :model="viewModel" >
        </c-admin-editor>

        <template v-if="hasInstanceMethods">
          <h1 class="pb-1 pt-4">Methods</h1>
          <c-methods :model="viewModel" />
        </template>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn 
          color="primary" 
          text 
          @click.native="cancel()"
          >Cancel</v-btn>
        <v-btn 
          color="success" 
          @click.native="save()"
          :loading="this.viewModel.$save.isLoading"
          >
          <v-icon left>fa fa-save</v-icon>
          Save
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>


<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ViewModel, Property, ModelType, BehaviorFlags } from 'coalesce-vue';

import CAdminEditor from './c-admin-editor.vue'
    
@Component({
  name: 'c-editor-dialog',
  components: {
    CAdminEditor
  }
})
export default class extends Vue {

  @Prop({required: false, type: String, default: null})
  public modelName!: string | null;

  private dialogOpen = false;
  private viewModel: ViewModel<Model<ModelType>> | null = null;

  create(initial?: object) {
    const modelName = this.modelName;
    if (!modelName) {
      throw Error("Cannot use a c-editor-dialog for create() if prop `model-name` wasn't provided to c-editor-dialog.")
    }
    this.viewModel = new ViewModel.typeLookup![modelName](initial);
    this.dialogOpen = true;
  }

  edit(viewModel: ViewModel) {
    this.viewModel = viewModel;
    this.dialogOpen = true;
  }

  get metadata() {
    return this.viewModel?.$metadata;
  }

  get canEdit() {
    if (!this.viewModel) return false;

    return this.metadata && (this.metadata.behaviorFlags & 
      this.viewModel.$primaryKey ? BehaviorFlags.Edit : BehaviorFlags.Create
    ) != 0
  }
  get hasInstanceMethods() {
    return !this.isCreate && this.metadata && Object.values(this.metadata.methods).some(m => !m.isStatic)
  }

  get isCreate() {
    return !this.viewModel!.$primaryKey;
  }

  cancel() {
    if (!this.viewModel) return;

    if (!this.isCreate) {
      this.viewModel
        .$load()
        .then(() => this.close())
    } else {
      this.close()
    }
  }

  save() {
    if (!this.viewModel) return;
    const isCreate = !this.viewModel.$primaryKey
    this.viewModel
      .$save()
      .then(() => {
        this.$emit("saved")
        this.$emit(isCreate ? "created" : "updated", this.viewModel)
        this.close()
      })
  }

  close() {
    this.dialogOpen = false;
    this.viewModel = null;
  }
}

</script>