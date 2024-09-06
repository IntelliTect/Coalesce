import { SecurityServiceViewModel } from "@/viewmodels.g";
import {
  //#if Identity
  Permission,
  //#endif
  UserInfo,
} from "@/models.g";

const securityService = new SecurityServiceViewModel();
securityService.whoAmI.setConcurrency("debounce");
//#if AppInsights
securityService.whoAmI.onFulfilled(() => {
  //@ts-expect-error AppInsights imported from backend JavaScriptSnippet; no types available.
  window.appInsights?.setAuthenticatedUserContext(userInfo.value.userName);
});
//#endif

/** Properties about the currently authenticated user */
export const userInfo = computed(() => {
  return securityService.whoAmI.result ?? new UserInfo();
});

//#if Identity
/** Returns true if the user has any of the specified permissions */
export function can(...permission: Permission[]) {
  return (
    userInfo.value?.permissions?.some((r) =>
      permission.map((p) => Permission[p]).includes(r),
    ) || false
  );
}
//#endif

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

refreshUserInfo();

// Make useful properties available in vue <template>s
declare module "vue" {
  interface ComponentCustomProperties {
    //#if Identity
    Permission: typeof Permission;
    $can: typeof can;
    //#endif
    $userInfo: (typeof userInfo)["value"];
  }
}
export const globalProperties = {
  //#if Identity
  Permission,
  $can: can,
  //#endif
  get $userInfo() {
    return userInfo.value;
  },
};
