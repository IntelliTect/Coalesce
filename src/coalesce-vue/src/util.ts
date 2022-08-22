import { addYears } from "date-fns";
import {
  type ComponentPublicInstance,
  type ReactiveFlags,
  version,
  CreateComponentPublicInstance,
} from "vue";

export type OwnProps<T, TExclude> = Pick<T, Exclude<keyof T, keyof TExclude>>;

export type Indexable<T> = { [k: string]: any | undefined } & T;

export type DeepPartial<T> =
  | T
  | {
      [P in keyof T]?: T[P] extends Array<infer U>
        ? Array<DeepPartial<U>>
        : T[P] extends ReadonlyArray<infer U>
        ? ReadonlyArray<DeepPartial<U>>
        : DeepPartial<T[P]>;
    };

export function isNullOrWhitespace(value: string | null | undefined) {
  // Manually looping here is leagues faster than using a regex or .trim().

  if (value == null || value === "") {
    return true;
  }

  for (let i = 0; i < value.length; i++) {
    switch (value[i]) {
      case " ":
      case "\t":
      case "\r":
      case "\n":
        continue;
      default:
        return false;
    }
  }
  return true;
}

export function parseDateUserInput(input: string, defaultDate: Date) {
  const mmdd = /^\s*(\d+)\s*[\-\/\\\.]\s*(\d+)\s*$/.exec(input);
  if (mmdd) {
    // Parse formats like "month/day" (without a year)
    const month = parseInt(mmdd[1]) - 1;
    const day = parseInt(mmdd[2]);
    const parsed = new Date(
      defaultDate.getFullYear(),
      month,
      day,
      defaultDate.getHours(),
      defaultDate.getMinutes(),
      defaultDate.getSeconds(),
      defaultDate.getMilliseconds()
    );

    // Find the closest occurrence of the given mm/dd to the defaultDate.
    // For e.g., if the current date is Jan 1 2020 and a user enters 12/20,
    // they most likely meant Dec 20 2019, not Dec 20 2020.
    // Likewise, If the current date is Dec 20 2019 and the user enters 1/1,
    // they most likely meant Jan 1 2020, not Jan 1 2019.
    const bestGuessMaxDiffMs = 120 * 24 * 60 * 60 * 1000;
    let bestMatch: Date = parsed;
    let bestMatchDiff = Math.pow(2, 52);
    for (const candidate of [
      addYears(parsed, 1),
      parsed,
      addYears(parsed, -1),
    ]) {
      const diff = Math.abs(defaultDate.valueOf() - candidate.valueOf());
      if (diff < bestMatchDiff && diff <= bestGuessMaxDiffMs) {
        bestMatch = candidate;
        bestMatchDiff = diff;
      }
    }
    return bestMatch;
  }

  return new Date(input);
}

/**
 * Converts deep Javascript objects to URL encoded query strings.
 * Code extracted from jQuery.param() and boiled down to bare metal js.
 * Should handle deep/nested objects and arrays in the same manner as jQuery's ajax functionality.
 * Origin: https://gist.github.com/dgs700/4677933
 * Replaces NPM module "qs" after it became bloated with dependencies in a commit dated 3/18/2021.
 * @license MIT
 */
export function objectToQueryString(
  a: Array<any> | { [s: string]: any } | null
) {
  var items: Array<any> = [];
  const add = function (key: string, value: any) {
    value = value == null ? "" : value;
    items[items.length] =
      encodeURIComponent(key) +
      "=" +
      encodeURIComponent(value == null ? "" : value);
  };

  buildParams("", a, add);

  return items.join("&");
}

export function objectToFormData(a: Array<any> | { [s: string]: any } | null) {
  var items = new FormData();
  const add = function (key: string, value: any) {
    if (value instanceof Uint8Array) {
      // Add raw binary as blobs
      value = new Blob([value]);
    }

    items.append(key, value == null ? "" : value);
  };

  buildParams("", a, add);

  return items;
}

const rbracket = /\[\]$/;
function isScalarFormValue(obj: any) {
  return (
    obj instanceof Blob || obj instanceof Uint8Array || typeof obj !== "object"
  );
}
function buildParams(
  prefix: string,
  obj: any,
  add: (key: string, value: any) => void
) {
  var name;
  if (obj instanceof Array) {
    var isScalarArray = obj.every(isScalarFormValue);
    for (let i = 0, l = obj.length; i < l; i++) {
      const v = obj[i];
      if (isScalarArray || rbracket.test(prefix)) {
        // Treat each array item as a scalar.
        add(prefix, v);
      } else {
        // Item is non-scalar (array or object), encode its numeric index.
        const k = typeof v === "object" && v != null ? i.toString() : "";
        buildParams(prefix ? prefix + "[" + k + "]" : k, v, add);
      }
    }
  } else if (obj === null) {
    // Handle null before typeof obj == "object", which will be the case for null.
    // Serialize scalar item.
    add(prefix, obj);
  } else if (typeof obj == "object" && !isScalarFormValue(obj)) {
    // Serialize object item.
    for (name in obj) {
      if (obj.hasOwnProperty(name)) {
        buildParams(prefix ? prefix + "[" + name + "]" : name, obj[name], add);
      }
    }
  } else {
    // Serialize scalar item.
    add(prefix, obj);
  }
}

/** This is the property added by `markRaw`.
 * We use it directly so we can declare it on the proto of ViewModel/ListViewModel
 * rather than calling markRaw on each instance.
 *
 * We make these classes nonreactive with Vue, preventing ViewModel instance from being wrapped with a Proxy.
 * To achieve reactivity, we instead make individual members reactive with `reactive`/`ref`.
 *
 * We have to do this because `reactive` doesn't play nice with prototyped objects.
 * Any sets to a setter on the ViewModel class will trigger the reactive proxy,
 * and since the setter is defined on the prototype and Vue checks hasOwnProperty
 * when determining if a field is new on an object, all setters trigger reactivity
 * even if the value didn't change.
 *
 * We cant use the export of ReactiveFlags from vue because of https://github.com/vuejs/core/issues/1228
 */
export const ReactiveFlags_SKIP = "__v_skip" as ReactiveFlags.SKIP;

/** A type that accepts a component instance in both Vue2 and Vue3. Notably, the type of `this` in methods in the options API is not assignable to ComponentPublicInstance, but it is to CreateComponentPublicInstance. */
export type VueInstance =
  | ComponentPublicInstance
  | CreateComponentPublicInstance;

export function getInternalInstance(vue: VueInstance) {
  // @ts-ignore vue2/vue3 compat shim.
  if ("$" in vue) return vue.$;
  // Vue2.7 doesn't have a notion of InternalInstance,
  // so places that use it in Vue3 use the public instance instead.
  return vue;
}

export const IsVue2 = version.charAt(0) == "2";
export const IsVue3 = version.charAt(0) == "3";
