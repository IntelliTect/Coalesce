import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { generateW3CId } from "@microsoft/applicationinsights-core-js";
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
let isFirstNavigation = true;

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
    if (!isFirstNavigation) {
      // Generate a new trace ID for each navigation (except the initial page load)
      // Otherwise, each successive page view will get lumped in with the root page view.
      // This will also split up telemetry by page view in Aspire (but the wrapping page view
      // span itself will be missing from Aspire since we don't do OTLP ingest here).
      appInsights.context.telemetryTrace.traceID = generateW3CId();
    }
    isFirstNavigation = false;

    const time = new Date();
    let hasSent = false;

    flushPageView = () => {
      if (hasSent) return;
      hasSent = true;
      appInsights.trackPageView({
        startTime: time,
        properties: {
          // Since this is a SPA, there's no meaningful loading time of the page itself.
          duration: 0,
        },
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
