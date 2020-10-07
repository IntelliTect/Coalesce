import { parseDateUserInput } from "../src/util";

describe("parseDateUserInput", () => {
  const defaultDefaultDate = new Date(2020, 10, 7);
  test.each(<Array<[string, Date, Date|null]>>[
    ["12/6", new Date(2020, 11, 6), new Date(2020, 10, 7)],
    [" 1 / 3 ", new Date(2021, 0, 3), new Date(2020, 10, 7)],
    ["12-7", new Date(2019, 11, 7), new Date(2020, 1, 24)],
    ["1.4", new Date(2020, 0, 4), new Date(2020, 1, 24)],
    ["3/4/19", new Date(2019, 2, 4), null],
  ])("%s => %s", (input, expected, today) => {
    expect(parseDateUserInput(input, today || defaultDefaultDate)).toEqual(
      expected
    );
  });
});
