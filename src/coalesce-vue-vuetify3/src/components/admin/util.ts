import {
  ListParameters,
  mapValueToModel,
  Model,
  ModelReferenceNavigationProperty,
  ViewModel,
} from "coalesce-vue";
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

export function copyParamsToNewViewModel(
  vm: ViewModel,
  params: ListParameters
) {
  vm.$params.dataSource = params.dataSource;
  if (params.filter) {
    for (const propName in vm.$metadata.props) {
      const prop = vm.$metadata.props[propName];
      const filterValue = params.filter[propName];
      if (filterValue != null) {
        try {
          (vm as any)[propName] = mapValueToModel(filterValue, prop);
        } catch (e) {
          // mapValueToModel will throw for unmappable values.
          console.error(`Could not map filter parameter ${propName}. ${e}`);
        }
      }
    }
  }
}
