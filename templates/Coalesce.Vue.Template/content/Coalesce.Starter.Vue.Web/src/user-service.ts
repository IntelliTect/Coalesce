import { SecurityServiceViewModel } from "@/viewmodels.g";
import { Permission, UserInfo } from "@/models.g";

const securityService = new SecurityServiceViewModel();
securityService.whoAmI.setConcurrency("debounce");
securityService.whoAmI.onRejected((e) => {
  const caller = securityService.whoAmI;
  if (caller.message && /you are not signed in/i.test(caller.message)) {
    // This message we test for originates from the redirect handler in Program.cs.
    // User is logged out. Full-page refresh to the public app's sign in page.
    window.location.reload();
  } else {
    throw e;
  }
});

/** Properties about the currently authenticated user */
export const userInfo = computed(() => {
  return securityService.whoAmI.result ?? new UserInfo();
});

/** Returns true if the user has any of the specified permissions */
export function can(...permission: Permission[]) {
  return (
    userInfo.value?.permissions?.some((r) =>
      permission.map((p) => Permission[p]).includes(r),
    ) || false
  );
}

export const refreshUserInfo = () => securityService.whoAmI();

const interval = 1000 * 60 * 5; // Refresh every X minutes.
setInterval(() => {
  // Don't refresh user info if the window is minimized or the tab is in the background.
  if (!document.hidden) {
    refreshUserInfo().catch();
  }
}, interval);

// Reload user info on window focus.
document.addEventListener(
  "visibilitychange",
  () => {
    if (!document.hidden) refreshUserInfo();
  },
  false,
);

// Make useful properties available in vue <template>s
declare module "@vue/runtime-core" {
  interface ComponentCustomProperties {
    Permission: typeof Permission;
    $can: typeof can;
    $userInfo: (typeof userInfo)["value"];
  }
}
export const globalProperties = {
  Permission,
  $can: can,
  $userInfo: userInfo,
};
