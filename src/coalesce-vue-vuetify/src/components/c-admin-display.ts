import Vue, { PropOptions } from "vue";
import { getValueMeta } from "./c-metadata-component";
import { propDisplay, valueDisplay, Property, DisplayOptions, Model, ClassType } from "coalesce-vue";
import type { RawLocation } from 'vue-router'

import CDisplay from './c-display'

export default Vue.extend({
  name: "c-admin-display",
  functional: true,
  props: {
    for: <PropOptions<any>>{ required: false },
    model: <PropOptions<Model<ClassType>>>{ type: Object },
  },

	render(h, ctx) {
    const props = ctx.props;
    const { model } = props;

    if (model == null) {
      // If no model was provided, just display nothing.
      // This isn't an error case - it just means the thing we're tring to display 
      // is `null`-ish, and should be treated the same way that vue would treat {{null}}
      return h('span');
    }

    const modelMeta = model ? model.$metadata : null;
    let meta = getValueMeta(props.for, modelMeta, ctx.parent.$coalesce.metadata);
    if (!meta && modelMeta && "displayProp" in modelMeta) {
      meta = modelMeta.displayProp || null;
    }

    if (!meta) {
      throw Error("Provided model has no $metadata property, and no specific value was provided via the 'for' component prop to c-display.");
    }

    if ("params" in meta) {
      throw Error("Cannot display a method");
    }

    if (modelMeta?.type == "model") {
      const pkValue = (model as any)[modelMeta.keyProp.name]

      // Display collection navigations as counts with links to the c-admin-table-page for the collection
      if (pkValue && meta.role == "collectionNavigation" && "foreignKey" in meta) {
        return h(
          'router-link', {
            props: {
              to: <RawLocation>{ 
                name: 'coalesce-admin-list', 
                params: { type: meta.itemType.typeDef.name },
                query: { ['filter.' + meta.foreignKey.name]: pkValue }
              }
            } 
          },
          // Use `propDisplay` for our formatted count, forcing the count always by preventing enumeration.
          propDisplay(model, meta, { collection: { enumeratedItemsMax: 0 }})
            || "None"
        )
      }
      
      // Display reference navigations with links to the editor page for the item.
      if (pkValue && meta.role == "referenceNavigation" && "foreignKey" in meta) {
        const fkValue = (model as any)[meta.foreignKey.name]
        if (fkValue) {
          return h(
            'router-link', {
              props: {
                to: <RawLocation>{ 
                  name: 'coalesce-admin-item', 
                  params: { 
                    type: meta.typeDef.name,
                    id: (model as any)[meta.foreignKey.name]
                  },
                }
              } 
            },
            propDisplay(model, meta)
          )
        }
      }
    }
    
    return h(CDisplay, { props: props, ...ctx.data }, ctx.children);
  }
});
