<template>
  <v-container max-width="800px">
    <v-card>
      <v-card-title>User Profile</v-card-title>
      <c-loader-status
        :loaders="{
          'no-initial-content no-error-content': [user.$load],
          //#if Tenancy
          '': [user.$bulkSave, user.evict],
          //#else
          //'': [user.$bulkSave],
          //#endif
        }"
      >
        <v-card-text>
          <!--#if Tenancy -->
          <!-- Since users transcend tenants, a user may only edit their own user attributes
           since otherwise an admin in one tenant could affect how users show up in other tenants. -->
          <c-input :model="user" for="userName" :readonly="!isMe"></c-input>
          <c-input :model="user" for="fullName" :readonly="!isMe"></c-input>
          <!--#else 
          <c-input :model="user" for="userName"></c-input>
          <c-input :model="user" for="fullName"></c-input>
          #endif -->

          <div class="d-flex">
            <c-input
              :model="user"
              for="email"
              readonly
              :hint="
                user.emailConfirmed ? 'Email Verified' : 'Email not verified'
              "
              persistent-hint
              class="mb-3"
            >
              <template #append-inner>
                <v-icon v-if="user.emailConfirmed" color="success">
                  fa fa-check-circle
                </v-icon>
                <v-icon v-else color="warning">
                  fa fa-exclamation-triangle
                </v-icon>
              </template>
            </c-input>

            <!--#if LocalAuth -->
            <div v-if="!user.emailConfirmed">
              <v-btn
                color="success"
                :loading="user.sendEmailConfirmation.isLoading"
                prepend-icon="fa fa-paper-plane"
                class="ml-3"
                @click="user.sendEmailConfirmation()"
              >
                Send Email Confirmation
              </v-btn>
              <c-loader-status
                :loaders="user.sendEmailConfirmation"
                no-progress
              />
            </div>

            <v-dialog v-else width="400px">
              <template #activator="{ props }">
                <v-btn
                  color="primary"
                  variant="text"
                  class="ml-3"
                  v-bind="props"
                >
                  Change Email...
                </v-btn>
              </template>
              <template #default="{ isActive }">
                <v-card title="Change Email">
                  <v-card-text>
                    <c-input
                      :model="user.setEmail"
                      for="newEmail"
                      hide-details="auto"
                    ></c-input>
                    <c-loader-status :loaders="user.setEmail" />
                    <v-alert
                      v-if="user.setEmail.wasSuccessful"
                      :text="user.setEmail.message ?? ''"
                      type="success"
                    ></v-alert>
                  </v-card-text>
                  <v-card-actions>
                    <v-btn
                      v-if="!user.setEmail.wasSuccessful"
                      variant="text"
                      @click="isActive.value = false"
                    >
                      Cancel
                    </v-btn>
                    <v-spacer></v-spacer>
                    <v-btn
                      v-if="user.setEmail.wasSuccessful"
                      @click="
                        isActive.value = false;
                        user.$load();
                      "
                    >
                      Done
                    </v-btn>
                    <v-btn
                      v-else
                      :loading="user.setEmail.isLoading"
                      color="primary"
                      variant="elevated"
                      @click="user.setEmail.invokeWithArgs()"
                    >
                      Update Email
                    </v-btn>
                  </v-card-actions>
                </v-card>
              </template>
            </v-dialog>
            <!--#endif -->
          </div>
        </v-card-text>

        <!--#if LocalAuth -->
        <template v-if="isMe">
          <v-card-title>
            Password
            <v-dialog width="400px">
              <template #activator="{ props }">
                <v-btn
                  color="primary"
                  variant="tonal"
                  class="ml-3"
                  v-bind="props"
                >
                  Change Password...
                </v-btn>
              </template>
              <template #default="{ isActive }">
                <v-card title="Change Password">
                  <v-card-text>
                    <c-input
                      :model="user.setPassword"
                      for="currentPassword"
                    ></c-input>
                    <c-input
                      :model="user.setPassword"
                      for="newPassword"
                    ></c-input>
                    <c-input
                      :model="user.setPassword"
                      for="confirmNewPassword"
                    ></c-input>
                    <c-loader-status :loaders="user.setPassword" />
                  </v-card-text>
                  <v-card-actions>
                    <v-btn variant="text" @click="isActive.value = false">
                      Cancel
                    </v-btn>
                    <v-spacer></v-spacer>
                    <v-btn
                      :loading="user.setPassword.isLoading"
                      color="primary"
                      variant="elevated"
                      @click="
                        user.setPassword
                          .invokeWithArgs()
                          .then(() => (isActive.value = false))
                      "
                    >
                      Update Password
                    </v-btn>
                  </v-card-actions>
                </v-card>
              </template>
            </v-dialog>
          </v-card-title>
        </template>
        <!--#endif -->

        <!--#if Passkeys -->
        <template v-if="isMe">
          <v-card-title>
            Passkeys
            <v-btn
              color="primary"
              variant="tonal"
              class="ml-3"
              prepend-icon="fa fa-plus"
              @click="addPasskey"
            >
              Add a new passkey
            </v-btn>
          </v-card-title>
          <v-card-text v-if="!supportsPasskeys">
            Passkeys are not supported by this browser.
          </v-card-text>
          <v-card-text v-else>
            <c-loader-status
              :loaders="{
                'no-initial-content': [passkeyService.getPasskeys],
                '': [
                  passkeyService.getPasskeys,
                  passkeyService.addPasskey,
                  passkeyService.deletePasskey,
                  passkeyService.renamePasskey,
                ],
              }"
              progress-absolute
            >
              <v-list v-if="passkeys.length > 0" bg-color="transparent">
                <v-list-item
                  v-for="passkey in passkeys"
                  :key="passkey.credentialId!"
                  :title="passkey.name!"
                  :subtitle="`Created ${formatDistanceToNow(new Date(passkey.createdOn!), { addSuffix: true })}`"
                  prepend-icon="fa fa-key"
                >
                  <template #append>
                    <v-btn
                      size="small"
                      variant="tonal"
                      color="primary"
                      class="mr-1"
                      @click="renamePasskey(passkey.credentialId!)"
                    >
                      Rename
                    </v-btn>
                    <v-btn
                      size="small"
                      variant="tonal"
                      color="error"
                      @click="deletePasskey(passkey.credentialId!)"
                    >
                      Delete
                    </v-btn>
                  </template>
                </v-list-item>
              </v-list>
              <p v-else>No passkeys are registered.</p>
            </c-loader-status>
          </v-card-text>
        </template>
        <!--#endif -->

        <template v-if="isUserAdmin">
          <v-card-title> Roles & permissions </v-card-title>
          <v-card-text>
            <v-row>
              <v-col>
                <c-input :model="user" for="userRoles"></c-input>
              </v-col>
              <v-col>
                <h3>Effective Permissions:</h3>
                <div style="max-height: 300px; overflow-y: auto">
                  <div
                    v-for="permission in PermissionMeta.values.map((p) => ({
                      meta: p,
                      roles: user.roles.filter((r) =>
                        r.permissions?.includes(p.value),
                      ),
                    }))"
                    :key="permission.meta.strValue"
                  >
                    <span v-if="permission.roles.length">
                      <v-icon icon="fa fa-check text-success"></v-icon>
                      {{ permission.meta.displayName }}
                      <span class="text-caption text-grey pl-1">
                        (via
                        {{ permission.roles.map((r) => r.name).join(",") }})
                      </span>
                    </span>
                    <span v-else>
                      <v-icon icon="fa fa-times text-error"></v-icon>
                      {{ permission.meta.displayName }}
                    </span>
                  </div>
                </div>
              </v-col>
            </v-row>

            <!--#if Tenancy -->
            <template v-if="isGlobalAdmin">
              <c-input
                :model="user"
                for="isGlobalAdmin"
                :readonly="id == $userInfo.id"
              ></c-input>
            </template>
            <!--#endif -->
          </v-card-text>
        </template>

        <v-card-actions>
          <!--#if Tenancy  -->
          <!-- Admins can kick users (but not themselves) out of the tenant. -->
          <v-btn
            v-if="isUserAdmin && id !== $userInfo.id"
            color="error"
            large
            @click="removeFromTenant"
          >
            Remove User from Tenant
          </v-btn>
          <!--#endif -->
          <v-spacer></v-spacer>
          <v-btn
            color="success"
            prepend-icon="fa fa-save"
            :loading="user.$bulkSave.isLoading"
            :disabled="!user.$bulkSavePreview().isDirty"
            :variant="!user.$bulkSavePreview().isDirty ? 'text' : 'elevated'"
            @click="user.$bulkSave()"
          >
            Save
          </v-btn>
        </v-card-actions>
      </c-loader-status>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { UserViewModel } from "@/viewmodels.g";
import { Permission } from "@/models.g";
import { Permission as PermissionMeta } from "@/metadata.g";
//#if Passkeys
import { formatDistanceToNow } from "date-fns";
import { PasskeyServiceViewModel } from "@/viewmodels.g";
//#endif

const router = useRouter();
const { can, userInfo } = useUser();

const props = defineProps<{ id: string }>();
const user = new UserViewModel();

const isUserAdmin = computed(() => can(Permission.UserAdmin));

//#if Tenancy
const isGlobalAdmin = computed(() =>
  userInfo.value.roles?.includes("GlobalAdmin"),
);

async function removeFromTenant() {
  await user.evict.confirmInvoke(
    `Really remove the user from the ${userInfo.value.tenantName} organization?`,
  );
  user.$load.wasSuccessful = null;
  router.back();
}
//#endif

const isMe = computed(() => props.id == userInfo.value.id);

if (!isUserAdmin.value && !isMe.value) {
  // Non-admins can only view themselves
  router.replace({ name: "error-404" });
} else {
  user.$load(props.id);
}

//#if Passkeys
// Passkey Management
const supportsPasskeys =
  typeof navigator.credentials !== "undefined" &&
  typeof window.PublicKeyCredential !== "undefined" &&
  typeof window.PublicKeyCredential.parseCreationOptionsFromJSON === "function";

const passkeyService = new PasskeyServiceViewModel();
passkeyService.getPasskeys();

const passkeys = computed(() => passkeyService.getPasskeys.result ?? []);

let addAttempt = 0;
async function addPasskey() {
  const attempt = ++addAttempt;
  try {
    // Get creation options from server
    const creationOption = await passkeyService.getCreationOptions();
    const options = PublicKeyCredential.parseCreationOptionsFromJSON(
      JSON.parse(creationOption),
    );

    // Create credential
    const credential = (await navigator.credentials.create({
      publicKey: options,
    })) as PublicKeyCredential;

    const credentialJson = JSON.stringify(credential);

    const name = prompt("Enter a name for your passkey:")?.trim();
    if (!name) return;

    if (name.length >= 200) {
      alert("Names must be less than 200 characters.");
      return;
    }

    // Add to server with name
    await passkeyService.addPasskey(credentialJson, name);
    await passkeyService.getPasskeys();
  } catch (error) {
    if (attempt == addAttempt && error instanceof Error) {
      const errorMessage =
        error.name === "NotAllowedError"
          ? "No passkey was provided by the authenticator."
          : error.message || "Failed to create passkey";

      passkeyService.addPasskey.wasSuccessful = false;
      passkeyService.addPasskey.message = errorMessage;
    }
  }
}

async function renamePasskey(credentialId: string) {
  const passkey = passkeys.value.find((p) => p.credentialId == credentialId);

  const newName = prompt(
    "Enter a new name for your passkey:",
    passkey?.name || "",
  )?.trim();
  if (!newName) return;

  if (newName.length >= 200) {
    alert("Names must be less than 200 characters.");
    return;
  }

  await passkeyService.renamePasskey(credentialId, newName.trim());
  await passkeyService.getPasskeys();
}

async function deletePasskey(credentialId: string) {
  const passkey = passkeys.value.find((p) => p.credentialId == credentialId);

  if (
    !confirm(
      `Really delete the "${passkey?.name || "Unnamed passkey"}" passkey?`,
    )
  ) {
    return;
  }

  await passkeyService.deletePasskey(credentialId);
  await passkeyService.getPasskeys();
}
//#endif
</script>
