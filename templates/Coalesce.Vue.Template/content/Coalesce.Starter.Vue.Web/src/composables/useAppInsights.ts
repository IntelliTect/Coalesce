import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { watch } from "vue";

import { userInfo } from "../user-service";
import router from "../router";

const appInsights = new ApplicationInsights({
  config: !APPLICATIONINSIGHTS_CONNECTION_STRING
    ? {
        // We always instantiate AppInsights even if there is no key
        // so we don't have to worry about the instance being null in app code,
        // but disable it if there is no key so we're not spamming the console with errors.
        disableTelemetry: true,
        instrumentationKey: "null",
      }
    : {
        connectionString: APPLICATIONINSIGHTS_CONNECTION_STRING,
        // We do our own page view tracking that hooks into vue-router.
        enableAutoRouteTracking: false,
      },
});

appInsights.addTelemetryInitializer(function (envelope) {
  if (
    envelope.baseType === "ExceptionData" &&
    // Filter out unactionable, junk errors:
    envelope.data?.message?.includes(
      "ResizeObserver loop completed with undelivered notifications",
    )
  ) {
    return false;
  }
  envelope.data ||= {};
  //#if (Tenancy)
  envelope.data["tenant.id"] = userInfo.value.tenantId;
  //#endif
});

appInsights.loadAppInsights();

watch(
  () => userInfo.value.userName,
  () => {
    appInsights.setAuthenticatedUserContext(userInfo.value.userName!);
  },
);

let flushPageView: (() => void) | undefined;
router.beforeEach((to, from) => {
  if (to.path != from.path) {
    // If there's a previous page view still unsent,
    // flush it now before the new page takes over and changes
    // the window.location and document.title.
    // This only happens when a user is clicking through pages very fast.
    flushPageView?.();
  }
});

router.afterEach((to, from) => {
  if (to.path != from.path) {
    const time = new Date();
    let hasSent = false;

    flushPageView = () => {
      if (hasSent) return;
      hasSent = true;
      appInsights.trackPageView({
        startTime: time,
        properties: { duration: 0 },
      });
    };

    // Wait a moment before sending the page view
    // so that the page has a chance to update `document.title`,
    // which makes the timeline easier to read in app insights.
    setTimeout(flushPageView, 2000);
  }
});

export default function useAppInsights() {
  return appInsights;
}
