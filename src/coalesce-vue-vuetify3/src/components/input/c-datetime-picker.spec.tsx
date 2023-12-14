import { Grade, Student } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import { delay, mount, nextTick } from "@test/util";
import { CDatetimePicker } from "..";

describe("CDatetimePicker", () => {
  let model: StudentViewModel;
  beforeEach(() => {
    model = new StudentViewModel({
      name: "bob",
      grade: Grade.Freshman,
      password: "secretValue",
      email: "bob@college.edu",
      phone: "123-123-1234",
      color: "#ff0000",
      notes: "multiline\n\nstring",
    });
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CDatetimePicker model={model.manyParams} for="date" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Date");

    // Set a value, and look for the value
    model.manyParams.args.date = new Date("2023-08-16T01:02:03Z");
    await delay(1);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(model.manyParams.args.date.getFullYear()).toBe(2017);
  });

  test("validation rules are passed date, not string", async () => {
    const rule = vitest.fn(
      (v) => !v || v.getFullYear() > 2017 || "Year must be > 2017"
    );

    const wrapper = mount(() => (
      //@ts-ignore useless error about extra properties
      <CDatetimePicker model={model} for="birthDate" rules={[rule]} />
    ));

    // Perform an input on the component, and then look at the args that were passed to the rule function:
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(wrapper.text()).toContain("Year must be > 2017");
    expect(model.birthDate?.getFullYear()).toBe(2017);
    expect(rule).toHaveBeenLastCalledWith(model.birthDate);

    // Do it again, but with a valid input this time. The error should be gone.
    await wrapper.find("input").setValue("1/3/2018");
    await delay(1);
    expect(wrapper.text()).not.toContain("Year must be > 2017");
    expect(model.birthDate?.getFullYear()).toBe(2018);
    expect(rule).toHaveBeenLastCalledWith(model.birthDate);
  });
});
