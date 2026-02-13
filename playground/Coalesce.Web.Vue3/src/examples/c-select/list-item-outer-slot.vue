<template>
  <h1>c-select list-item-outer slot</h1>

  <h2>Custom checkbox behavior</h2>
  <p>
    Using list-item-outer to customize the prepend slot, e.g. adding inert to
    checkboxes so they aren't independently tab-navigable.
  </p>
  <v-row>
    <v-col>
      <c-select for="Person" multiple v-model:keyValue="selectedIds">
        <template #list-item-outer="{ props, item, selected, select }">
          <v-list-item v-bind="props">
            <template #prepend>
              <v-list-item-action start inert>
                <v-checkbox-btn
                  :model-value="selected"
                  @update:model-value="select"
                  tabindex="-1"
                />
              </v-list-item-action>
            </template>
            <v-list-item-title>
              {{ item.name }}
            </v-list-item-title>
          </v-list-item>
        </template>
      </c-select>
    </v-col>
    <v-col>
      {{ selectedIds }}
    </v-col>
  </v-row>

  <h2>Custom list-item-outer with additional info</h2>
  <p>Full control over list-item rendering with custom content</p>
  <v-row>
    <v-col>
      <c-select for="Case" multiple v-model:keyValue="selectedCaseIds">
        <template #list-item-outer="{ props, item, selected, select }">
          <v-list-item v-bind="props">
            <template #prepend>
              <v-list-item-action start inert>
                <v-checkbox-btn
                  :model-value="selected"
                  @update:model-value="select"
                  tabindex="-1"
                />
              </v-list-item-action>
            </template>
            <v-list-item-title>
              {{ item.title }} ({{ item.caseKey }})
            </v-list-item-title>
            <v-list-item-subtitle v-if="item.assignedTo">
              Assigned to: {{ item.assignedTo.name }}
            </v-list-item-subtitle>
          </v-list-item>
        </template>
      </c-select>
    </v-col>
    <v-col>
      {{ selectedCaseIds }}
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref } from "vue";

const selectedIds = ref<number[]>([]);
const selectedCaseIds = ref<number[]>([]);
</script>
