<template>
  <v-container style="max-width: 1600px">
    <v-row>
      <v-col cols="12" lg="6">
        <v-card>
          <v-card-text>
            <v-list compact>
              <!--#if Tenancy  -->
              <v-list-item
                v-if="$can(Permission.Admin)"
                title="My Organization Settings"
                subtitle="Edit the details of your organization."
                :to="`/admin/Tenant/edit/${$userInfo.tenantId}`"
                prepend-icon="fa fa-users"
              />
              <v-list-item
                v-if="$userInfo.roles?.includes('GlobalAdmin')"
                to="/admin/Tenant?dataSource=GlobalAdminSource"
                prepend-icon="fa fa-building"
                title="All Organizations"
                subtitle="Perform administration of all organizations."
              />
              <!--#endif  -->
              <!--#if (AuditLogs && Identity) -->
              <v-list-item
                v-if="$can(Permission.ViewAuditLogs)"
                title="Audit Logs"
                subtitle="Logs of each data change made in the application."
                to="/admin/audit"
                prepend-icon="fa fa-clipboard-list"
              />
              <!--#endif  -->
              <!--#if (AuditLogs && !Identity)
              <v-list-item
                title="Audit Logs"
                subtitle="Logs of each data change made in the application."
                to="/admin/audit"
                prepend-icon="fa fa-clipboard-list"
              >
              </v-list-item>
              #endif  -->
              <v-divider class="my-2"></v-divider>
              <v-list-item
                title="Coalesce Security Overview"
                subtitle="An overview of how each property, method, and endpoint is served by Coalesce."
                href="/coalesce-security"
                prepend-icon="fa fa-lock-open"
              />
              <!--#if OpenAPI  -->
              <v-list-item
                title="Open API"
                subtitle="View OpenAPI documentation for the application."
                to="/openapi"
                prepend-icon="fa fa-code"
              />
              <!--#endif  -->
              <!--#if Hangfire  -->
              <!--#if Tenancy  -->
              <v-list-item
                v-if="$userInfo.roles?.includes('GlobalAdmin')"
                href="/hangfire"
                prepend-icon="fa fa-briefcase"
                title="Hangfire"
                subtitle="View the Hangfire background job dashboard."
              />
              <!--#endif  -->
              <!--#if (!Tenancy)
              <v-list-item
                v-if="$can(Permission.Admin)"
                href="/hangfire"
                prepend-icon="fa fa-briefcase"
                title="Hangfire"
                subtitle="View the Hangfire background job dashboard."
              />
              #endif  -->
              <!--#endif  -->
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" lg="6">
        <v-card>
          <v-card-text>
            <v-list density="compact">
              <v-list-item
                v-for="type in adminTypes"
                :key="type.name"
                :title="type.displayName"
                :subtitle="type.description"
                :to="{
                  name: 'coalesce-admin-list',
                  params: { type: type.name },
                }"
              >
                <template #prepend>
                  <v-avatar
                    color="surface-variant"
                    class="font-weight-bold ml-n1"
                  >
                    {{
                      type.displayName
                        .split(" ")
                        .map((w) => w[0])
                        .join("")
                        .slice(0, 4)
                    }}
                  </v-avatar>
                </template>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <br />
  </v-container>
</template>

<script setup lang="ts">
import $metadata from "@/metadata.g";
import type { Domain, ModelType } from "coalesce-vue";

const excludedTypes: Array<keyof typeof $metadata.types> = [
  //#if AuditLogs
  "AuditLog",
  "AuditLogProperty",
  //#endif
  //#if Identity
  "UserRole",
  //#endif
  //#if Tenancy
  "Tenant",
  //#endif
];

const adminTypes = Object.values(($metadata as Domain).types).filter(
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore may be errors if the project has only model or only object types
  (t): t is ModelType => t.type == "model" && !excludedTypes.includes(t.name),
);
</script>
