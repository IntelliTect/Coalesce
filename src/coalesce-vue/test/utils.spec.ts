import { defineComponent, getCurrentInstance } from "vue";
import { bindToQueryString } from "../src";
import { IsVue2, parseDateUserInput } from "../src/util";

describe("parseDateUserInput", () => {
  const defaultDefaultDate = new Date(2020, 10, 7);
  test.each(<Array<[string, Date, Date | null]>>[
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

describe("VueInstance", () => {
  test("is assignable from defineComponent `this`", async () => {
    defineComponent({
      created() {
        bindToQueryString(this, this, 'a')
      }
    })
  })

  test("is assignable from getCurrentInstance", async () => {
    () => bindToQueryString(getCurrentInstance()!.proxy!, {}, 'a')
  })

  test("is not assignable from invalid object", async () => {
    //@ts-expect-error
    () => bindToQueryString(window, this, 'a')
  })

  test("is not assignable from invalid scalar", async () => {
    //@ts-expect-error
    () => bindToQueryString("foo", this, 'a')
  })
})