<template>
  <v-combobox
    class="c-select-string-value"
    :modelValue="internalValue ?? undefined"
    @update:modelValue="onInput"
    :loading="loading"
    :items="items"
    v-model:search="search"
    v-bind="inputBindAttrs"
  >
  </v-combobox>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { ModelApiClient, ItemResultPromise } from "coalesce-vue";
import { makeMetadataProps, useMetadataProps } from "../c-metadata-component";

const MODEL_REQUIRED_MESSAGE =
  "c-select-string-value requires a model to be provided via the `model` prop.";

export default defineComponent({
  name: "c-select-string-value",

  setup(props) {
    const metaProps = useMetadataProps(props);
    const modelMeta = metaProps.modelMeta.value;

    if (!modelMeta || modelMeta.type != "model") {
      throw Error(MODEL_REQUIRED_MESSAGE);
    }

    const methodMeta = modelMeta.methods[props.method];

    if (
      !methodMeta ||
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
        // (c as any)[methodMeta.name](page, search) as ListResultPromise<string>
      })
      .setConcurrency("debounce");

    return { ...metaProps, caller };
  },

  props: {
    ...makeMetadataProps(),
    method: { required: true, type: String },
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

      throw Error(MODEL_REQUIRED_MESSAGE);
    },
  },

  methods: {
    // `unknown` because vuetify's types are a little weird right now (wont infer `string`)
    onInput(value: unknown) {
      if (this.model && this.valueMeta) {
        return ((this.model as any)[this.valueMeta.name] = value);
      }

      this.$emit("input", value);
    },
  },

  mounted() {
    this.caller();
  },
});
</script>
