<template>
  <div class="c-table" :class="{ 'c-table--editable': editable }">
    <c-loader-status
      :loaders="{ 'no-initial-content': [list.$load], ...loaders }"
    >
      <v-table style="--v-table-header-height: 40px">
        <thead>
          <tr>
            <th
              v-for="header in headers"
              :key="'header-' + header.value"
              class="text-left"
              :class="{
                sortable: header.sortable,
              }"
              @click="header.sortable ? orderByToggle(header.value) : void 0"
            >
              {{ header.text }}
              <v-icon v-if="list.$params.orderBy == header.value">
                fa fa-caret-up
              </v-icon>
              <v-icon
                v-else-if="list.$params.orderByDescending == header.value"
              >
                fa fa-caret-down
              </v-icon>
            </th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="(item, index) in list.$items" :key="index">
            <td
              v-for="prop in effectiveProps"
              :key="prop.name"
              :class="['prop-' + prop.name]"
              class="text-xs-left"
            >
              <c-input
                v-if="editable"
                :model="item"
                :for="prop"
                :readonly="isPropReadOnly(prop, item)"
                label=""
                hide-details="auto"
                hint=""
                density="compact"
                variant="outlined"
              >
                <c-admin-display v-if="admin" :model="item" :for="prop" />
                <c-display v-else :model="item" :for="prop" />
              </c-input>

              <c-admin-display v-else-if="admin" :model="item" :for="prop" />
              <c-display v-else :model="item" :for="prop" />
            </td>
            <slot name="item-append" :item="item" />
          </tr>
        </tbody>
      </v-table>
    </c-loader-status>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import {
  ListViewModel,
  Property,
  ModelType,
  HiddenAreas,
  ViewModel,
} from "coalesce-vue";

import { isPropReadOnly } from "../../util";

export default defineComponent({
  name: "c-table",

  props: {
    list: { required: true, type: Object as PropType<ListViewModel<any, any>> },
    props: { required: false, type: Array as PropType<Array<string>> },
    admin: { required: false, type: Boolean },
    editable: { required: false, type: Boolean },
    extraHeaders: { required: false, type: Array as PropType<Array<string>> },
    loaders: { required: false, type: Object },
  },

  computed: {
    metadata(): ModelType {
      return this.list.$metadata;
    },

    effectiveProps() {
      if (this.props && this.props.length) {
        return this.props
          .map((propName) => this.metadata.props[propName])
          .filter((prop) => !!prop);
      }

      return Object.values(this.metadata.props).filter(
        (p) => p.hidden === undefined || (p.hidden & HiddenAreas.List) == 0
      );
    },

    headers() {
      return [
        ...this.effectiveProps.map((o) => ({
          text: o.displayName,
          value: o.name,
          sortable: o.type != "collection",
          align: "left",
        })),
        ...(this.extraHeaders || []).map((h) => ({
          text: h,
          value: h,
          sortable: false,
        })),
      ];
    },
  },

  methods: {
    isPropReadOnly(p: Property, model: ViewModel) {
      return isPropReadOnly(p, model);
    },

    // TODO: put orderByToggle on ListViewModel.
    orderByToggle(field: string) {
      const list = this.list;
      const params = list.$params;

      if (params.orderBy == field && !params.orderByDescending) {
        params.orderBy = null;
        params.orderByDescending = field;
      } else if (!params.orderBy && params.orderByDescending == field) {
        params.orderBy = null;
        params.orderByDescending = null;
      } else {
        params.orderBy = field;
        params.orderByDescending = null;
      }
    },
  },
});
</script>

<style lang="scss">
.c-table {
  word-break: initial;

  img {
    // Images can come from c-display for a property with subtype `image-url`.
    max-height: 50px;
    max-width: 150px;
  }
  &.c-table--editable {
    td {
      padding: 0px 0px !important;
    }
    th,
    td {
      .v-input {
        min-width: 120px;
      }
      .v-input__slot {
        margin-bottom: 0px;
      }
      .v-field__input {
        padding-left: 6px;
        padding-right: 6px;
      }
      // Remove extra padding on the top of v-switch components
      // so they align nicely with other components.
      .v-input--switch,
      .v-input--checkbox {
        margin-top: 6px;
      }

      textarea {
        line-height: 1.1;
      }
    }
    td {
      vertical-align: top;
    }
    td > *:not(.v-input) {
      padding: 4px 8px;
    }
  }

  th {
    vertical-align: bottom;
    .v-icon {
      font-size: 16px;
    }
  }
}
</style>
