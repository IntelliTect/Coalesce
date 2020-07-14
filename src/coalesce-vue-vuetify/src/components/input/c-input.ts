

import Vue, { PropOptions } from "vue";
import { getValueMeta } from "../c-metadata-component";
import { Model, ViewModel, ClassType, DataSource, DataSourceType, ModelType, mapToModel, mapValueToModel, AnyArgCaller, ApiState } from "coalesce-vue";

import CSelect from './c-select.vue'
import CSelectManyToMany from './c-select-many-to-many.vue'
import CSelectValues from './c-select-values.vue'
import CDisplay from '../display/c-display';
import CDatetimePicker from './c-datetime-picker.vue';

const primitiveTypes = ["string", "number", "date", "enum", "boolean"];

export default Vue.extend({
  name: "c-input",
  functional: true,

  inject: {
    'c-input-props': { default: {} }
  },

  props: {
    for: <PropOptions<any>>{ required: false },
    model: <
      | PropOptions<Model<ClassType> 
      | DataSource<DataSourceType> 
      | AnyArgCaller>
    >{ required: false },
    value: <PropOptions<any>>{ required: false },
  },

	render(h, ctx) {

    let model = ctx.props.model; 
    const modelMeta = model ? model.$metadata : null;
    let valueMeta = getValueMeta(ctx.props.for, modelMeta, ctx.parent.$coalesce.metadata);

    if (!valueMeta) {
      throw Error("c-input requires value metadata. Specify it with the 'for' prop'");
    }

    // Support binding to method args via `:model="myModel.myMethod" for="myArg"`.
    // getValueMeta will resolve to the metadata of the specific parameter;
    // we then have to resolve the args object from the ApiState.
    if (model instanceof ApiState && "args" in model && valueMeta.name in model.args) {
      model = model.args
    }

    if (!valueMeta || !("role" in valueMeta)) {
      throw Error("c-input requires value metadata. Specify it with the 'for' prop'");
    }

    const { on: ctxOn, props: ctxProps, ...ctxData } = ctx.data;
    let data = {
      ...ctxData,
      attrs: undefined as typeof ctxData.attrs,

      props: {
        // If a model is provided, pull the value off the model.
        value: model
          ? (model as any)[valueMeta.name]
          : primitiveTypes.includes(valueMeta.type)
          ? mapValueToModel(ctx.props.value, valueMeta) 
          : ctx.props.value
      } as Exclude<typeof ctxProps, undefined>,

      on: {
        ...ctxOn,
      } as Exclude<typeof ctxOn, undefined>,
    }

    
    // Handle components that delegate to other c-metadata-component based components.
    // These components don't need to have complex attributes computed
    // because they will perform the same computation of attributes themselves.
    // They do this because they're designed to be used as standalone components
    // (and not strictly as components that have been delegated to by c-input).
    switch (valueMeta.type) {

      case 'date':
        data.attrs = {...ctxData.attrs};
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;
        return h(CDatetimePicker, data);

      case 'model':
        data.attrs = {...ctxData.attrs};
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;
        return h(CSelect, data);

      case 'collection':
        data.attrs = {...ctxData.attrs};
        data.props.model = ctx.props.model;
        data.props.for = ctx.props.for;

        if ('manyToMany' in valueMeta) {
          return h(CSelectManyToMany, data);
        } else if (valueMeta.itemType.type != 'model' && valueMeta.itemType.type != 'object') {
          return h(CSelectValues, data);
        } else {
          // console.warn(`Unsupported collection type ${valueMeta.itemType.type} for v-input`)
        }
    }


    // We've now handled any components that will do their own computation of these
    // attributes via c-metadata-component's inputBindAttrs.
    // We now need to compute them in order to render components 
    // that delegate directly to vuetify components.
    const attrs = data.attrs = {
      // If a label is not provided to the component, default to the displayName of the value. 
      label: valueMeta.displayName,

      rules: model && model instanceof ViewModel && valueMeta.name in (modelMeta as ModelType)!.props
        ? model.$getRules(valueMeta.name)
        : 'rules' in valueMeta && valueMeta.rules
          ? Object.values(valueMeta.rules)
          : undefined,

      ...ctx.injections?.['c-input-props'],
      ...ctxData.attrs,
    } as Exclude<typeof ctxData.attrs, undefined>

    data.on.input = function(value: any) {
      if (model && valueMeta) {
        (model as any)[valueMeta.name] = value;
      }
      if (ctxOn && typeof ctxOn.input == 'function') {
        ctxOn.input.apply(null, arguments);
      }
    }

    // Handle components that delegate immediately to Vuetify
    switch (valueMeta.type) {
      case 'string':
      case 'number':
        if (valueMeta.type == 'number') {
          // For numeric values, use a numeric text field.
          attrs.type == 'number'
        }

        if (valueMeta.role == "primaryKey") { 
          // If this is an editable primary key, emit the value on change (leaving the field)
          // instead of on every keystroke. If we were to emit on every keystroke,
          // the very first character the user types would end up as the PK.
          data.on.change = data.on.input;
          delete data.on.input;
        }

        if ('textarea' in attrs && attrs.textarea !== false) {
          return h('v-textarea', data);
        }
        return h('v-text-field', data);

      case 'boolean':
        // v-switch uses 'change' as its event, not 'input'.
        data.on.change = data.on.input;
        delete data.on.input;
        // It also uses 'input-value' instead of 'value' for its value prop.
        data.props['input-value'] = data.props.value;
        delete data.props.value;
        
        if ('checkbox' in attrs && attrs.checkbox !== false) {
          return h('v-checkbox', data);
        }
        return h('v-switch', data);
      
      case 'enum':
        data.props.items = valueMeta.typeDef.values
        data.props['item-text'] = 'displayName';
        data.props['item-value'] = 'value';
        return h('v-select', data);

      case 'file': 
        // v-file-input uses 'change' as its event, not 'input'.
        data.on.change = data.on.input;
        delete data.on.input;
        return h('v-file-input', data);
    }

    // Fall back to just displaying the value.
    // Note that this probably looks bad on Vuetify 2+ because we're
    // abusing its internal classes to try to emulate the look of a text field,
    // but this hasn't been updated for 2.0.
    if (ctx.children) {
      return h("div", {}, ctx.children)
    }
    return h('div', {
        staticClass:"input-group input-group--dirty input-group--text-field"
      },[
        h('label', attrs.label),
        h('p', [
          h(CDisplay, {
            staticClass: "subtitle-1",
            props:{
              value: data.props.value,
              model: ctx.props.model,
              for: ctx.props.for
            }
          })
        ])
      ]
    )
  }
});
