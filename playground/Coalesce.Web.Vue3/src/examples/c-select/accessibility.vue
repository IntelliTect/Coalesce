<template>
  <h1>c-select with accessibility enhancements</h1>
  
  <h2>list-props Example</h2>
  <p>Adding aria-label and other props to the list</p>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        multiple
        v-model:keyValue="selectedIds"
        :list-props="{
          ariaLabel: 'List of people',
          tabindex: 0,
        }"
      />
    </v-col>
    <v-col>
      {{ selectedIds }}
    </v-col>
  </v-row>

  <h2>Custom list-item-complete slot with accessibility</h2>
  <p>Customizing the prepend slot to add inert to checkboxes</p>
  <v-row>
    <v-col>
      <c-select
        for="Person"
        multiple
        v-model:keyValue="selectedIds2"
        :list-props="{
          ariaLabel: 'List of people to select',
        }"
      >
        <template #list-item-complete="{ props, item, selected, select }">
          <v-list-item v-bind="props">
            <template #prepend>
              <!-- inert prevents the checkbox from being tab-navigable and interactive.
                   The parent v-list-item remains clickable for selection. -->
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
      {{ selectedIds2 }}
    </v-col>
  </v-row>

  <h2>Custom list-item-complete with additional info</h2>
  <p>Full control over list-item rendering with custom content</p>
  <v-row>
    <v-col>
      <c-select
        for="Case"
        multiple
        v-model:keyValue="selectedCaseIds"
        :list-props="{
          ariaLabel: 'List of cases',
        }"
      >
        <template #list-item-complete="{ props, item, selected, select }">
          <v-list-item v-bind="props">
            <template #prepend>
              <!-- inert prevents the checkbox from being tab-navigable and interactive.
                   The parent v-list-item remains clickable for selection. -->
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
            <v-list-item-subtitle v-if="item.assignedToName">
              Assigned to: {{ item.assignedToName }}
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
const selectedIds2 = ref<number[]>([]);
const selectedCaseIds = ref<number[]>([]);
</script>
