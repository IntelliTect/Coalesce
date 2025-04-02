import { Model, ModelReferenceNavigationProperty } from "coalesce-vue";
import { Router } from "vue-router";

export function getRefNavRoute(
  router: Router,
  owner: Model,
  prop: ModelReferenceNavigationProperty
) {
  const item = (owner as any)[prop.name];
  const fk = (owner as any)[prop.foreignKey.name] ?? item?.$primaryKey;

  if (!fk) return;

  // Resolve to an href to allow overriding of admin routes in userspace.
  // If we just gave a named raw location, it would always use the coalesce admin route
  // instead of the user-overridden one (that the user overrides by declaring another
  // route with the same path).
  return router.resolve({
    name: "coalesce-admin-item",
    params: {
      type: item?.$metadata.name ?? prop.typeDef.name,
      id: fk,
    },
  }).fullPath;
}
