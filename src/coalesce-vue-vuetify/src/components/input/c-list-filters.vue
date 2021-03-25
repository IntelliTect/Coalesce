<template>
  <v-menu
    class="c-list-filters--menu"
    :close-on-content-click="false"
    offset-y
  >
    <template #activator="{on}">
      <v-btn
        class="c-list-filters"
        text
        v-on="on"
      >
        <v-badge :value="activeCount" left :content="activeCount" color="accent">
          <v-icon :left="$vuetify.breakpoint.mdAndUp">fa fa-filter</v-icon>
        </v-badge>
        <span class="hidden-sm-and-down">Filters</span>
      </v-btn>
    </template>

    <v-card class="c-list-filters--card">
      <v-card-title>
        Filters
      </v-card-title>
      <v-card-text>
          
        <v-row 
          v-for="filter in definedFilters"
          :key="filter.key"
          no-gutters
          align-items="center"
          align="center"
          class="py-2"
        >
          <v-col 
            class="flex-grow-0 pr-4 " 
            style="max-width: 170px" 
          >
            <span class="">
              {{filter.displayName}}
            </span>

            <v-btn-toggle 
              class="d-block text-no-wrap" 
              :value="filter.isNull ? 1 : filter.isDefined && !filter.isNull ? 2 : null"
            >
              <v-btn 
                x-small
                @click="removeFilter(filter)"
                title="Remove Filter"
              >
                <i class="fa fa-times"></i>
              </v-btn>
              <v-btn 
                x-small @click="setFilter(filter, null)"
                title="Filter where value is null"
              >
                <pre>null</pre>
              </v-btn>
              <v-btn 
                x-small @click="setFilter(filter, '')"
                title="Set custom filter value"
              >
                <i class="fa fa-ellipsis-h"></i>
              </v-btn>
            </v-btn-toggle>
          </v-col>
          <v-col>
            <pre class="pl-5" v-if="filter.isNull">== null</pre>
            
            <!-- If the filter is a foreign key, and there's zero or one value specified,
              provide a dropdown input. -->
            <c-input
              v-else-if="filter.propMeta 
                && (filter.propMeta.role == 'foreignKey' || filter.propMeta.role == 'primaryKey')
                && (!filter.value || filter.value.toString().indexOf(',') == -1)"
              :key-value="filter.value"
              @update:key-value="value => setFilter(filter, value)"
              :for="filter.propMeta.role == 'primaryKey' ? list.$metadata.name : filter.propMeta.navigationProp"
              clearable hide-details
            >
            </c-input>

            <!-- Display an enum multiselect for enum props -->
            <v-select
              v-else-if="filter.propMeta && filter.propMeta.type == 'enum'"
              :value="parseValueArray(filter.value)"
              @input="value => setFilter(filter, value)"
              :items="filter.propMeta.typeDef.values"
              item-text="displayName"
              item-value="value"
              :label="filter.displayName"
              clearable hide-details multiple
            />
            
            <!-- Display a number multiselect for number props -->
            <v-combobox
              v-else-if="filter.propMeta && filter.propMeta.type == 'number'"
              :value="parseValueArray(filter.value)"
              @input="value => setFilter(filter, value)"
              :label="filter.displayName"
              clearable hide-details  disable-lookup
              multiple chips deletable-chips small-chips
            />

            <v-switch
              v-else-if="filter.propMeta && filter.propMeta.type == 'boolean'"
              :input-value="filter.value === 'true' ? true : filter.value === 'false' ? false : null"
              @change="value => setFilter(filter, value)"
              :label="filter.displayName"
              clearable hide-details  
            />

            <!-- Text field for everything else -->
            <v-text-field
              v-else
              :value="filter.value === 'null' ? null : filter.value"
              @input="value => setFilter(filter, value)"
              :label="filter.displayName"
              outlined dense clearable hide-details
            />
            
          </v-col>
        </v-row>
      </v-card-text>

      <v-card-title>
        Add Filters
      </v-card-title>
      <v-card-text>
        <v-chip-group column style="max-width: 400px">
          <v-chip
            v-for="filter in undefinedFilters"
            :key="filter.key"
            small color="primary"
            @click="addFilter(filter)"
          >
            {{filter.displayName}}
          </v-chip>
        </v-chip-group>
      </v-card-text>
      
    </v-card>
  </v-menu>
<!--   
  <div 
    v-if="list.$params.filter"
    class="c-list-filters"
  >
    <v-chip
      v-for="filter in undefinedFilters"
      :key="key"
      deletable
    >
      <span v-if="key in list.$metadata.props">
        {{list.$metadata.props[key].displayName}}:
      </span>
      <span v-else>
        {{key}}:
      </span>
      {{value}}
    </v-chip>
  </div> -->
</template>


<script lang="ts">
import type { ListViewModel, Property } from 'coalesce-vue';
import Vue, { PropOptions } from 'vue';

interface FilterInfo {
  key: string,
  displayName: string,
  isDefined: boolean,
  isActive: boolean,
  isNull: boolean,
  value?: any,
  propMeta?: Property
}

const filterTypes = ["string", "number", "boolean", "enum", "date"];

export default Vue.extend({
  name: 'c-list-filters',
  props: { 
    list: <PropOptions<ListViewModel>>{ required: true },
  },
  methods: {
    removeFilter(filterInfo: FilterInfo) {
      const copy: any = { ...this.list.$params.filter }
      delete copy[filterInfo.key];
      this.list.$params.filter = copy;
    },
    setFilter(filterInfo: FilterInfo, value: any) {
      if (Array.isArray(value)) {
        value = value.join(',')
      }
      const filter = this.list.$params.filter ?? {}
      this.$set(filter, filterInfo.key, value)
      this.list.$params.filter = filter;
    },
    addFilter(filterInfo: FilterInfo) {
      if (filterInfo.propMeta?.type == "boolean") {
        this.setFilter(filterInfo, true);
      } else {
        this.setFilter(filterInfo, '');
      }
    },
    parseValueArray(filterInfo: FilterInfo) {
      if (filterInfo.propMeta?.type == "enum" || filterInfo.propMeta?.type == "number") {
        // Enums use a real enum multiselect control, so we need to get our type ducks in a row
        return String(filterInfo.value)
          .split(',')
          .filter(v => v !== '')
          .map(v => isNaN(v as any) ? v : +v)
      } 
      return filterInfo.value;
    },
  },
  computed: {
    filters(): FilterInfo[] {
      const filter = this.list.$params.filter ?? {};
      const meta = this.list.$metadata;

      // Start with the set of valid filter properties
      const filterNames = Object.values(meta.props)
        .filter(p => filterTypes.includes(p.type) && !p.dontSerialize)
        .map(p => p.name);

      // Add in any actually existing filters that aren't represented by a prop.
      // This is unlikely, but someone can write a custom datasource that can look for any filter prop.
      for (const key in filter) {
        if (!filterNames.includes(key)) filterNames.push(key);
      }

      return filterNames
        .map(key => {
          const value = filter[key];
          const propMeta = meta.props[key] ?? null;

          return <FilterInfo>{
            key,
            value,
            propMeta,
            isNull: value === null || value === 'null',
            isDefined: key in filter,
            // Both undefined and emptystring are considered by Coalesce to be "not filtered".
            // `null` as a value is a filter that checks that the value is `null`.
            isActive: value !== '' && value !== undefined,
            displayName: propMeta?.role == "foreignKey"
              ? propMeta.navigationProp?.displayName
              : propMeta?.displayName ?? key
          }
        })
        .sort((a,b) => 
          // a.isActive != b.isActive ? (b.isActive ? 1 : -1) : 
          a.displayName.localeCompare(b.displayName)
        )
    },

    definedFilters(): FilterInfo[] {
      return this.filters.filter(info => info.isDefined)
    },

    undefinedFilters(): FilterInfo[] {
      return this.filters.filter(info => !info.isDefined)
    },

    activeFilters(): FilterInfo[] {
      return this.filters.filter(info => info.isActive)
    },

    activeCount(): number {
      return this.activeFilters.length;
    }
  }
})
</script>



<style lang="scss" >
  .c-list-filters--card {
    max-width: 500px;
  }
</style>

