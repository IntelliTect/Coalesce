<template>
  <div class="c-pagination-page">
    <v-btn 
      color="accent" 
      outlined small icon
      @click="list.$previousPage()" 
      :disabled="!list.$hasPreviousPage">
      <v-icon>fa fa-angle-left</v-icon>
    </v-btn>
    
    Page 
    <v-text-field
      class="pt-0 d-inline-block current-page"
      type="number"
      min="1"
      :max="list.$load.pageCount"
      v-model.number="list.$page"
      hide-details outlined dense
    ></v-text-field>
    <span class="text-no-wrap ml-0">of {{list.$load.pageCount}}</span>

    <v-btn 
      color="accent" 
      outlined small icon
      @click="list.$nextPage()" 
      :disabled="!list.$hasNextPage">
      <v-icon>fa fa-angle-right</v-icon>
    </v-btn>
  </div>
</template>


<script lang="ts">
export default {
  name: 'c-pagination-page',
  props: { 
    list: { required: true },
  }
}
</script>



<style lang="scss" >
.c-pagination-page {
  display: flex;
  align-items: center;
  font-size: 16px;
  // margin: 0 10px;
  > * {
    margin: 0 0.5em !important;
  }

  $height: 30px;
  .current-page {
    width: 100px;

    .v-input__slot {
      height: unset !important;
      min-height: unset !important;

      input {
        padding: 6px 0 !important
      }
      
      .v-input__append-inner {
        margin-top: 4px;
      }

      // https://css-tricks.com/snippets/css/turn-off-number-input-spinners/
      input[type=number]::-webkit-inner-spin-button, 
      input[type=number]::-webkit-outer-spin-button { 
        -webkit-appearance: none; 
        margin: 0; 
      }
    }
  }


}
</style>

