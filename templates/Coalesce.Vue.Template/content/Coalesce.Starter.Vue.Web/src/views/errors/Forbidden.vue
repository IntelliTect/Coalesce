<template>
  <v-empty-state headline="Forbidden" class="justify-start mt-16">
    <template #title>
      {{ $userInfo.userName }} is not permitted to access this page.
    </template>

    <template #actions>
      <v-btn
        size="x-large"
        variant="outlined"
        color="primary"
        @click="$router.back()"
      >
        <v-icon start>fa fa-arrow-left</v-icon> Back
      </v-btn>
      <v-btn size="x-large" to="/" variant="outlined" color="primary">
        <v-icon start>fa fa-home</v-icon> Home
      </v-btn>
      <v-btn
        size="x-large"
        :href="logOutUrl"
        variant="outlined"
        color="primary"
      >
        <v-icon start>fa fa-sign-out</v-icon> Sign Out
      </v-btn>
    </template>

    <template #text>
      <h3
        v-if="permissions?.length"
        class="text-center text-body-2 my-3 font-weight-light"
      >
        Required permissions:
        {{
          permissions
            ?.map((p) => PermissionMeta.valueLookup[p].displayName)
            .join(", ")
        }}
      </h3>
    </template>
  </v-empty-state>
</template>

<script lang="ts" setup>
import { Permission } from "@/models.g";
import { Permission as PermissionMeta } from "@/metadata.g";

const props = defineProps<{ permissions?: Permission[] }>();

const logOutUrl = computed(
  () =>
    "/SignOut?ReturnUrl=" +
    encodeURIComponent(
      window.location.href.replace(window.location.origin, ""),
    ),
);
</script>
