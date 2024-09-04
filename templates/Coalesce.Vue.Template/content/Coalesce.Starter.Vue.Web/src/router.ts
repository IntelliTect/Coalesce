import { createRouter, createWebHistory } from "vue-router";
import {
  CAdminEditorPage,
  CAdminTablePage,
  //#if AuditLogs
  CAdminAuditLogPage,
  //#endif
} from "coalesce-vue-vuetify3";
//#if Identity
import { Permission } from "./models.g";
//#endif

export default createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "home",
      component: () => import("./views/Home.vue"),
    },
    //#if ExampleModel
    {
      path: "/widget/:id(\\d+)?",
      name: "widget-edit",
      component: () => import("./views/WidgetEdit.vue"),
      props: (r) => ({ id: +r.params.id }),
    },
    //#endif
    {
      path: "/admin",
      name: "admin",
      component: () => import("./views/Admin.vue"),
    },

    // Coalesce admin routes
    {
      path: "/admin/:type",
      name: "coalesce-admin-list",
      component: titledAdminPage(CAdminTablePage),
      props: true,
    },
    {
      path: "/admin/:type/edit/:id?",
      name: "coalesce-admin-item",
      component: titledAdminPage(CAdminEditorPage),
      props: true,
    },
    //#if AuditLogs
    {
      path: "/admin/audit",
      component: titledAdminPage(CAdminAuditLogPage),
      meta: { permissions: [Permission.ViewAuditLogs] },
      props: { type: "AuditLog" },
    },
    //#endif
    {
      name: "error-404",
      path: "/:pathMatch(.*)*",
      component: () => import("@/views/errors/NotFound.vue"),
    },
  ],
});

/** Creates a wrapper component that will pull page title from the
 *  coalesce admin page component and pass it to `useTitle`.
 */
function titledAdminPage<
  T extends
    | typeof CAdminTablePage
    | typeof CAdminEditorPage
    //#if AuditLogs
    | typeof CAdminAuditLogPage,
  //#endif
>(component: T) {
  return defineComponent({
    setup() {
      const pageRef = ref<InstanceType<T>>();
      useTitle(() => pageRef.value?.pageTitle);
      return { pageRef };
    },
    render() {
      return h(component as any, {
        color: "primary",
        ref: "pageRef",
        ...this.$attrs,
      });
    },
  });
}
