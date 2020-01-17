<template>
  <v-card class="c-admin-table">
    <c-admin-table-toolbar
      :list="viewModel"
    >
    </c-admin-table-toolbar>

    <v-card-text>
      
      <c-editor-dialog 
        v-if="canEdit"
        ref="editor"
        :model-name="metadata.name"
        @saved="viewModel.$load()"
      >
      </c-editor-dialog>

      <c-methods-dialog
        v-if="hasInstanceMethods"
        ref="methods"
      >
      </c-methods-dialog>


      <c-table
        :list="viewModel"
        :extra-headers="canEdit || canDelete || hasInstanceMethods ? ['Actions'] : []"
        >
        <template #item.append="{item}">
          <td>
            <v-layout>
              <v-btn
                v-if="canEdit"
                title="Edit"
                text icon
                @click="$refs.editor.edit(item)">
                <!-- Using an <i> directly is much more performant than v-icon. -->
                <i aria-hidden="true" class="v-icon notranslate mdi mdi-pencil"></i>
              </v-btn>

              <v-btn
                v-if="hasInstanceMethods"
                @click="$refs.methods.open(item)"
                title="Methods"
                text icon>
                <i aria-hidden="true" class="v-icon notranslate mdi mdi-console-line"></i>
              </v-btn>
            
              <v-btn
                v-if="canDelete"
                title="Delete"
                text icon
                @click="deleteItemWithConfirmation(item)">
                <i aria-hidden="true" class="v-icon notranslate mdi mdi-delete"></i>
              </v-btn>
              
            </v-layout>
          </td>
        </template>
      </c-table>
    </v-card-text>
  </v-card>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import MetadataComponent from './c-metadata-component'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags } from 'coalesce-vue';

import CDisplay from './c-display'
import CTable from './c-table.vue';
import CEditorDialog from './c-editor-dialog.vue';
import CMethodsDialog from './c-methods-dialog.vue';
import CAdminTableToolbar from './c-admin-table-toolbar.vue';
    
@Component({
  name: 'c-admin-table',
  components: {
    CDisplay, CEditorDialog, CTable, CMethodsDialog, CAdminTableToolbar
  }
})
export default class extends MetadataComponent {
  @Prop({required: true, type: Object})
  public model!: ListViewModel;

  get viewModel(): ListViewModel {
    if (this.model instanceof ListViewModel) return this.model;
    throw Error("c-admin-table: prop `model` is required, and must be a ListViewModel.");
  }
  
  async deleteItemWithConfirmation(item: ViewModel<any, any>){
    if (confirm('Are you sure you wish to delete this item?')) {
      await item.$delete();
      await this.viewModel.$load();
    }
  }

  get metadata(): ModelType {
    return this.viewModel.$metadata
  }

  get canEdit() {
    return this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Edit) != 0
  }
  get canDelete() {
    return this.metadata && (this.metadata.behaviorFlags & BehaviorFlags.Delete) != 0
  }
  get hasInstanceMethods() {
    return this.metadata && Object.values(this.metadata.methods).some(m => !m.isStatic)
  }

  created() {
    this.viewModel.$load.setConcurrency("debounce");
    this.viewModel.$startAutoLoad(this, { wait: 0 })
    this.viewModel.$load();
  }
}
</script>

