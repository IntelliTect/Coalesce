<template>
  <div class="c-method">
    <v-row
      class="my-0 c-method--section c-method--params"
      v-if="filteredParams.length"
    >
      <v-col> Parameters </v-col>
      <v-col class="py-0">
        <v-row class="my-0">
          <v-col
            v-for="param in filteredParams"
            :key="param.name"
            cols="12"
            sm="6"
            md="4"
            lg="3"
          >
            <c-input
              v-model="caller.args[param.name]"
              :for="param"
              hide-details="auto"
              density="compact"
              variant="outlined"
            />
          </v-col>
        </v-row>
      </v-col>
    </v-row>

    <v-row class="my-0 c-method--section c-method--results">
      <v-col>
        <template
          v-if="methodMeta.return.type == 'file' && !filteredParams.length"
        >
          <v-btn color="primary" @click="invoke" :loading="caller.isLoading">
            <span v-if="filteredParams.length">Execute</span>
            <span v-else>
              <v-icon start>fa fa-search</v-icon>
              Preview
            </span>
          </v-btn>
          <v-btn
            class="mt-1"
            color="primary"
            @click="invokeAndDownload"
            :loading="caller.isLoading"
          >
            <v-icon start>fa fa-download</v-icon>
            Download
          </v-btn>
        </template>
        <v-btn
          v-else
          color="primary"
          @click="invoke"
          :loading="caller.isLoading"
          sm
        >
          Execute
        </v-btn>
      </v-col>
      <v-col>
        <c-loader-status
          :progress-placeholder="false"
          :loaders="{
            'no-initial-content no-error-content no-loading-content': [caller],
          }"
          style="min-height: 55px"
        >
          <h3>
            Result:
            <v-btn
              v-if="caller.result && methodMeta.return.type == 'file'"
              color="primary"
              @click="downloadFileResult"
              :loading="caller.isLoading"
              size="x-small"
              outlined
            >
              <v-icon start small>fa fa-download</v-icon>
              Save to Disk
            </v-btn>
          </h3>

          <span
            v-if="
              caller.wasSuccessful &&
              (methodMeta.return.type == 'void' || caller.message)
            "
            class="c-method--result-success"
          >
            <v-alert type="success" dense>{{
              caller.message || "Success"
            }}</v-alert>
          </span>

          <div v-if="caller.result && methodMeta.return.type == 'file'">
            <pre
              >{{ caller.result.name }} • {{ caller.result.type }} • {{
                caller.result.size.toLocaleString()
              }} bytes</pre
            >

            <br />

            <template
              v-if="
                fileDownloadKind == 'preview' && 'getResultObjectUrl' in caller
              "
            >
              <img
                v-if="caller.result.type.indexOf('image') >= 0"
                :src="caller.getResultObjectUrl(instance.proxy)"
                :alt="caller.result.name"
                class="elevation-1"
                style="max-width: 100%"
              />
              <video
                v-else-if="caller.result.type.indexOf('video') >= 0"
                :src="caller.getResultObjectUrl(instance.proxy)"
                :alt="caller.result.name"
                class="elevation-1"
                controls
                style="max-width: 100%"
              />
              <pre v-else>Unable to show preview.</pre>
            </template>
          </div>

          <c-display
            v-else-if="
              caller.result != null && methodMeta.return.type !== 'void'
            "
            element="pre"
            class="c-method--result-value"
            v-model="caller.result"
            :for="methodMeta.return"
            :options="resultDisplayOptions"
          />

          <span
            v-else-if="
              caller.wasSuccessful != null &&
              caller.result == null &&
              methodMeta.return.type !== 'void'
            "
            class="c-method--result-null"
          >
            <pre>{{ "" + caller.result }}</pre>
          </span>
        </c-loader-status>
      </v-col>
    </v-row>
  </div>
</template>

<script lang="ts" setup generic="TModel extends ViewModel | ListViewModel">
import { computed, ref } from "vue";
import { ViewModel, ListViewModel } from "coalesce-vue";
import type {
  DisplayOptions,
  AnyArgCaller,
  ItemApiState,
  Method,
} from "coalesce-vue";
import { getCurrentInstance } from "vue";

defineOptions({
  name: "c-admin-method",
});

const resultDisplayOptions = {
  collection: {
    enumeratedItemsMax: Infinity,
    enumeratedItemsSeparator: "\n",
  },
} as DisplayOptions;

type MethodsOf<TModel> = TModel extends {
  $metadata: {
    methods: infer O extends Record<string, Method>;
  };
}
  ? O
  : never;

type MethodForSpec =
  // Check if we only know that the type's method names are any strings
  "__never" extends keyof MethodsOf<TModel["$metadata"]>
    ? // If so, we have to allow any string because the exact method names aren't known.
      string | Method
    : // We know the exact method names of the type, so restrict to just those:
      keyof MethodsOf<TModel> | MethodsOf<TModel>[keyof MethodsOf<TModel>];

const props = defineProps<{
  /** An object owning the method that is specified by the `for` prop. */
  model: TModel;

  /** A metadata specifier for the method being bound. One of:
   * * A string with the name of the method belonging to `model`. E.g. `"myMethod"`.
   * * A direct reference to the metadata object. E.g. `model.$metadata.methods.myMethod`.
   */
  for: MethodForSpec;

  autoReloadModel?: boolean;
}>();

const fileDownloadKind = ref<"preview" | "download">("preview");
const instance = getCurrentInstance()!;

const methodMeta = computed((): Method => {
  const modelMeta = props.model.$metadata;
  const meta =
    typeof props.for == "string" ? modelMeta.methods[props.for] : props.for;

  if (meta && "params" in meta) {
    return meta;
  }
  throw Error("`c-method` requires metadata for a method.");
});

const filteredParams = computed(() => {
  return Object.values(methodMeta.value.params).filter((p) => !p.source);
});

const caller = computed((): AnyArgCaller => {
  const caller = (viewModel.value as any)[methodMeta.value.name];
  if (!caller)
    throw Error(
      `Method '${methodMeta.value.name}' doesn't exist on provided model.`
    );
  return caller;
});

const viewModel = computed((): ViewModel | ListViewModel => {
  if (props.model instanceof ViewModel) return props.model;
  if (props.model instanceof ListViewModel) return props.model;
  throw Error(
    "c-method: prop `model` is required, and must be a ViewModel or ListViewModel."
  );
});

async function invoke() {
  fileDownloadKind.value = "preview";
  await caller.value.invokeWithArgs();
  if (props.autoReloadModel) {
    await viewModel.value.$load();
  }
  if (methodMeta.value.autoClear) {
    caller.value.resetArgs();
  }
}

async function invokeAndDownload() {
  fileDownloadKind.value = "download";
  await caller.value.invokeWithArgs();

  if (props.autoReloadModel) {
    // Don't await. Just do this in the background while we setup the file download.
    viewModel.value.$load();
  }
  if (methodMeta.value.autoClear) {
    caller.value.resetArgs();
  }

  downloadFileResult();
}

function downloadFileResult() {
  const fileCaller = caller.value as ItemApiState<any, File>;
  const file = fileCaller.result;
  if (!(file instanceof File)) return;

  const a = document.createElement("a");
  document.body.appendChild(a);
  a.href = fileCaller.getResultObjectUrl(instance!.proxy)!;
  a.download = file.name;
  a.click();
  setTimeout(() => {
    document.body.removeChild(a);
  }, 1);
}
</script>

<style lang="scss">
.c-method--section {
  > .v-col:first-child {
    font-size: 18px;
    flex-grow: 0;
    min-width: 170px;
    text-align: right;
  }
}
.c-method--result {
  margin-top: 10px;
}
.c-method--result-error {
  white-space: pre-wrap;
  word-break: break-word;
}
.c-method--result-null {
  color: #999;
}
</style>
