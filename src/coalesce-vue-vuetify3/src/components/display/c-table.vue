<template>
  <div ref="cTable" class="c-table" :class="{ 'c-table--editable': editable }">
    <c-loader-status
      :loaders="{ 'no-initial-content': [listVm.$load], ...loaders }"
    >
      <v-table style="--v-table-header-height: 40px" v-bind="tableProps">
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
              @click="header.sortable ? orderByToggle(header.value) : undefined"
            >
              {{ header.text }}
              <v-icon v-if="listVm.$params.orderBy == header.value">
                fa fa-caret-up
              </v-icon>
              <v-icon
                v-else-if="listVm.$params.orderByDescending == header.value"
              >
                fa fa-caret-down
              </v-icon>
            </th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="(item, index) in listVm.$modelOnlyMode
              ? listVm.$modelItems
              : listVm.$items"
            :key="index"
            @click="onRowClick($event, item)"
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
                validate-on="eager"
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

<script setup lang="ts" generic="TList extends ListViewModel = ListViewModel">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { ListViewModel, ModelType, HiddenAreas } from "coalesce-vue";
import { VTable } from "vuetify/components";
import { isPropReadOnly } from "../../util";

const props = defineProps<{
  list: TList;

  /** The names of properties on the listed items to be shown as columns. */
  props?: string[];
  admin?: boolean;
  editable?: boolean;
  extraHeaders?: Array<{ header: string; isFixed: boolean }> | Array<string>;
  loaders?: any;

  /** Additional props to pass to VTable */
  tableProps?: VTable["$props"];
}>();

type ViewModelType = TList extends ListViewModel<any, any, infer TViewModel>
  ? TViewModel
  : never;

defineSlots<{
  ["item-prepend"]?(props: { item: ViewModelType }): any;
  ["item-append"]?(props: {
    item: ViewModelType;
    isHorizontalScrollbarVisible: boolean;
  }): any;
}>();

const emit = defineEmits<{ "click:item": [arg: ViewModelType] }>();

// Silly wrapper because using `list` directly in the template
// has Typescript bugs right now in vue-language-tools.
const listVm = computed(() => props.list);

const cTable = ref<HTMLDivElement>();
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

onMounted(() => {
  if (cTable.value) {
    resizeObserver.observe(cTable.value);
  }
  checkHorizontalScrollbar();
});

onUnmounted(() => {
  resizeObserver.disconnect();
});

const metadata = computed((): ModelType => props.list.$metadata);

const effectiveProps = computed(() => {
  if (props.props?.length) {
    return props.props
      .map((propName) => metadata.value.props[propName])
      .filter((prop) => !!prop);
  }

  return Object.values(metadata.value.props).filter(
    (p) => p.hidden === undefined || (p.hidden & HiddenAreas.List) == 0
  );
});

const headers = computed(() => {
  return [
    ...effectiveProps.value.map((o) => ({
      text: o.displayName,
      value: o.name,
      sortable: o.type != "collection",
      align: "left",
      prop: o.name,
      isFixed: false,
    })),
    ...(props.extraHeaders || []).map((h) => {
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
});

// TODO: put orderByToggle on ListViewModel.
function orderByToggle(field: string) {
  const list = props.list;
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
}

function onRowClick(event: MouseEvent, item: ViewModelType) {
  if (props.editable) return;

  if (event.target instanceof HTMLElement && event.target.closest("a,button")) {
    return;
  }

  emit("click:item", item);
}
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
