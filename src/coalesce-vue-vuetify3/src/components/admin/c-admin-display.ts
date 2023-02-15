import { defineComponent, h, resolveComponent } from "vue";
import { getValueMeta } from "../c-metadata-component";
import { propDisplay, ViewModelCollection } from "coalesce-vue";

import CDisplay, { cDisplayProps } from "../display/c-display";
import { useRouter } from "vue-router";

export default defineComponent({
  name: "c-admin-display",

  props: cDisplayProps,

  setup() {
    return { router: useRouter() };
  },

  render() {
    const props = this.$props;
    const { model } = props;

    if (model == null) {
      // If no model was provided, just display nothing.
      // This isn't an error case - it just means the thing we're trying to display
      // is `null`-ish, and should be treated the same way that vue would treat {{null}}
      return h("span");
    }

    const modelMeta = model ? model.$metadata : null;
    let meta = getValueMeta(props.for, modelMeta, this.$coalesce.metadata);
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

    if (modelMeta?.type == "model") {
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
            to: this.router.resolve({
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
            (((model as any)[narrowedMeta.name] as ViewModelCollection<any>)
              ?.$hasLoaded === false
              ? "View"
              : "None")
        );
      }

      // Display reference navigations with links to the editor page for the item.
      if (
        pkValue &&
        meta.role == "referenceNavigation" &&
        "foreignKey" in meta
      ) {
        const narrowedMeta = meta;
        const fkValue = (model as any)[meta.foreignKey.name];
        if (fkValue) {
          return h(
            resolveComponent("router-link"),
            {
              // Resolve to an href to allow overriding of admin routes in userspace.
              // If we just gave a named raw location, it would always use the coalesce admin route
              // instead of the user-overridden one (that the user overrides by declaring another
              // route with the same path).
              to: this.router.resolve({
                name: "coalesce-admin-item",
                params: {
                  type: meta.typeDef.name,
                  id: (model as any)[meta.foreignKey.name],
                },
              }).fullPath,
            },
            () => propDisplay(model, narrowedMeta) ?? fkValue
          );
        }
      }
    }

    return CDisplay.render?.apply(this);
  },
});
