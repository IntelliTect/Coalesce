import {
  ListParameters,
  mapValueToModel,
  mapToDto,
  Model,
  ModelReferenceNavigationProperty,
  ViewModel,
  ModelValueProperty,
  ModelType,
} from "coalesce-vue";
import { Router } from "vue-router";

export function getRefNavRoute(
  router: Router,
  owner: Model,
  prop: ModelReferenceNavigationProperty | ModelValueProperty,
) {
  const item = (owner as any)[prop.name];
  const fk =
    ("foreignKey" in prop
      ? (owner as any)[prop.foreignKey?.name]
      : undefined) ?? item?.[prop.typeDef.keyProp.name];

  const meta: ModelType = item?.$metadata ?? prop.typeDef;

  try {
    if (!item && "foreignKey" in prop && prop.foreignKey.role == "primaryKey") {
      // This is a shared-key one-to-one, and the model isn't loaded.
      // That most likely means that the model doesn't exist
      // (or it failed to be .Included() in the response).
      // We want to route to a create editor, not an edit editor.
      return router.resolve({
        name: "coalesce-admin-item",
        params: {
          type: meta.name,
        },
        query: { ["filter." + meta.keyProp.name]: fk },
      }).fullPath;
    }

    if (!fk) return;

    // Resolve to an href to allow overriding of admin routes in userspace.
    // If we just gave a named raw location, it would always use the coalesce admin route
    // instead of the user-overridden one (that the user overrides by declaring another
    // route with the same path).
    return router.resolve({
      name: "coalesce-admin-item",
      params: {
        type: meta.name,
        id: String(mapToDto(fk, meta.keyProp)),
      },
    }).fullPath;
  } catch {
    return undefined;
  }
}

export function copyParamsToNewViewModel(
  vm: ViewModel,
  params: ListParameters,
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
