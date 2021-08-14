import { addYears } from "date-fns";

export type OwnProps<T, TExclude> = Pick<T, Exclude<keyof T, keyof TExclude>>;

export type Indexable<T> = { [k: string]: any | undefined } & T;

export type DeepPartial<T> = {
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
    const bestGuessMaxDiffMs = 120*24*60*60*1000;
    let bestMatch: Date = parsed;
    let bestMatchDiff = Math.pow(2, 52);
    for (const candidate of [
      addYears(parsed, 1),
      parsed,
      addYears(parsed, -1),
    ]) {
      const diff = Math.abs(
        defaultDate.valueOf() - candidate.valueOf()
      );
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
export function objectToQueryString(a: Array<any> | { [s: string]: any } | null) {
  var items: Array<any> = [];
  const add = function (key: string, value: any) {
    value = value == null ? "" : value;
    items[items.length] =
      encodeURIComponent(key) +
      "=" +
      encodeURIComponent(value == null ? "" : value);
  };

  if (a == null) {
    return "";
  }

  if (a instanceof Array) {
    for (let i = 0, l = a.length; i < l; i++) {
      add(i.toString(), a[i]);
    }
  } else {
    for (const prefix in a) {
      if (a.hasOwnProperty(prefix)) {
        buildParams(prefix, a[prefix], add);
      }
    }
  }

  return items.join("&");
}

const rbracket = /\[\]$/;
function buildParams(
  prefix: string,
  obj: any,
  add: (key: string, value: any) => void
) {
  var name;
  if (obj instanceof Array) {
    for (let i = 0, l = obj.length; i < l; i++) {
      const v = obj[i];
      if (rbracket.test(prefix)) {
        // Treat each array item as a scalar.
        add(prefix, v);
      } else {
        // Item is non-scalar (array or object), encode its numeric index.
        buildParams(
          prefix + "[" + (typeof v === "object" && v != null ? i : "") + "]",
          v,
          add
        );
      }
    }
  } else if (obj === null) {
    // Handle null before typeof obj == "object", which will be the case for null.
    // Serialize scalar item.
    add(prefix, obj);
  } else if (typeof obj == "object") {
    // Serialize object item.
    for (name in obj) {
      if (obj.hasOwnProperty(name)) {
        buildParams(prefix + "[" + name + "]", obj[name], add);
      }
    }
  } else {
    // Serialize scalar item.
    add(prefix, obj);
  }
}
