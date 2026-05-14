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
  modelValue?: Statuses;
}>();

const statusIcons: Record<string, string> = {
  Open: "fa fa-circle-dot",
  InProgress: "fa fa-spinner",
  Resolved: "fa fa-circle-check",
  ClosedNoSolution: "fa fa-ban",
  Cancelled: "fa fa-circle-xmark",
};

const statusColors: Record<string, string> = {
  Open: "#1976D2",
  InProgress: "#F57C00",
  Resolved: "#388E3C",
  ClosedNoSolution: "#757575",
  Cancelled: "#D32F2F",
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
