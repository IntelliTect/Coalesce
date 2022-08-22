import { mount } from "@vue/test-utils";
import { defineComponent } from "vue";

export const MetadataSymbol = Symbol("metadata");

export function shortStringify(value: any, maxLen = 55) {
  // InvalidDate stringifies to null - we want to preserve the invalidity in the display.
  if (value instanceof Date) return `Date(${value.toString()})`;
  if (value === undefined) return "undefined";

  let str = JSON.stringify(value, (key, value) => {
    if (key === "$metadata") return MetadataSymbol;
    return value;
  });
  if (str.length > maxLen) {
    str = str.substr(0, maxLen) + "...";
  }
  return str;
}

export function expectDeepMatch(actual: any, expected: any) {
  if (
    expected &&
    typeof expected == "object" &&
    expected.constructor === Object
  ) {
    for (const k in expected) {
      if (k == "$metadata") {
        // Don't recurse into metadata because it will infinitely recurse.
        // Instead, expect reference equality.
        return expect(actual[k]).toBe(expected[k]);
      }
      expectDeepMatch(actual[k], expected[k]);
    }
    for (const k in actual) {
      if (!(k in expected)) {
        return expect(actual[k]).toBeUndefined();
      }
    }
  } else {
    return expect(actual).toEqual(expected);
  }
}

export async function delay(ms: number) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

export function mountData<T>(data: T) {
  return mount(
    defineComponent({
      template: "<div></div>",
      data() {
        return data;
      },
    })
  ).vm;
}
