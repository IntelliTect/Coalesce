import { defineComponent, getCurrentInstance } from "vue";
import { type DateKind, bindToQueryString } from "../src";
import { parseDateUserInput } from "../src/util";

describe("parseDateUserInput", () => {
  const defaultDefaultDate = new Date(2020, 10, 7);
  test.each(<Array<[DateKind, string, Date, Date | null]>>[
    ["date", "12/6", new Date(2020, 11, 6), new Date(2020, 10, 7)],
    ["date", " 1/3 ", new Date(2021, 0, 3), new Date(2020, 10, 7)],
    ["date", "12-7", new Date(2019, 11, 7), new Date(2020, 1, 24)],
    ["date", "1.4", new Date(2020, 0, 4), new Date(2020, 1, 24)],
    ["date", "3/4/19", new Date(2019, 2, 4), null],
    ["time", "3a", new Date(2020, 10, 7, 3), null],
    ["time", "3 a", new Date(2020, 10, 7, 3), null],
    ["time", "313 am", new Date(2020, 10, 7, 3, 13), null],
    ["time", "3am", new Date(2020, 10, 7, 3), null],
    ["time", "123a", new Date(2020, 10, 7, 1, 23), null],
    ["time", "1127p", new Date(2020, 10, 7, 23, 27), null],
    ["time", "1127", new Date(2020, 10, 7, 11, 27), null],
    ["time", "1527", new Date(2020, 10, 7, 15, 27), null],
    ["time", "15:27", new Date(2020, 10, 7, 15, 27), null],
    ["datetime", "12/2 3a", new Date(2020, 11, 2, 3), null],
    ["datetime", "12/2/2020 834 AM", new Date(2020, 11, 2, 8, 34), null],
    ["datetime", "4/11", new Date(2020, 3, 11), new Date(2020, 11, 2, 8, 34)],
    [
      "datetime",
      "4/11 1327",
      new Date(2020, 3, 11, 13, 27),
      new Date(2020, 11, 2, 8, 34),
    ],

    ["date", "1", undefined, null],
    ["date", "1 7", new Date(2021, 0, 7), null],
    ["date", "1 7 2022", new Date(2022, 0, 7), null],
    ["datetime", "1", undefined, null],
    ["datetime", "1 7", new Date(2021, 0, 7), null],
    ["datetime", "1 7 2022", new Date(2022, 0, 7), null],
  ])("%s => %s", (kind, input, expected, today) => {
    expect(
      parseDateUserInput(input, today || defaultDefaultDate, kind)
    ).toEqual(expected);
  });
});

describe("VueInstance", () => {
  test("is assignable from defineComponent `this`", async () => {
    defineComponent({
      data() {
        return { a: 1 };
      },
      created() {
        bindToQueryString(this, this, "a");
      },
    });
  });

  test("is assignable from getCurrentInstance", async () => {
    () => bindToQueryString(getCurrentInstance()!.proxy!, { a: 1 }, "a");
  });

  test("is not assignable from invalid object", async () => {
    () =>
      bindToQueryString(
        //@ts-expect-error
        window,
        { a: 1 },
        "a"
      );
  });

  test("is not assignable from invalid scalar", async () => {
    () =>
      bindToQueryString(
        //@ts-expect-error
        "foo",
        { a: 1 },
        "a"
      );
  });
});
