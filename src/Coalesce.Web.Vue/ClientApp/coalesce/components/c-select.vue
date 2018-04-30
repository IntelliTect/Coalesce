<template>
  <v-select
    :label="propMeta.displayName"
    :loading="loading"
    :items="listItems"
    :search-input.sync="search"
    :item-text="propMeta.typeDef.displayProp.name"
    :item-value="propMeta.typeDef.keyProp.name"
    :return-object="true"
    v-model="item[propMeta.name]"
    :filter="() => true"
    @onKeyDown.delete.stop="item[propMeta.name] = null"
    autocomplete
    clearable
    open-on-clear
  dense
  >
  <!-- multiple chips -->

    <template slot="no-data">
      <!-- TODO: i18n -->
      <span>No data available</span>
    </template>
    <template slot="item" slot-scope="data">
      <!-- <c-display :item="data.item" /> -->
      <v-list-tile-content>
        <v-list-tile-title >
          <c-display :item="data.item" :prop="propMeta.typeDef.displayProp" />
        </v-list-tile-title>
      </v-list-tile-content>
    </template>
    <template slot="selection" slot-scope="data">
      <c-display :item="item" :prop="propMeta" class="input-group__selections__comma"/>
    </template>

  </v-select>
</template>

<script lang="ts" src="./c-select.ts"/>

