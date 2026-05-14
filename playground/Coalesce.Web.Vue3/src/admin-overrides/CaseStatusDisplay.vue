<template>
  <!-- Custom display override for Case.status. Shows an icon + label for each status. -->
  <span class="case-status-display">
    <v-icon size="small" :color="statusConfig.color" class="mr-1">{{
      statusConfig.icon
    }}</v-icon>
    <span :style="{ color: statusConfig.color }">{{ statusConfig.label }}</span>
  </span>
</template>

<script setup lang="ts">
import { Statuses } from "@/models.g";
import $metadata from "@/metadata.g";
import { computed } from "vue";

const props = defineProps<{
  modelValue?: Statuses | null;
}>();

const statusIcons: Record<Statuses, string> = {
  [Statuses.Open]: "fa fa-circle-dot",
  [Statuses.InProgress]: "fa fa-spinner",
  [Statuses.Resolved]: "fa fa-circle-check",
  [Statuses.ClosedNoSolution]: "fa fa-ban",
  [Statuses.Cancelled]: "fa fa-circle-xmark",
};

const statusColors: Record<Statuses, string> = {
  [Statuses.Open]: "#1976D2",
  [Statuses.InProgress]: "#F57C00",
  [Statuses.Resolved]: "#388E3C",
  [Statuses.ClosedNoSolution]: "#757575",
  [Statuses.Cancelled]: "#D32F2F",
};

const fallback = { icon: "fa fa-circle", color: "inherit", label: "" };

const statusConfig = computed(() => {
  const key = props.modelValue;
  if (!key) return fallback;
  return {
    icon: statusIcons[key] ?? fallback.icon,
    color: statusColors[key] ?? fallback.color,
    label: $metadata.enums.Statuses.valueLookup[key]?.displayName ?? key,
  };
});
</script>

<style scoped>
.case-status-display {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-weight: 500;
}
</style>
