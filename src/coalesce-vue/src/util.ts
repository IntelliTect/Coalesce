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
