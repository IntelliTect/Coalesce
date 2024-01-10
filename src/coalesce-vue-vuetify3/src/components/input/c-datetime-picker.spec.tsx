import { Grade } from "@test/targets.models";
import { StudentViewModel } from "@test/targets.viewmodels";
import { delay, mount } from "@test/util";
import { CDatetimePicker } from "..";
import { Case, ComplexModel } from "@test/models.g";
import { ComplexModelViewModel } from "@test/viewmodels.g";
import { AnyArgCaller } from "coalesce-vue";

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

  test("types", () => {
    const model = new ComplexModel();
    const vm = new ComplexModelViewModel();
    const ds = new Case.DataSources.AllOpenCases();

    const selectedDate = new Date();

    () => <CDatetimePicker model={vm} for="systemDateOnly" />;
    () => <CDatetimePicker model={model} for="systemDateOnly" />;
    () => (
      <CDatetimePicker
        model={model}
        for={vm.$metadata.props.dateTimeNullable}
      />
    );
    () => <CDatetimePicker model={model as any} for="systemDateOnly" />;

    //@ts-expect-error non-existent prop
    () => <CDatetimePicker model={vm} for="asdf" />;
    //@ts-expect-error non-date prop
    () => <CDatetimePicker model={vm} for="long" />;

    () => (
      <CDatetimePicker for="ComplexModel.dateTime" modelValue={selectedDate} />
    );
    () => (
      <CDatetimePicker
        for={vm.$metadata.props.dateTimeNullable}
        modelValue={selectedDate}
      />
    );

    () => <CDatetimePicker modelValue={selectedDate} />;
    //@ts-expect-error wrong value type
    () => <CDatetimePicker modelValue={selectedDate as string} />;

    const caller = vm.methodWithManyParams;
    () => <CDatetimePicker model={caller} for="dateTime" />;
    () => <CDatetimePicker model={caller as AnyArgCaller} for="dateTime" />;
    () => <CDatetimePicker model={caller as AnyArgCaller} for="anyString" />;

    //@ts-expect-error non-existent param
    () => <CDatetimePicker model={caller} for="asdf" />;
    //@ts-expect-error non-date param
    () => <CDatetimePicker model={caller} for="integer" />;

    () => <CDatetimePicker model={ds} for="minDate" />;
    //@ts-expect-error invalid param
    () => <CDatetimePicker model={ds} for="asdf" />;
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CDatetimePicker model={model.manyParams} for="startDate" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Start Date");

    // Set a value, and look for the value
    model.manyParams.args.startDate = new Date("2023-08-16T01:02:03Z");
    await delay(1);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(model.manyParams.args.startDate.getFullYear()).toBe(2017);
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
