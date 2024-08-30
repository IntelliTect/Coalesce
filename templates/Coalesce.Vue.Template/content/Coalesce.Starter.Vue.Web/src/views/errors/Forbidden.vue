<template>
  <v-container fluid>
    <v-row style="margin-top: 10vh">
      <v-col>
        <h1 class="text-center text-h1 font-weight-thin">Forbidden</h1>
        <h2 class="text-center text-h4 font-weight-thin mt-5">
          User '{{ $userInfo.userName }}' is not permitted to access the
          requested page.
        </h2>

        <v-row
          justify="space-between"
          style="max-width: 600px"
          class="mx-auto mt-10"
        >
          <v-btn
            size="x-large"
            @click="$router.back()"
            variant="outlined"
            color="primary"
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
            <v-icon start>fa fa-sign-out</v-icon> Switch Account
          </v-btn>
        </v-row>

        <!-- This is deliberately off the bottom of the screen for mild obscurity -->
        <h3
          class="text-center text-body-2 my-3 font-weight-light"
          style="position: absolute; top: 100vh; left: 0; right: 0"
        >
          User needs at least one of these required permissions:
          <br />
          {{
            permissions
              ?.map((p) => PermissionMeta.valueLookup[p].displayName)
              .join(", ")
          }}
        </h3>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts" setup>
import { Permission } from "@/models.g";
import { Permission as PermissionMeta } from "@/metadata.g";

const props = defineProps<{ permissions?: Permission[] }>();

const logOutUrl = computed(
  () =>
    "/Account/LogOff?ReturnUrl=" +
    encodeURIComponent(
      window.location.href.replace(window.location.origin, ""),
    ),
);
</script>
