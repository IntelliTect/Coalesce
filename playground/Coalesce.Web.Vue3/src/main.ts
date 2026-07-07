import "./css/site.scss";

import "@fontsource/roboto/latin.css";
import "@fontsource/roboto/latin-italic.css";
import "@fontsource/roboto-mono/latin.css";
import "@fortawesome/fontawesome-free/css/all.css";
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";

import { createApp, defineAsyncComponent } from "vue";
import { createVuetify } from "vuetify";
import { aliases, fa } from "vuetify/iconsets/fa";

import { createRouter, createWebHistory } from "vue-router";
import { AxiosClient } from "coalesce-vue";
import {
  createCoalesceVuetify,
  CAdminEditorPage,
  CAdminTablePage,
  CAdminAuditLogPage,
} from "coalesce-vue-vuetify3";

import App from "./App.vue";

// viewmodels.g has sideeffects - it populates the global lookup on ViewModel and ListViewModel.
import "@/viewmodels.g";
import $metadata from "@/metadata.g";

import CaseStatusInput from "@/admin-overrides/CaseStatusInput.vue";
import CaseStatusDisplay from "@/admin-overrides/CaseStatusDisplay.vue";

import PersonTableToolbarActions from "@/admin-extensions/PersonTableToolbarActions.vue";
import PersonEditorToolbarActions from "@/admin-extensions/PersonEditorToolbarActions.vue";
import PersonEditorActions from "@/admin-extensions/PersonEditorActions.vue";
import PersonTableRowActions from "@/admin-extensions/PersonTableRowActions.vue";
import PersonTablePageHeader from "@/admin-extensions/PersonTablePageHeader.vue";

import testWorker from "./worker.ts?worker";
import Examples from "./components/Examples.vue";
new testWorker();

new Worker(new URL("./worker.ts", import.meta.url));

AxiosClient.defaults.baseURL = "/api";
AxiosClient.defaults.withCredentials = true;

const examples = import.meta.glob("@/examples/**/*.vue");

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/test-setup",
      component: () => import("./components/test-setup.vue"),
    },
    {
      path: "/audit-logs",
      component: CAdminAuditLogPage,
      props: { type: "AuditLog" },
    },
    {
      path: "/examples",
      alias: "/",
      component: Examples,
      children: Object.entries(examples).map((x) => ({
        path: x[0].replace("/src/examples/", "").replace(".vue", ""),
        component: x[1],
      })),
    },
    {
      path: "/admin/:type",
      name: "coalesce-admin-list",
      component: CAdminTablePage,
      props: (route) => ({ ...route.params, color: "primary" }),
    },
    {
      path: "/admin/:type/item/:id?",
      name: "coalesce-admin-item",
      component: CAdminEditorPage,
      props: (route) => ({
        ...route.params,
        color: "primary",
        // autoSave: false,
      }),
    },
  ],
});

const vuetify = createVuetify({
  icons: {
    defaultSet: "fa",
    aliases,
    sets: { fa },
  },
  theme: {
    defaultTheme: localStorage.getItem("dark") == "true" ? "dark" : "light",
    themes: {
      light: {
        dark: false,
        colors: {
          primary: "#127815",
        },
      },
      dark: {
        colors: {
          primary: "#0047a3",
        },
      },
    },
  },
});
const coalesceVuetify = createCoalesceVuetify({
  metadata: $metadata,
  adminOverrides: [
    [
      $metadata.types.Case.props.status,
      { input: CaseStatusInput, display: CaseStatusDisplay },
    ],
  ],
  adminExtensions: [
    [
      $metadata.types.Person,
      {
        editorToolbarActions: PersonEditorToolbarActions,
        editorActions: PersonEditorActions,
        editorPageHeader: defineAsyncComponent(
          () => import("@/admin-extensions/PersonEditorPageHeader.vue"),
        ),
        tableToolbarActions: PersonTableToolbarActions,
        tableRowActions: PersonTableRowActions,
        tablePageHeader: PersonTablePageHeader,
      },
    ],
  ],
});

const app = createApp(App);
app.use(router);
app.use(vuetify);
app.use(coalesceVuetify);
app.mount("#app");
