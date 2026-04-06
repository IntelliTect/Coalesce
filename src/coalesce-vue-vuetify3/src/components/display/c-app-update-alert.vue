<template>
  <v-snackbar
    :model-value="isUpdateAvailable"
    :timeout="-1"
    color="info"
    location="bottom left"
    class="c-app-update-alert"
  >
    <slot>A new version of this application is available.</slot>
    <template #actions>
      <slot name="actions" :reload="reload">
        <v-btn variant="text" @click="reload">Refresh</v-btn>
      </slot>
    </template>
  </v-snackbar>
</template>

<script setup lang="ts">
import { useAppUpdateCheck } from "coalesce-vue";
import type { AxiosInstance } from "axios";

const props = defineProps<{
  /** The Axios instance to monitor for version changes. Defaults to the Coalesce AxiosClient. */
  axiosInstance?: AxiosInstance;
}>();

const { isUpdateAvailable } = useAppUpdateCheck(props.axiosInstance);

function reload() {
  window.location.reload();
}
</script>
