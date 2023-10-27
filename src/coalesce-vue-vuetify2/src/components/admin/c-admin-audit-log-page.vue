<template>
  <v-container fluid class="c-admin-audit-log-page">
    <v-card class="c-admin-audit-log-page">
      <v-toolbar
        extended
        class="c-admin-table-toolbar"
        dense
        :color="color"
        dark
      >
        <v-toolbar-title
          class="c-admin-table-toolbar--model-name hidden-xs-only"
        >
          {{ list.$metadata.displayName }}
        </v-toolbar-title>

        <v-divider class="hidden-xs-only mx-4" vertical></v-divider>

        <v-btn
          class="c-admin-table-toolbar--button-reload"
          text
          @click="list.$load()"
        >
          <v-icon left>fa fa-sync-alt</v-icon>
          <span class="hidden-sm-and-down">Reload</span>
        </v-btn>

        <v-spacer></v-spacer>

        <span class="c-admin-table-toolbar--range hidden-sm-and-down">
          Showing <c-list-range-display :list="list" />
        </span>

        <v-spacer></v-spacer>

        <c-list-page class="c-admin-table-toolbar--page" :list="list" />

        <template v-slot:extension>
          <div class="c-audit-logs--filters">
            <v-text-field
              v-model="list.$params.filter.type"
              label="Type Name"
              style="width: 150px"
              @clear="$nextTick(() => (list.$params.filter.type = ''))"
              flat
              solo-inverted
              hide-details
              single-line
              clearable
            />

            <v-text-field
              v-model="list.$params.filter.keyValue"
              label="Key Value"
              style="width: 150px"
              @clear="$nextTick(() => (list.$params.filter.keyValue = ''))"
              flat
              solo-inverted
              hide-details
              single-line
              clearable
            />

            <c-input
              v-model="list.$params.filter.state"
              :for="list.$metadata.props.state"
              @clear="$nextTick(() => (list.$params.filter.state = ''))"
              style="min-width: 210px; max-width: 210px"
              flat
              solo-inverted
              hide-details
              single-line
              clearable
            />

            <c-select
              v-if="userPropMeta"
              :for="userPropMeta"
              v-model:key-value="
                list.$params.filter[userPropMeta.foreignKey.name]
              "
              style="width: 240px"
              @clear="
                $nextTick(
                  () => (list.$params.filter[userPropMeta.foreignKey.name] = '')
                )
              "
              flat
              solo-inverted
              hide-details
              single-line
              clearable
            />
          </div>

          <v-spacer></v-spacer>
          <c-list-page-size :list="list" />
        </template>
      </v-toolbar>

      <v-card-text>
        <c-loader-status
          :loaders="{
            'no-initial-content no-error-content': [list.$load],
          }"
        >
          <v-simple-table class="c-audit-logs--table">
            <thead>
              <tr>
                <th class="c-audit-logs--column-user-date">By/Date</th>
                <th class="c-audit-logs--column-entity">Entity</th>
                <th class="c-audit-logs--column-details">Details</th>
                <th class="c-audit-logs--column-properties">Properties</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(objectChange, index) in list.$items"
                :key="objectChange.$stableId"
              >
                <td class="c-audit-logs--column-user-date">
                  <c-display
                    v-if="userPropMeta"
                    :model="objectChange"
                    element="div"
                    :for="userPropMeta.name"
                    class="text-no-wrap"
                  />
                  <c-display
                    :model="objectChange"
                    element="div"
                    for="date"
                    class="text-no-wrap text-caption d-block"
                    format="M/dd/yy h:mm:ss.SSS a"
                  />
                  <pre
                    :class="timeDiffClass(objectChange, list.$items[index + 1])"
                    v-text="timeDiff(objectChange, list.$items[index + 1])"
                    title="Time delta from the preceding row"
                  ></pre>
                </td>

                <td class="c-audit-logs--column-entity">
                  <pre>Type: <c-display :model="objectChange" for="type" /></pre>
                  <pre> Key: <c-display 
                    :model="objectChange"       
                    for="keyValue" 
                    style="    
                      white-space: break-spaces;
                      display: inline-block;
                      padding-right: 40px;
                      vertical-align: top;
                    "/></pre>
                </td>

                <td class="c-audit-logs--column-details">
                  <slot name="row-detail" :item="objectChange">
                    <table>
                      <tr class="prop-state">
                        <td>Change:</td>
                        <td><c-display :model="objectChange" for="state" /></td>
                      </tr>
                      <tr
                        v-for="propMeta in otherProps"
                        :key="objectChange.$stableId + '-prop-' + propMeta.name"
                        :class="'prop-' + propMeta.name"
                      >
                        <td>{{ propMeta.displayName }}:</td>
                        <td>
                          <c-display
                            :model="objectChange"
                            :for="propMeta"
                            class="grey--text"
                          />
                        </td>
                      </tr>
                      <slot name="row-detail-append" :item="objectChange" />
                    </table>
                  </slot>
                </td>

                <td class="c-audit-logs--column-properties">
                  <table>
                    <thead>
                      <tr>
                        <th class="c-audit-logs--property-name">Property</th>
                        <th>Old</th>
                        <th>New</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="prop in objectChange.properties"
                        :key="prop.id"
                      >
                        <td class="c-audit-logs--property-name">
                          <c-display :model="prop" for="propertyName" />
                        </td>
                        <td class="c-audit-logs--property-value">
                          <c-display :model="prop" for="oldValue" />
                        </td>
                        <td class="c-audit-logs--property-value">
                          <c-display :model="prop" for="newValue" />
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
            </tbody>
          </v-simple-table>

          <v-divider />
          <c-list-pagination :list="list" class="mt-4" />
        </c-loader-status>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { differenceInMilliseconds } from "date-fns";
import {
  HiddenAreas,
  ListViewModel,
  ModelApiClient,
  ModelReferenceNavigationProperty,
  ModelType,
  useBindToQueryString,
  ViewModel,
} from "coalesce-vue";

interface ObjectChangeBase {
  $metadata: ModelType;
  id: number | null;
  type: string | null;
  keyValue: string | null;
  date: Date | null;
  state: number | null;
  properties: {
    $metadata: ModelType;
    id: number | null;
    parentId: number | null;
    oldValue: string | null;
    newValue: string | null;
  }[];
}

type ObjectChangeViewModel = ViewModel<ObjectChangeBase> & ObjectChangeBase;
type ObjectChangeListViewModel = ListViewModel<
  ObjectChangeBase,
  ModelApiClient<ObjectChangeBase>,
  ObjectChangeViewModel
>;

const props = withDefaults(
  defineProps<{
    type?: string;
    color?: string;
    list?: string;
  }>(),
  { color: "primary" }
);

let list: ObjectChangeListViewModel;
if (props.list) {
  list = props.list as any;
} else {
  if (!props.type) {
    throw Error(
      "c-admin-audit-log-page: If prop `list` is not provided, `type` is required."
    );
  } else if (!ListViewModel.typeLookup![props.type]) {
    throw Error(
      `No model named ${props.type} is registered to ListViewModel.typeLookup`
    );
  }
  list = new ListViewModel.typeLookup![props.type]() as any;
}

const userPropMeta = computed(() => {
  return (
    Object.values(list.$metadata.props)
      .filter(
        (p): p is ModelReferenceNavigationProperty =>
          p.role == "referenceNavigation"
      )
      // FUTURE: Could there be other props that we detect as representing a user?
      .filter((p) => ["user"].includes(p.name.toLowerCase()))[0]
  );
});

const otherProps = computed(() => {
  return Object.values(list.$metadata.props).filter(
    (p) =>
      ((p.hidden || 0) & HiddenAreas.List) != HiddenAreas.List &&
      p != userPropMeta.value &&
      !["id", "type", "keyValue", "date", "state", "properties"].includes(
        p.name
      ) &&
      (p.role !== "foreignKey" || !p.navigationProp)
  );
});

function timeDiff(
  current: ObjectChangeViewModel,
  older?: ObjectChangeViewModel
) {
  if (!older) return "";
  let ms = differenceInMilliseconds(current.date!, older.date!);
  const positive = ms >= 0;
  ms = Math.abs(ms);

  var totalSec = ms / 1000;
  var hours = Math.floor(totalSec / 3600);
  var minutes = Math.floor((totalSec - hours * 3600) / 60);
  var seconds = totalSec - hours * 3600 - minutes * 60;

  return (
    (positive ? "+" : "-") +
    (hours > 0 ? hours + "h " : "") +
    (hours > 0 || minutes > 0 ? minutes + "m " : "") +
    seconds.toFixed(2) +
    "s"
  );
}

function timeDiffClass(
  current: ObjectChangeViewModel,
  older?: ObjectChangeViewModel
) {
  if (!older) return "";
  const diff = current.date!.valueOf() - (older?.date ?? 0).valueOf();
  return diff == 0 ? "grey--text" : diff > 0 ? "success--text" : "error--text";
}

let filter = {
  type: "",
  keyValue: "",
  state: "",
};

if (userPropMeta.value) {
  const fkName = userPropMeta.value.foreignKey.name;
  //@ts-ignore
  filter[fkName] = "";
  useBindToQueryString(filter, fkName, "user");
}

list.$params.filter = filter;

useBindToQueryString(filter, "type");
useBindToQueryString(filter, "keyValue");
useBindToQueryString(filter, "state");
useBindToQueryString(list.$params, "page", "page", (p) => +p);
useBindToQueryString(list.$params, "pageSize", "pageSize", (p) => +p);

list.$load();
list.$useAutoLoad({ wait: 0 });

defineExpose({
  /** Support for common convention of exposing 'pageTitle' from router-view hosted components. */
  pageTitle: "Audit Logs",
});
</script>

<style lang="scss">
@mixin small {
  font-size: 0.75rem !important;
  font-weight: 400;
  letter-spacing: 0.0333333333em !important;
  line-height: 1rem;
}

.c-audit-logs--filters {
  display: flex;
  flex-wrap: nowrap;
  flex-grow: 1;
  > * {
    margin-right: 16px !important;
  }
}

.c-audit-logs--table {
  --v-table-header-height: auto !important;

  > .v-data-table__wrapper > table > tbody > tr > td {
    padding-top: 4px;
    padding-bottom: 4px;
  }
  th {
    vertical-align: bottom;
  }
  td {
    vertical-align: top;
  }
}

.c-audit-logs--column-details {
  table {
    @include small;
    font-family: monospace;
    td {
      white-space: pre-wrap;
      vertical-align: top;
      padding: 0 2px;

      &:first-child {
        white-space: nowrap;
        text-align: right;
      }
    }
  }
}

.c-audit-logs--column-properties table {
  @include small;
  table-layout: fixed;
  border-spacing: 0;
  width: 100%;
  text-align: left;

  min-width: 500px;

  .c-audit-logs--property-name {
    width: 170px;
    white-space: nowrap;
  }
  .c-audit-logs--property-value {
    white-space: pre-wrap;
    overflow: auto;
    min-width: 100px;
  }

  th,
  td {
    padding: 1px 6px;
    line-height: 1.25rem;
  }
  th {
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
  }
  tr:not(:last-child) td {
    border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  }
}
</style>