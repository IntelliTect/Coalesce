<template>
  <v-combobox
    class="c-select-string-value"
    :modelValue="internalValue ?? undefined"
    @update:modelValue="onInput"
    :loading="loading"
    :items="items"
    :hide-no-data="false"
    v-model:searchInput="search"
    v-bind="inputBindAttrs"
  >
  </v-combobox>
  <!-- TODO: This component has a lot of issues in vuetify3:
    - Dropdown menu hides until the second keystroke.
    - Current value disappears when the component receives focus if the value has not yet been edited since the component mounted.
  -->
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { ModelApiClient, ItemResultPromise } from "coalesce-vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

const MODEL_REQUIRED_MESSAGE =
  "c-select-string-value requires a model to be provided via the `model` prop, or a type to be provided via the `for` prop.";

export default defineComponent({
  name: "c-select-string-value",

  // We manually pass attrs via inputBindAttrs, so disable the default Vue behavior.
  // If we don't do this, some HTML attrs (e.g. tabindex) will incorrectly be placed
  // on the root element rather than on the search field in Vuetify component.
  inheritAttrs: false,

  setup(props) {
    const metaProps = useMetadataProps(props);
    const valueMeta = metaProps.valueMeta.value;
    const modelMeta =
      metaProps.modelMeta.value ??
      (valueMeta?.type == "model" ? valueMeta.typeDef : null);

    if (!modelMeta || modelMeta.type != "model") {
      throw Error(MODEL_REQUIRED_MESSAGE);
    }

    const methodMeta = modelMeta.methods[props.method];

    if (!methodMeta) {
      throw Error(
        `No method named ${props.method} could be found on type ${modelMeta.name}. Note: method name is expected to be camelCase.`
      );
    }

    if (
      !methodMeta.isStatic ||
      methodMeta.transportType != "item" ||
      methodMeta.return.type != "collection" ||
      methodMeta.return.itemType.type != "string"
    ) {
      throw Error(
        "c-select-string-value requires a static model method that returns an array of strings."
      );
    }

    const caller = new ModelApiClient(modelMeta)
      .$withSimultaneousRequestCaching()
      .$makeCaller("item", (c, page?: number, search?: string) => {
        return c.$invoke(methodMeta, {
          page,
          search,
          ...props.params,
        }) as ItemResultPromise<string[]>;
      })
      .setConcurrency("debounce");

    return { ...metaProps, caller };
  },

  props: {
    ...makeMetadataProps(),
    method: { required: true, type: String },
    modelValue: { required: false, type: String },
    params: { required: false, type: Object },
    listWhenEmpty: { required: false, default: false, type: Boolean },
  },

  data() {
    return {
      search: null as string | null,
    };
  },

  watch: {
    search(newVal: string, oldVal: string) {
      if (!newVal && !this.listWhenEmpty) {
        return;
      }

      if (newVal != oldVal) {
        // Single equals intended. Works around https://github.com/vuetifyjs/vuetify/issues/7344,
        // since null == undefined, the transition from undefined to null will fail.
        this.caller(1, newVal);
      }
    },
  },

  computed: {
    loading() {
      return this.caller.isLoading;
    },

    items() {
      if (!this.search && !this.listWhenEmpty) return [];
      return this.caller.result || [];
    },

    internalValue(): string | null {
      if (this.model && this.valueMeta) {
        return (this.model as any)[this.valueMeta.name];
      }

      return this.modelValue ?? null;
    },
  },

  methods: {
    // `unknown` because vuetify's types are a little weird right now (wont infer `string`)
    onInput(value: unknown) {
      if (this.model && this.valueMeta) {
        (this.model as any)[this.valueMeta.name] = value;
      }

      this.$emit("update:modelValue", value);
    },
  },

  mounted() {
    this.caller();
  },
});
</script>
