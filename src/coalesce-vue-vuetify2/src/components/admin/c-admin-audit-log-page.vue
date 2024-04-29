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
          {{ listVm.$metadata.displayName }}
        </v-toolbar-title>

        <v-divider class="hidden-xs-only mx-4" vertical></v-divider>

        <v-btn
          class="c-admin-table-toolbar--button-reload"
          text
          @click="listVm.$load()"
        >
          <v-icon left>fa fa-sync-alt</v-icon>
          <span class="hidden-sm-and-down">Reload</span>
        </v-btn>

        <v-spacer></v-spacer>

        <span class="c-admin-table-toolbar--range hidden-sm-and-down">
          Showing <c-list-range-display :list="listVm" />
        </span>

        <v-spacer></v-spacer>

        <c-list-page class="c-admin-table-toolbar--page" :list="listVm" />

        <template v-slot:extension>
          <div class="c-audit-logs--filters">
            <v-text-field
              v-model="listVm.$params.filter.type"
              label="Type Name"
              style="width: 150px"
              @click:clear="$nextTick(() => (listVm.$params.filter.type = ''))"
              flat
              solo-inverted
              hide-details
              single-line
              clearable
              :persistent-placeholder="false"
            />

            <v-text-field
              v-model="listVm.$params.filter.keyValue"
              label="Key Value"
              style="width: 150px"
              @click:clear="
                $nextTick(() => (listVm.$params.filter.keyValue = ''))
              "
              flat
              solo-inverted
              hide-details
              single-line
              clearable
              :persistent-placeholder="false"
            />

            <c-input
              v-model="listVm.$params.filter.state"
              :for="listVm.$metadata.props.state"
              @click:clear="$nextTick(() => (listVm.$params.filter.state = ''))"
              style="min-width: 210px; max-width: 210px"
              flat
              solo-inverted
              hide-details
              single-line
              clearable
              :persistent-placeholder="false"
            />

            <c-select
              v-if="userPropMeta"
              :for="userPropMeta"
              :key-value.sync="
                listVm.$params.filter[userPropMeta.foreignKey.name]
              "
              style="width: 240px"
              @click:clear="
                $nextTick(
                  () =>
                    (listVm.$params.filter[userPropMeta.foreignKey.name] = '')
                )
              "
              flat
              solo-inverted
              hide-details
              single-line
              clearable
              :persistent-placeholder="false"
            />
          </div>

          <v-spacer></v-spacer>
          <c-list-page-size :list="listVm" />
        </template>
      </v-toolbar>

      <v-card-text>
        <c-loader-status
          :loaders="{
            'no-initial-content no-error-content': [listVm.$load],
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
                v-for="(auditLog, index) in listVm.$items"
                :key="auditLog.$stableId"
              >
                <td class="c-audit-logs--column-user-date">
                  <c-display
                    v-if="userPropMeta"
                    :model="auditLog"
                    element="div"
                    :for="userPropMeta.name"
                    class="text-no-wrap"
                  />
                  <c-display
                    :model="auditLog"
                    element="div"
                    for="date"
                    class="text-no-wrap text-caption d-block"
                    format="M/dd/yy h:mm:ss a z"
                    :title="
                      [
                        propDisplay(auditLog, 'date', {
                          format: {
                            format: 'yyyy-MM-dd HH:mm:ss.SSS z',
                          },
                        }),
                        propDisplay(auditLog, 'date', {
                          format: {
                            format: 'yyyy-MM-dd HH:mm:ss.SSS z',
                            timeZone: 'UTC',
                          },
                        }),
                        (auditLog.date.valueOf() / 1000).toFixed(3) +
                          ' (Unix Seconds)',
                      ].join('\n')
                    "
                  />
                  <pre
                    :class="timeDiffClass(auditLog, listVm.$items[index + 1])"
                    v-text="timeDiff(auditLog, listVm.$items[index + 1])"
                    title="Time delta from the preceding row"
                  ></pre>
                </td>

                <td class="c-audit-logs--column-entity">
                  <pre title="Entity Type">{{ auditLog.type }}</pre>
                  <pre
                    style="
                      white-space: break-spaces;
                      display: inline-block;
                      vertical-align: top;
                    "
                    title="Entity Key"
                    >{{ auditLog.keyValue }}</pre
                  >
                  <div
                    v-if="auditLog.description"
                    v-text="auditLog.description"
                    class="c-audit-logs--entry-desc text-grey"
                  ></div>
                </td>

                <td
                  class="c-audit-logs--column-details c-audit-logs--row-detail"
                >
                  <slot name="row-detail" :item="auditLog">
                    <table>
                      <tr class="prop-state">
                        <td>Change:</td>
                        <td><c-display :model="auditLog" for="state" /></td>
                      </tr>
                      <tr
                        v-for="propMeta in otherProps"
                        :key="auditLog.$stableId + '-prop-' + propMeta.name"
                        :class="'prop-' + propMeta.name"
                      >
                        <td>{{ propMeta.displayName }}:</td>
                        <td>
                          <c-display
                            :model="auditLog"
                            :for="propMeta"
                            class="grey--text"
                          />
                        </td>
                      </tr>
                      <slot name="row-detail-append" :item="auditLog" />
                    </table>
                  </slot>
                </td>

                <td
                  class="c-audit-logs--column-properties c-audit-logs--row-properties"
                >
                  <table>
                    <thead>
                      <tr>
                        <th class="c-audit-logs--property-name">Property</th>
                        <th>Old</th>
                        <th>New</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="prop in auditLog.properties" :key="prop.id">
                        <td class="c-audit-logs--property-name">
                          {{ prop.propertyName }}
                        </td>
                        <td
                          class="c-audit-logs--property-value c-audit-logs--property-old"
                        >
                          <!-- Extra span is required for preserving whitespace -->
                          <span>{{ prop.oldValue }}</span>
                          <span
                            class="c-audit-logs--property-value-desc grey--text"
                            v-if="prop.oldValueDescription"
                            v-text="' (' + prop.oldValueDescription + ')'"
                          ></span>
                        </td>
                        <td
                          class="c-audit-logs--property-value c-audit-logs--property-old"
                        >
                          <!-- Extra span is required for preserving whitespace -->
                          <span>{{ prop.newValue }}</span>
                          <span
                            class="c-audit-logs--property-value-desc grey--text"
                            v-if="prop.newValueDescription"
                            v-text="' (' + prop.newValueDescription + ')'"
                          ></span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
            </tbody>
          </v-simple-table>

          <v-divider />
          <c-list-pagination :list="listVm" class="mt-4" />
        </c-loader-status>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script lang="ts">
import { computed, defineComponent } from "vue";
import { differenceInMilliseconds } from "date-fns";
import {
  HiddenAreas,
  ListViewModel,
  ModelApiClient,
  ModelReferenceNavigationProperty,
  ModelType,
  propDisplay,
  useBindToQueryString,
  ViewModel,
} from "coalesce-vue";

interface AuditLogBase {
  $metadata: ModelType;
  id: number | null;
  type: string | null;
  keyValue: string | null;
  description: string | null;
  date: Date | null;
  state: number | null;
  properties: {
    $metadata: ModelType;
    id: number | null;
    parentId: number | null;
    propertyName: string | null;
    oldValue: string | null;
    oldValueDescription: string | null;
    newValue: string | null;
    newValueDescription: string | null;
  }[];
}

type AuditLogViewModel = ViewModel<AuditLogBase> & AuditLogBase;
type AuditLogListViewModel = ListViewModel<
  AuditLogBase,
  ModelApiClient<AuditLogBase>,
  AuditLogViewModel
>;

export default defineComponent({
  props: {
    type: { type: String, required: false },
    color: { type: String, required: false, default: "primary" },
    list: { type: Object, required: false },
  },
  setup(props) {
    let listVm: AuditLogListViewModel;
    if (props.list) {
      listVm = props.list as any;
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
      listVm = new ListViewModel.typeLookup![props.type]() as any;
      listVm.$load.setConcurrency("cancel");
    }

    const userPropMeta = computed(() => {
      return (
        Object.values(listVm.$metadata.props)
          .filter(
            (p): p is ModelReferenceNavigationProperty =>
              p.role == "referenceNavigation"
          )
          // FUTURE: Could there be other props that we detect as representing a user?
          .filter((p) =>
            ["user", "createdby", "changedby"].some((needle) =>
              p.name.toLowerCase().includes(needle)
            )
          )[0]
      );
    });

    const otherProps = computed(() => {
      return Object.values(listVm.$metadata.props).filter(
        (p) =>
          ((p.hidden || 0) & HiddenAreas.List) != HiddenAreas.List &&
          p != userPropMeta.value &&
          ![
            "id",
            "type",
            "keyValue",
            "date",
            "state",
            "properties",
            "description",
          ].includes(p.name) &&
          (p.role !== "foreignKey" || !p.navigationProp)
      );
    });

    function timeDiff(current: AuditLogViewModel, older?: AuditLogViewModel) {
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
      current: AuditLogViewModel,
      older?: AuditLogViewModel
    ) {
      if (!older) return "";
      const diff = current.date!.valueOf() - (older?.date ?? 0).valueOf();
      return diff == 0
        ? "grey--text"
        : diff > 0
        ? "success--text"
        : "error--text";
    }

    let filter = {
      type: "",
      keyValue: "",
      state: "",
    } as { [s: string]: string };

    if (userPropMeta.value) {
      const fkName = userPropMeta.value.foreignKey.name;
      filter[fkName] = "";
      useBindToQueryString(filter, fkName, "user");
    }

    listVm.$params.filter = filter;

    useBindToQueryString(filter, "type");
    useBindToQueryString(filter, "keyValue");
    useBindToQueryString(filter, "state");
    useBindToQueryString(listVm.$params, "page", "page", (p) => +p);
    useBindToQueryString(listVm.$params, "pageSize", "pageSize", (p) => +p);

    listVm.$load();
    listVm.$useAutoLoad({ wait: 100 });

    const pageTitle = "Audit Logs";

    return {
      listVm,
      timeDiff,
      timeDiffClass,
      propDisplay,
      userPropMeta,
      otherProps,
      pageTitle,
    };
  },
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

.c-audit-logs--column-entity {
  min-width: 180px;
}

.c-audit-logs--row-detail {
  @include small;
  font-family: monospace;
  table {
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

.c-audit-logs--entry-desc {
  @include small;
}

.c-audit-logs--row-properties table {
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
    > * {
      // Pre-wrap must go on inner spans because Prettier won't preserve whitespace between tags
      // that are children of a `td` when it formats.
      white-space: pre-wrap;
    }
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
  tr:hover td {
    background: rgba(0, 0, 0, 0.1);
  }
}
</style>
