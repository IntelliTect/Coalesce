<template>
  <v-text-field v-if="propMeta.type === 'string' || propMeta.type === 'number'" 
    v-model="item[propMeta.name]" 
    :label="propMeta.displayName"
    :type="propMeta.type">
  </v-text-field>

  <v-select v-else-if="propMeta.type === 'enum' "
    v-model="item[propMeta.name]" 
    :items="propMeta.values"
    :label="propMeta.displayName"
    item-text="displayName"
    autocomplete
    dense
  >
  </v-select>
  
  <c-select v-else-if="propMeta.type === 'model' "
    :item="item"
    :prop="prop"
  >
  </c-select>

  <div v-else-if="propMeta.type === 'collection' " @click="collectionEdit = true"> 
    <div tabindex="-1" data-uid="54" role="combobox" class="input-group input-group--dirty input-group--text-field input-group--select input-group--multiple primary--text">
      <label>{{propMeta.displayName}}</label>
      <div class="input-group__input">
        <div class="input-group__selections" style="overflow: hidden;">
            <c-display :item="item" :prop="propMeta" class="input-group__selections__comma"></c-display>
          <!-- <div v-for="(child, index) in item[propMeta.name]" :key="child[propMeta.model.keyProp.name]"
            class="input-group__selections__comma">
            <c-display :item="child"></c-display>
            <template v-if="index < item[propMeta.name].length - 1">, </template>
          </div> -->
        </div>
        <div class="menu menu--disabled" style="display: inline-block;"></div>
        <i aria-hidden="true" class="icon icon--disabled material-icons input-group__append-icon input-group__icon-cb">mode_edit</i>
      </div>
      <div class="input-group__details">
      <!---->
      </div>
    </div>

    <v-dialog v-model="collectionEdit" max-width="80%">
      <v-card>
        <v-toolbar color="primary" dark dense>
          <!-- <v-toolbar-side-icon></v-toolbar-side-icon> -->
          <v-toolbar-title>
            {{propMeta.displayName}}: 
            {{item.$metadata.displayName}}
            {{item[item.$metadata.displayProp.name]}}
          </v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn icon @click="collectionEdit=false"><v-icon>close</v-icon></v-btn>
        </v-toolbar>
        <v-card-text>
          <v-list >
            <v-list-tile avatar v-for="child in item[propMeta.name]" :key="child[propMeta.model.keyProp.name]">
              <v-list-tile-content>
                <v-list-tile-title>
                  <c-display :item="child"></c-display>
                </v-list-tile-title>
                <v-list-tile-sub-title>
                  {{propMeta.model.keyProp.displayName}}:
                  {{child[propMeta.model.keyProp.name]}}
                </v-list-tile-sub-title>
              </v-list-tile-content>
            </v-list-tile>
          </v-list>
        </v-card-text>
      <v-card-actions>
        <v-btn color="primary" flat @click.stop="collectionEdit=false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
<!-- 
    <v-select
      v-model="item[propMeta.name]" 
      :items="item[propMeta.name]"
      disabled
      :label="propMeta.displayName"
      :item-text="propMeta.model.displayProp.name"
      dense
      multiple
    >
    </v-select> -->
  </div>

  <div v-else>
    <c-display :item="item" :prop="prop"></c-display>
  </div>
</template>

<script lang="ts">

import { Vue, Component, Watch, Prop } from 'vue-property-decorator'
import { IHaveMetadata, PropertyMetadata, EnumPropertyMetadata, ModelPropertyMetadata } from './metadata'
import MetadataComponent from './c-metadata-component'
import CSelect from './c-select.vue'
import CDisplay from './c-display';

//@ts-ignore
import { VTextField } from 'vuetify/es5/components/VTextField';
    
@Component({
  name: 'c-input',
  components: {
    CSelect, CDisplay
  }
})
export default class extends MetadataComponent {
  collectionEdit = false;
}

</script>

