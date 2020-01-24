<template>
  <div class="pagination grey--text">
    <div>
      <div>
        Rows Per Page:
        <v-select
          class="ml-4 pt-0 page-size"
          :items="[10,25,100]"
          v-model="list.$params.pageSize"
          hide-details
          dense
          single-line
        ></v-select>
      </div>
    </div>

    <div >
      <c-list-range-display :list="list" />
    </div>

    <div>
      <c-list-page :list="list" />
    </div>
  </div>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { ListViewModel } from 'coalesce-vue';

import CListPage from './c-list-page.vue';
import CListRangeDisplay from '../display/c-list-range-display.vue';

@Component({
  name: 'c-list-pagination',
  components: {
    CListPage,
    CListRangeDisplay
  }
})
export default class extends Vue {
  @Prop({required: true})
  list!: ListViewModel<any,any>
}
</script>



<style lang="scss" scoped>

  .pagination {
    display: flex;

    .v-text-field {
      margin-top: 0px
    }
    
    > * {
      display: flex;
      align-items: center;
      flex-grow: 1;
      flex-basis:0;
      // margin: 0 10px;
      &:nth-child(2) > * {
        margin: auto;
      }
      &:nth-child(3) > * {
        margin-left: auto;
      }
      > * {
        display: flex;
        align-items: center;
        // margin: 0 10px;
      }
    }

    .page-size {
      width: 100px;
    }
    .current-page {
      width: 100px;
    }
  }
</style>

