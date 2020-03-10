<template>
  <v-card class="c-admin-table">
    <c-admin-table-toolbar
      :list="viewModel"
    />

    <v-card-text class="pt-3">
      <c-table
        admin
        :list="viewModel"
        :extra-headers="canEdit || canDelete || hasInstanceMethods ? ['Actions'] : []"
      >
        <template #item.append="{item}">
          <td>
            <v-layout>
              <v-btn
                v-if="canEdit || hasInstanceMethods"
                class="mx-1"
                title="Edit"
                text icon
                :to="{name: 'coalesce-admin-item', params: { type: metadata.name, id: item.$primaryKey }}"
              >
                <!-- Using an <i> directly is much more performant than v-icon. -->
                <i aria-hidden="true" class="v-icon notranslate fa fa-edit"></i>
              </v-btn>
            
              <v-btn
                v-if="canDelete"
                class="mx-1"
                title="Delete"
                text icon
                @click="deleteItemWithConfirmation(item)">
                <i aria-hidden="true" class="v-icon notranslate fa fa-trash-alt"></i>
              </v-btn>
              
            </v-layout>
          </td>
        </template>
      </c-table>

      <c-list-pagination :list="list" class="mt-4"></c-list-pagination>
    </v-card-text>
  </v-card>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { Model, ClassType, ListViewModel, Property, ModelType, ViewModel, BehaviorFlags } from 'coalesce-vue';

import CTable from '../display/c-table.vue';
import CAdminTableToolbar from './c-admin-table-toolbar.vue';
    
@Component({
  name: 'c-admin-table',
  components: {
    CTable, CAdminTableToolbar
  }
})
export default class extends Vue {
  @Prop({required: true, type: Object})
  public list!: any;

  get viewModel(): ListViewModel {
    if (this.list instanceof ListViewModel) return this.list;
    throw Error("c-admin-table: prop `viewModel` is required, and must be a ListViewModel.");
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

<style lang="scss">
  .c-admin-table {
    a {
      text-decoration: none;
    }
  }
</style>