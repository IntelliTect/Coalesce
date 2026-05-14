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
import type { Model } from "coalesce-vue";
import { computed } from "vue";

const props = defineProps<{
  model?: Model<any>;
  /** Direct value of the status enum, for use in dropdown item slots. */
  value?: Statuses;
}>();

const statusConfigs: Record<
  string,
  { icon: string; color: string; label: string }
> = {
  Open: { icon: "fa fa-circle-dot", color: "#1976D2", label: "Open" },
  InProgress: { icon: "fa fa-spinner", color: "#F57C00", label: "In Progress" },
  Resolved: { icon: "fa fa-circle-check", color: "#388E3C", label: "Resolved" },
  ClosedNoSolution: {
    icon: "fa fa-ban",
    color: "#757575",
    label: "Closed, No Solution",
  },
  Cancelled: {
    icon: "fa fa-circle-xmark",
    color: "#D32F2F",
    label: "Cancelled",
  },
};

const fallback = { icon: "fa fa-circle", color: "inherit", label: "" };

const statusConfig = computed(() => {
  const key = props.value ?? (props.model as any)?.status;
  if (typeof key === "string") return statusConfigs[key] ?? fallback;
  return fallback;
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
