<template>
  <div class="template-builder">
    <div class="border" style="padding: 4px">
      <label
        v-for="{ param, depth } in treeParameters"
        :key="param.key"
        :style="{
          opacity: !param.meetsReqs ? 0.5 : 1,
          marginLeft: depth * 32 + 'px',
        }"
        class="template-builder-option"
      >
        <input
          type="checkbox"
          :disabled="!param.meetsReqs"
          v-model="selections"
          :value="param.key"
        />
        <div style="flex-grow: 1">
          <div style="font-weight: bold">
            <span style="font-size: 18px">{{ param.displayName }}</span>
            <div style="float: right">
              <code style="font-size: small">--{{ param.key }}</code>
            </div>
          </div>
          <div style="font-size: 12px; line-height: 1.3; margin-top: 4px">
            <div style="white-space: pre-wrap">
              {{ param.description }}
              <a v-if="param.link" :href="param.link" target="_blank">
                Read More</a
              >
            </div>

            <div
              v-if="param.requires"
              style="
                font-style: italic;
                margin-top: 0px;
                transition: max-height 0.25s;
                overflow: hidden;
              "
              :style="{ maxHeight: !param.meetsReqs ? '30px' : '0px' }"
            >
              Requires {{ param.requires }}
            </div>

            <div
              v-if="param.warning"
              style="
                font-style: italic;
                margin-top: 0px;
                transition: max-height 0.25s;
                overflow: hidden;
                color: var(--vp-c-warning-1);
              "
              :style="{
                maxHeight:
                  param.warning.active && selections.includes(param.key)
                    ? '30px'
                    : '0px',
              }"
            >
              ⚠ {{ param.warning.message }}
            </div>
          </div>
        </div>
      </label>

      <hr />
      <label style="padding: 0 12px 14px; display: flex; align-items: center">
        <span style="font-weight: bold; font-size: 18px; padding-right: 8px">
          Root Namespace:
        </span>
        <input
          type="text"
          v-model="namespace"
          placeholder="MyCompany.MyProject"
          class="border"
          style="
            padding: 6px 12px;
            font-family: monospace;
            font-size: 16px;
            margin-top: 4px;
            border-color: var(--v-c-text-2);
            background-color: field;
            flex-grow: 1;
          "
        />
      </label>
    </div>
  </div>
</template>

<style lang="scss">
.template-builder {
  .border {
    border: 1px solid var(--vp-code-bg);
    border-radius: 4px;
  }
}
.template-builder-option {
  border-radius: 4px;
  padding: 8px;
  display: flex;
  align-items: start;
  cursor: pointer;

  input[type="checkbox"] {
    height: 22px;
    width: 22px;
    margin-right: 16px;
    flex-shrink: 0;
  }

  &:hover {
    background-color: var(--vp-code-bg);
    transition:
      color 0.25s,
      background-color 0.5s;
  }
}
</style>

<script setup lang="ts">
import { ref, watch } from "vue";
import { withBase } from "vitepress";
import templateJson from "../../../templates/Coalesce.Vue.Template/content/.template.config/template.json";

type Parameter = {
  key: string;
  displayName: string;
  description?: string;
  meetsReqs: boolean;
  requires: string;
  warning: { message: string; active: boolean } | null;
  onlyIf?: string[];
  link?: string;
  parent?: string;
};

const options = defineModel<string>("options");
const namespace = defineModel<string>("namespace");

const parameters = Object.entries(templateJson.symbols)
  .map(([k, v]) => ({
    ...v,
    key: k,
    get meetsReqs() {
      return !("$coalesceRequires" in v) || evalReq(v.$coalesceRequires as any);
    },
    get requires() {
      return !("$coalesceRequires" in v)
        ? null
        : displayReq(v.$coalesceRequires as any, getAncestors(k));
    },
    get warning() {
      if (!("$coalesceWarning" in v)) return null;
      const w = v.$coalesceWarning as {
        ifNot: Requirements;
        message: string;
      };
      return {
        message: w.message,
        active: !evalReq(w.ifNot),
      };
    },
    link:
      "$coalesceLink" in v
        ? v.$coalesceLink.startsWith("/")
          ? withBase(v.$coalesceLink)
          : v.$coalesceLink
        : undefined,
    parent: "$coalesceParent" in v ? (v.$coalesceParent as string) : undefined,
  }))
  .filter((x) => x.type == "parameter") as Parameter[];

const treeParameters = (() => {
  const result: { param: Parameter; depth: number }[] = [];
  function addChildren(parentKey: string | undefined, depth: number) {
    for (const p of parameters) {
      if (p.parent === parentKey) {
        result.push({ param: p, depth });
        addChildren(p.key, depth + 1);
      }
    }
  }
  addChildren(undefined, 0);
  return result;
})();

function getAncestors(key: string): Set<string> {
  const ancestors = new Set<string>();
  let current = parameters.find((p) => p.key === key);
  while (current?.parent) {
    ancestors.add(current.parent);
    current = parameters.find((p) => p.key === current!.parent);
  }
  return ancestors;
}

const selections = ref([
  "Identity",
  "TrackingBase",
  "DarkMode",
  "AuditLogs",
  "UserPictures",
  "LocalAuth", // https://github.com/IntelliTect/Coalesce/issues/522
  "Passkeys",

  // Azure
  "GithubActions",
  "BlobStorage",
  "KeyVault",
  "AppInsights",
]);

watch(
  selections,
  (s) => {
    options.value = s
      .filter((x) => parameters.find((p) => p.key == x)?.meetsReqs)
      .map((x) => "--" + x)
      .join(" ");
  },
  { immediate: true },
);

type Requirements = ["and" | "or", ...(Requirements | string)[]];
function evalReq(req: Requirements): boolean {
  if (req[0] == "and") {
    return req
      .slice(1)
      .every((req) =>
        typeof req == "string" ? selections.value.includes(req) : evalReq(req),
      );
  } else if (req[0] == "or") {
    return req
      .slice(1)
      .some((req) =>
        typeof req == "string" ? selections.value.includes(req) : evalReq(req),
      );
  } else {
    return false;
  }
}
function displayReq(req: Requirements, exclude?: Set<string>): string | null {
  if (req[0] == "and") {
    const parts = req
      .slice(1)
      .filter((r) => !(typeof r == "string" && exclude?.has(r)))
      .map((req) =>
        typeof req == "string"
          ? parameters.find((p) => p.key == req)?.displayName
          : "(" + displayReq(req, exclude) + ")",
      );
    return parts.length ? parts.join(" and ") : null;
  } else if (req[0] == "or") {
    const parts = req
      .slice(1)
      .filter((r) => !(typeof r == "string" && exclude?.has(r)))
      .map((req) =>
        typeof req == "string"
          ? parameters.find((p) => p.key == req)?.displayName
          : "(" + displayReq(req, exclude) + ")",
      );
    return parts.length ? parts.join(" or ") : null;
  } else {
    return null;
  }
}
</script>
