import { addYears, parse } from "date-fns";
import type {
  ComponentPublicInstance,
  ReactiveFlags,
  CreateComponentPublicInstance,
  WatchOptions,
} from "vue";
import { version } from "vue";
import { DateKind } from "./metadata";

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

const iso8601DateRegex =
  /^(\d{4})-(\d{2})-(\d{2})[T ](\d{2}):(\d{2}):(\d{2})(?:\.(\d{0,7}))?(?:Z|(.)(\d{2}):?(\d{2})?)?/;
export function parseJSONDate(argument: string) {
  // DO NOT USE `new Date()` here.
  // Safari incorrectly interprets times without a timezone offset
  // (i.e. DateTime objects in c#) as UTC instead of local time.
  // Safari is the only browser that does that. IE,Chrome,Firefox,Edge
  // all do this correctly.
  // The reason we originally used `new Date()` here was for sheer performance.
  // This idea came from the Knockout stack where `moment(...)` is 20x slower than
  // `new Date()` and it seemed at the time that the date ctor did behave the same
  // across all browsers. However, this has proven not to be the case in practice.

  // `date-fns` `parseJSON` function is susceptible to the same issue,
  // which is why this function is a adaptation of that function.
  // This method is only slightly slower than parseJSON,
  // but is still 3-4x faster than parseISO while also having much, much less code.

  var parts = argument.match(iso8601DateRegex) || [];

  const part9 = parts[9];
  if (part9 !== undefined) {
    return new Date(
      Date.UTC(
        +parts[1],
        +parts[2] - 1,
        +parts[3],
        +parts[4] - (+part9 || 0) * (parts[8] == "-" ? -1 : 1),
        +parts[5] - (+parts[10] || 0) * (parts[8] == "-" ? -1 : 1),
        +parts[6],
        +((parts[7] || "0") + "00").substring(0, 3)
      )
    );
  } else {
    return new Date(
      +parts[1],
      +parts[2] - 1,
      +parts[3],
      +parts[4],
      +parts[5],
      +parts[6],
      +((parts[7] || "0") + "00").substring(0, 3)
    );
  }
}

export function parseDateUserInput(
  input: string,
  defaultDate: Date,
  dateKind: DateKind
) {
  // DO NOT do this if the input doesn't have a date part.
  // Behavior of new Date() is generally always Invalid Date if you just give it a time,
  // except if you're on Chrome and give it an invalid time like "8:98 AM" - it'll give you "Thu Jan 01 1998 08:00:00".
  // Since the user wouldn't ever see the date part when only entering a time, there's no chance to detect this error.
  input = input.trim();
  if (dateKind !== "time") {
    // Space is treated as a valid separator here because new Date() treats it like a separator.
    const mmddyy = /^(\d+)([\-\/\\\. ])(\d+)(?:\2(\d+))?/.exec(input);
    if (mmddyy) {
      // Parse formats like "month/day" (without a year)
      const month = parseInt(mmddyy[1]) - 1;
      const sep = mmddyy[2];
      const day = parseInt(mmddyy[3]);
      const year = parseInt(mmddyy[4]);

      if (!year) {
        // If the year is missing, add the best match to the current year
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

        input = input
          .replace(
            mmddyy[0],
            mmddyy[0].trim().replace(sep, "/") +
              "/" +
              bestMatch.getFullYear() +
              " "
          )
          .trim();
      } else {
        // just normalize the separators:
        input = input
          .replace(
            mmddyy[0],
            mmddyy[0]
              .trim()
              .replace(
                new RegExp(sep.replace(/[-\/\\^$*+?.()|[\]{}]/g, "\\$&"), "g"),
                "/"
              )
          )
          .trim();
      }
    }
  }

  if (dateKind != "date") {
    // If the AM/PM doesn't have a space before it, new Date() can't parse it. Try to correct:
    input = input.replace(/(\d)(a|p)/i, "$1 $2");

    // If the AM/PM lacks the "M", add it (new Date() won't parse without it)
    input = input.replace(/\b(a|p)\b/i, "$1m");

    // If the time is all mashed together without a separator, fix it
    input = input.replace(
      /((?:^|\s+)\d{1,2})(\d{2})((?:\s+am|\s+pm)|$)/i,
      "$1:$2$3"
    );

    // If it looks like there's hours but no minutes, fix it:
    input = input.replace(/((?:^|\s)\d{1,2})(\s+(?:am|pm))\b/i, "$1:00$2");
  }

  const segments = [...input.matchAll(/\w+\b/g)].length;

  if (dateKind == "time") {
    // The date ctor can't parse time-only values.
    if (segments == 1) {
      // Single contiguous number - parse as 24h
      return parse(input, "HHmm", defaultDate);
    } else if (segments == 2) {
      // Time with two parts number - parse as 24h
      return parse(input, "HH:mm", defaultDate);
    } else {
      return parse(input, "hh:mm aa", defaultDate);
    }
  }

  if (segments <= 1) {
    // Prevent an input like "7" or "1 7" from parsing as "1/1/2007"
    return undefined;
  }

  return new Date(input);
}

/**
 * Converts deep Javascript objects to URL encoded query strings.
 * @license MIT
 */
/*
 * Code extracted from jQuery.param() and boiled down to bare metal js.
 * Should handle deep/nested objects and arrays in the same manner as jQuery's ajax functionality.
 * Origin: https://gist.github.com/dgs700/4677933
 * Replaces NPM module "qs" after it became bloated with dependencies in a commit dated 3/18/2021.
 * license: MIT
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

type RelevantVueProps = "$route" | "$router" | "$nextTick";
/** A type that accepts a component instance in both Vue2 and Vue3. Notably, the type of `this` in methods in the options API is not assignable to ComponentPublicInstance, but it is to CreateComponentPublicInstance. */
export type VueInstance = Pick<
  ComponentPublicInstance | CreateComponentPublicInstance,
  RelevantVueProps
> & {
  // $watch is defined independently because its `this` type on the callback messes things up.
  $watch(
    expOrFn: string,
    callback: (n: any, o: any) => void,
    options?: WatchOptions
  ): () => void;
  $watch<T>(
    expOrFn: () => T,
    callback: (n: T, o: T) => void,
    options?: WatchOptions
  ): () => void;
};

export function getInternalInstance(vue: VueInstance): any {
  // This returns `any` because its just kind of impossible to type correctly.

  // @ts-ignore vue2/vue3 compat shim.
  if ("$" in vue) return vue.$;
  // Vue2.7 doesn't have a notion of InternalInstance,
  // so places that use it in Vue3 use the public instance instead.
  return vue;
}

export const IsVue2 = version.charAt(0) == "2";
export const IsVue3 = version.charAt(0) == "3";
