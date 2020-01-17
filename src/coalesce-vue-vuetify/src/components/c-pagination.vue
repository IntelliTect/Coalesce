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
      <div >
        <!-- These use list.$load explicitly so that the numbers match
        the actual currently displayed data, rather than the state of the parameters. -->
        {{(list.$load.page-1) * list.$load.pageSize + 1}}
        - 
        {{list.$load.page == list.$load.pageCount ? list.$load.totalCount : list.$load.page * list.$load.pageSize}}
        of
        {{list.$load.totalCount}}
      </div>
    </div>

    <div>
      <c-pagination-page :list="list" />
    </div>
  </div>
</template>

<script lang="ts">
import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { ListViewModel } from 'coalesce-vue';
import CPaginationPage from './c-pagination-page.vue';

@Component({
  name: 'c-pagination',
  components: {
    CPaginationPage
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

