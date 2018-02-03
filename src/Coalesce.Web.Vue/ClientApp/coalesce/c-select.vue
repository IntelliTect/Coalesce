<template>
    <v-select
        :label="propMeta.displayName"
        :loading="loading"
        :items="listItems"
        :search-input.sync="search"
        :item-text="propMeta.model.displayProp.name"
        :item-value="propMeta.model.keyProp.name"
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

    
        <template slot="item" slot-scope="data">
            <!-- <c-display :item="data.item" /> -->
            <v-list-tile-content>
                <v-list-tile-title >
                    <c-display :item="data.item" :prop="propMeta.model.displayProp" />
                </v-list-tile-title>
            </v-list-tile-content>
        </template>
        <template slot="selection" slot-scope="data">
            <c-display :item="item" :prop="propMeta" class="input-group__selections__comma"/>
        </template>

    </v-select>
</template>

<script lang="ts">
  
    import { Vue, Component, Watch } from 'vue-property-decorator';
    import { ModelMetadata, ModelPropertyMetadata } from './metadata'
    //@ts-ignore
    import { VSelect } from 'vuetify/es5/components/VSelect';
    import CDisplay from './c-display';
    import MetadataComponent from './c-metadata-component'

    @Component({
        name: 'c-select',
        components: {
            CDisplay
        }
    })
    export default class extends MetadataComponent {
        public loading: boolean = false;
        public search: string | null = null;
        public items: any[] = [];

        get listItems() {
            var items = this.items.slice();
            var selected = (this.item as any)[this.propMeta.name];
            
            // Appending this to the bottom is intentional - chances are, if a person opens a dropdown that already has a value selected, they don't want to re-select the value that's already selected.
            if (selected) items.push(selected);
            
            return items;
        }
        
        @Watch('search')
        queryDropdownItems() {
            
            this.loading = true;
            // TODO: this url is obviously totally a hack.
            fetch(`http://localhost:11202/api/${this.propMeta.name}/List?pageSize=500&search=${this.search || ''}`, {
                credentials: 'include'  
            })
                .then(response => response.json())
                .then(resp => {
                    this.items = resp.list.map((i: any) => Object.assign(i, {$metadata: (this.propMeta as ModelPropertyMetadata).model}));
                    this.loading = false;
                });
        }

        mounted() {
            this.queryDropdownItems();
            this.loading = false;
        }
    }
</script>

