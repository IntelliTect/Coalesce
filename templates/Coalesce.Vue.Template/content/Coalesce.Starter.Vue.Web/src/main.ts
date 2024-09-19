import { createApp } from "vue";
import { createVuetify } from "vuetify";
import { createCoalesceVuetify } from "coalesce-vue-vuetify3";
import { aliases, fa } from "vuetify/iconsets/fa";
import { AxiosClient as CoalesceAxiosClient } from "coalesce-vue";
import { isAxiosError } from "axios";

import App from "./App.vue";
import router from "./router";
import {
  refreshUserInfo,
  globalProperties as userServiceProps,
} from "./user-service";

// Import global CSS and Fonts:
import "typeface-roboto";
import "@fortawesome/fontawesome-free/css/all.css";
import "coalesce-vue-vuetify3/styles.css";
import "@/site.scss";
import "vuetify/styles";

import $metadata from "@/metadata.g";
// viewmodels.g has side effects - it populates the global lookup on ViewModel and ListViewModel.
// This global lookup allows the admin page components to function.
import "@/viewmodels.g";

// SETUP: vuetify
const inputDefaults = { density: "compact", variant: "outlined" };
const vuetify = createVuetify({
  icons: {
    defaultSet: "fa",
    aliases,
    sets: { fa },
  },
  defaults: {
    VTextField: inputDefaults,
    VTextarea: inputDefaults,
    VSelect: inputDefaults,
    VCombobox: inputDefaults,
    VAutocomplete: inputDefaults,
    VField: inputDefaults,
    VInput: inputDefaults,
    VSwitch: { color: "primary" },
  },
  theme: {
    themes: {
      light: {
        colors: {
          // TODO: Customize theme colors
        },
      },
      //#if DarkMode
      dark: {
        colors: {
          // TODO: Customize theme colors
        },
      },
      //#endif
    },
  },
});

// SETUP: coalesce-vue
CoalesceAxiosClient.defaults.baseURL = "/api";
CoalesceAxiosClient.defaults.withCredentials = true;
CoalesceAxiosClient.interceptors.response.use(undefined, (error) => {
  if (isAxiosError(error) && error.response?.status == 401) {
    console.warn("Received 401 from API endpoint. Refreshing for sign-in.");
    window.location.reload();
    return new Promise<void>(() => {
      /* Never resolving promise so failure doesn't momentarily propagate to UI before reload completes */
    });
  }
  return Promise.reject(error);
});

// SETUP: coalesce-vue-vuetify
const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
});

//#if AppInsights
// SETUP: app insights
//@ts-expect-error AppInsights imported from backend JavaScriptSnippet; no types available.
window.appInsights?.addTelemetryInitializer(function (envelope) {
  debugger;
  if (
    envelope.baseType === "ExceptionData" &&
    // Filter out unactionable, junk errors:
    envelope.data?.message?.includes(
      "ResizeObserver loop completed with undelivered notifications",
    )
  ) {
    return false;
  }
});
//#endif

const app = createApp(App);
Object.defineProperties(
  app.config.globalProperties,
  Object.getOwnPropertyDescriptors(userServiceProps),
);

app.use(router);
app.use(vuetify);
app.use(coalesceVuetify);

refreshUserInfo()
  .then(() => {
    app.mount("#app");
  })
  .catch((e) => {
    alert(
      "Unable to contact the application server. Please refresh the page to try again.",
    );
  });
