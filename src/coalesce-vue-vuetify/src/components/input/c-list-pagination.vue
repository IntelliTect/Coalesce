<template>
  <div class="c-list-pagination grey--text">
    <div>
      <c-list-page-size :list="list" :items="pageSizes" />
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
import CListPageSize from './c-list-page-size.vue';
import CListRangeDisplay from '../display/c-list-range-display.vue';

@Component({
  name: 'c-list-pagination',
  components: {
    CListPage,
    CListPageSize,
    CListRangeDisplay
  }
})
export default class extends Vue {
  @Prop({required: true})
  list!: ListViewModel<any,any>

  @Prop({required: false})
  pageSizes?: number[]
}
</script>



<style lang="scss" scoped>

  .c-list-pagination {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 16px;
  }
</style>

