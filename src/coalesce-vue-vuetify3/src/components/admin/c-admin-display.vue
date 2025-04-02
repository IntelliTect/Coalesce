<script lang="ts" setup generic="TModel extends Model | undefined">
import { h, resolveComponent } from "vue";
import { useMetadataProps } from "../c-metadata-component";
import {
  propDisplay,
  type ViewModelCollection,
  type Model,
} from "coalesce-vue";

import CDisplay, { type CDisplayProps } from "../display/c-display.vue";
import { useRouter } from "vue-router";
import { detectFileFormat } from "../../util";
import { getRefNavRoute } from "./util";

const props = withDefaults(defineProps<CDisplayProps<TModel>>(), {
  element: "span",
});

defineOptions({
  name: "c-admin-display",
});

const router = useRouter();

const { modelMeta: modelMetaRef, valueMeta } = useMetadataProps(props);

function render() {
  const { model } = props;

  if (model == null) {
    // If no model was provided, just display nothing.
    // This isn't an error case - it just means the thing we're trying to display
    // is `null`-ish, and should be treated the same way that vue would treat {{null}}
    return h("span");
  }

  const modelMeta = modelMetaRef.value;
  let meta = valueMeta.value;
  if (!meta && modelMeta && "displayProp" in modelMeta) {
    meta = modelMeta.displayProp || null;
  }

  if (!meta) {
    throw Error(
      "Provided model has no $metadata property, and no specific value was provided via the 'for' component prop to c-display."
    );
  }

  if ("params" in meta) {
    throw Error("Cannot display a method");
  }

  if (modelMeta && "type" in modelMeta && modelMeta.type == "model") {
    const pkValue = (model as any)[modelMeta.keyProp.name];

    // Display collection navigations as counts with links to the c-admin-table-page for the collection
    if (
      pkValue &&
      meta.role == "collectionNavigation" &&
      "foreignKey" in meta
    ) {
      const narrowedMeta = meta;
      return h(
        resolveComponent("router-link"),
        {
          // Resolve to an href to allow overriding of admin routes in userspace.
          // If we just gave a named raw location, it would always use the coalesce admin route
          // instead of the user-overridden one (that the user overrides by declaring another
          // route with the same path).
          to: router.resolve({
            name: "coalesce-admin-list",
            params: { type: meta.itemType.typeDef.name },
            query: { ["filter." + meta.foreignKey.name]: pkValue },
          }).fullPath,
        },
        // Use `propDisplay` for our formatted count, forcing the count always by preventing enumeration.
        () =>
          propDisplay(model, narrowedMeta, {
            collection: { enumeratedItemsMax: 0 },
          }) ||
          (((model as any)[narrowedMeta.name] as ViewModelCollection<any, any>)
            ?.$hasLoaded === false
            ? "View"
            : "None")
      );
    }

    // Display reference navigations with links to the editor page for the item.
    if (pkValue && meta.role == "referenceNavigation" && "foreignKey" in meta) {
      const narrowedMeta = meta;
      const fkValue = (model as any)[meta.foreignKey.name];
      if (fkValue) {
        return h(
          resolveComponent("router-link"),
          {
            to: getRefNavRoute(router, model, meta),
          },
          () => propDisplay(model, narrowedMeta) ?? fkValue
        );
      }
    }
  }

  if (meta.type == "binary") {
    const value = (model as any)[meta.name];

    if (value) {
      return h(
        "a",
        {
          href: "#",
          onClick: () => {
            const link = document.createElement("a");
            link.href = `data:application/octet-stream;base64,${value}`;
            link.download =
              meta.name + "." + (detectFileFormat(value) || "bin");
            link.click();
          },
        },
        h(CDisplay, { ...props })
      );
    }
  }

  return h(CDisplay, { ...props });
}
</script>

<template>
  <render />
</template>
