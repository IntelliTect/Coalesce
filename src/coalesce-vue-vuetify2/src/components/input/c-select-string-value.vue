<template>
  <v-combobox
    class="c-select-string-value"
    :value="fakeValue"
    @input="onInput"
    v-on="{ ...$listeners }"
    :loading="loading"
    :items="items"
    :search-input.sync="search"
    v-bind="inputBindAttrs"
    ref="combobox"
  >
  </v-combobox>
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
  // on the root element rather than on the search field in the Vuetify component.
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
        "c-select-string-value requires a static model method that returns a collection of strings."
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
    value: { required: false, type: String },
    params: { required: false, type: Object },
    listWhenEmpty: { required: false, default: false, type: Boolean },

    // Eager: Vuetify2 only, workaround https://github.com/vuetifyjs/vuetify/issues/4679
    eager: { default: false, type: Boolean },
  },

  data() {
    return {
      search: null as string | null,

      // Vuetify2 only: part of a workaround for https://github.com/vuetifyjs/vuetify/issues/4679.

      // `fakeValue` is the value that we pass to the v-combobox's `value` prop.
      // We have to do this because if you change the `value` prop of the combobox while typing,
      // the built-in filtering breaks.

      // `realValue` is what we expect the current value of `internalValue` to be.
      // We use this to avoid updating `fakeValue` while the user is typing if the values
      // are not in agreement.
      fakeValue: null as string | null,
      realValue: null as string | null,
    };
  },

  watch: {
    // Part of a workaround for https://github.com/vuetifyjs/vuetify/issues/4679
    internalValue: {
      immediate: true,
      handler(val: string) {
        if (
          this.fakeValue == this.realValue ||
          //@ts-expect-error
          // Always update fakeValue if the menu is closed,
          // since it means the user isn't searching and is therefore not subject to
          // the bugs that break filtering when the value is changed while searching.
          !this.$refs.combobox?.isMenuActive
        ) {
          this.fakeValue = this.realValue = val;
        }
      },
    },

    search(newVal: string, oldVal: string) {
      if (this.eager) {
        // v-combobox in Vuetify2 doesn't emit `update` events until a selection is made.
        // This means that normally it behaves more like a v-autocomplete or v-select
        // rather than a v-text-field. So, we've added the option to let it do the latter.
        const oldFake = this.fakeValue;
        this.onInput(newVal);
        this.fakeValue = oldFake;
      }

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

      return this.value ?? null;
    },
  },

  methods: {
    onInput(value: string) {
      this.realValue = this.fakeValue = value;
      if (this.model && this.valueMeta) {
        (this.model as any)[this.valueMeta.name] = value;
      }
      this.$emit("input", value);
    },
  },

  mounted() {
    this.caller();
  },
});
</script>
