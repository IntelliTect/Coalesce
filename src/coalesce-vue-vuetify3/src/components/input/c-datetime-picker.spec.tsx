import {
  delay,
  mount,
  mountApp,
  openMenu,
} from "@test/util";
import { CDatetimePicker } from "..";
import { Case, ComplexModel } from "@test-targets/models.g";
import { ComplexModelViewModel } from "@test-targets/viewmodels.g";
import { AnyArgCaller } from "coalesce-vue";
import { VForm } from "vuetify/components";

describe("CDatetimePicker", () => {
  let model: ComplexModelViewModel;
  beforeEach(() => {
    model = new ComplexModelViewModel({
      name: "bob"
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

    // Against models that might be null
    () => (
      <CDatetimePicker model={vm.referenceNavigation} for="systemDateOnly" />
    );

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

  test("disabled inherits from form", async () => {
    const wrapper = mount(() => (
      <VForm disabled>
        <CDatetimePicker />
      </VForm>
    ));

    expect(wrapper.find("input").element.disabled).toBeTruthy();
  });

  test("readonly inherits from form", async () => {
    const wrapper = mount(() => (
      <VForm readonly>
        <CDatetimePicker />
      </VForm>
    ));

    expect(wrapper.find("input").element.readOnly).toBeTruthy();
  });

  test("opens picker menu", async () => {
    const date = new Date(18478289085);
    const wrapper = mountApp(() => (
      <CDatetimePicker modelValue={date} timeZone="America/Los_Angeles" />
    )).findComponent(CDatetimePicker);

    const overlay = await openMenu(wrapper);

    expect(overlay.findAll(".c-time-picker__item-active")).toHaveLength(3);
    expect(overlay.text()).contains("August 1970");
    expect(overlay.text()).contains("Sun, Aug 2");
    console.log(overlay.text());
    expect(overlay.find(".c-time-picker-header").text()).equals("1:51 PM PDT");
  });

  test("caller model - date value", async () => {
    const wrapper = mount(() => (
      <CDatetimePicker model={model.methodWithManyParams} for="dateTime" />
    ));

    // Assert resting state
    expect(wrapper.find("label").text()).toEqual("Date Time");

    // Set a value, and look for the value
    model.methodWithManyParams.args.dateTime = new Date("2023-08-16T01:02:03Z");
    await delay(1);
    expect(wrapper.find("input").element.value).contains("2023");

    // Perform an input on the component, and then look for the new value.
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(model.methodWithManyParams.args.dateTime.getFullYear()).toBe(2017);
  });

  test("validation rules are passed date, not string", async () => {
    const rule = vitest.fn(
      (v) => !v || v.getFullYear() > 2017 || "Year must be > 2017"
    );

    const wrapper = mount(() => (
      <CDatetimePicker model={model} for="systemDateOnly" rules={[rule]} />
    ));

    // Perform an input on the component, and then look at the args that were passed to the rule function:
    await wrapper.find("input").setValue("1/3/2017");
    await delay(1);
    expect(wrapper.text()).toContain("Year must be > 2017");
    expect(model.systemDateOnly?.getFullYear()).toBe(2017);
    expect(rule).toHaveBeenLastCalledWith(model.systemDateOnly);

    // Do it again, but with a valid input this time. The error should be gone.
    await wrapper.find("input").setValue("1/3/2018");
    await delay(1);
    expect(wrapper.text()).not.toContain("Year must be > 2017");
    expect(model.systemDateOnly?.getFullYear()).toBe(2018);
    expect(rule).toHaveBeenLastCalledWith(model.systemDateOnly);
  });
});
