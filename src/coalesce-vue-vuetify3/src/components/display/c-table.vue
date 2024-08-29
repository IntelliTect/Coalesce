<template>
  <div ref="cTable" class="c-table" :class="{ 'c-table--editable': editable }">
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
                ['fixed-column-right']:
                  header.isFixed && isHorizontalScrollbarVisible,
                sortable: header.sortable,
                ['prop-' + header.prop]: !!header.prop,
                ['th-' + header.value]: !header.prop,
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
          <tr
            v-for="(item, index) in list.$modelOnlyMode
              ? list.$modelItems
              : list.$items"
            :key="index"
          >
            <slot name="item-prepend" :item="item" />
            <td
              v-for="prop in effectiveProps"
              :key="prop.name"
              :class="['prop-' + prop.name]"
              class="text-xs-left"
            >
              <c-input
                v-if="editable && !isPropReadOnly(prop, item)"
                :model="item"
                :for="prop"
                label=""
                hide-details="auto"
                hint=""
                density="compact"
                variant="outlined"
                rows="1"
                auto-grow
              >
                <!-- Rows and auto-grow for textarea inputs -->
                <c-admin-display v-if="admin" :model="item" :for="prop" />
                <c-display v-else :model="item" :for="prop" />
              </c-input>

              <c-admin-display v-else-if="admin" :model="item" :for="prop" />
              <c-display v-else :model="item" :for="prop" />
            </td>
            <slot
              name="item-append"
              :item="item"
              :isHorizontalScrollbarVisible="isHorizontalScrollbarVisible"
            />
          </tr>
        </tbody>
      </v-table>
    </c-loader-status>
  </div>
</template>

<script lang="ts">
import {
  defineComponent,
  onBeforeMount,
  onMounted,
  onUnmounted,
  PropType,
  ref,
} from "vue";
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
    extraHeaders: {
      required: false,
      type: Array as
        | PropType<Array<{ header: string; isFixed: boolean }>>
        | PropType<Array<string>>,
    },
    loaders: { required: false, type: Object },
  },

  setup() {
    const cTable = ref<HTMLDivElement | null>(null);
    const isHorizontalScrollbarVisible = ref(false);

    const checkHorizontalScrollbar = () => {
      const divElement = cTable.value;
      const tableElement = divElement?.querySelector("table");
      if (tableElement && divElement) {
        isHorizontalScrollbarVisible.value =
          divElement.clientWidth < tableElement.clientWidth;
      }
    };

    const resizeObserver = new ResizeObserver(() => {
      checkHorizontalScrollbar();
    });

    onMounted(async () => {
      if (cTable.value) {
        resizeObserver.observe(cTable.value);
      }
      checkHorizontalScrollbar();
    });

    onUnmounted(() => {
      if (cTable.value) {
        resizeObserver.unobserve(cTable.value);
      }
    });

    return {
      cTable,
      isHorizontalScrollbarVisible,
    };
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
          prop: o.name,
          isFixed: false,
        })),
        ...(this.extraHeaders || []).map((h) => {
          if (typeof h === "string") {
            return {
              text: h,
              value: h,
              sortable: false,
              prop: undefined,
              isFixed: false,
            };
          } else {
            return {
              text: h.header,
              value: h.header,
              sortable: false,
              prop: undefined,
              isFixed: h.isFixed,
            };
          }
        }),
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
  &.c-table--editable .v-table tr {
    td {
      padding: 0px 0px;
    }
    th {
      padding: 0px 8px;
    }
    td {
      .v-field {
        font-size: 14px;
      }
      .v-input {
        min-width: 120px;
      }
      .v-switch {
        margin-left: 16px;
      }
      .v-input__slot {
        margin-bottom: 0px;
      }
      .v-field__input {
        padding-left: 6px;
        padding-right: 6px;
      }

      textarea {
        line-height: 1.1;
      }
    }
    td:not(.c-admin-table--actions) > *:not(.v-input) {
      display: block;
      padding: 10px 8px;
    }
  }

  th {
    vertical-align: bottom;
    .v-icon {
      font-size: 16px;
    }
  }

  .fixed-column-right {
    position: sticky;
    right: 0;
    background: rgb(var(--v-theme-surface-light));
    box-shadow: -2px 2px 4px 0px rgba(0, 0, 0, 0.4);
  }
}
</style>
