import { CAdminMethod } from "..";
import $metadata from "@test-targets/metadata.g";
import {
  PersonViewModel,
  PersonListViewModel,
  WeatherServiceViewModel,
} from "@test-targets/viewmodels.g";
import { defineComponent, h, nextTick } from "vue";
import { mockEndpoint, mountWithCoalesceOptions } from "@test/util";

describe("CAdminMethod", () => {
  mockEndpoint("/Person/rename", () => ({
    wasSuccessful: true,
    object: { personId: 1, firstName: "A", lastName: "B" },
  }));

  test("types", () => {
    const vm = new PersonViewModel();
    const listVm = new PersonListViewModel();
    const serviceVm = new WeatherServiceViewModel();

    const rename = vm.$metadata.methods.rename;
    const getUser = listVm.$metadata.methods.getUser;
    const getWeather = serviceVm.$metadata.methods.getWeather;

    () => <CAdminMethod model={vm} for="rename" />;
    () => <CAdminMethod model={vm} for={rename} />;
    //@ts-expect-error method doesn't exist
    () => <CAdminMethod model={vm} for="invalid" />;
    //@ts-expect-error method from wrong type
    () => <CAdminMethod model={vm} for={getWeather} />;

    () => <CAdminMethod model={listVm} for="getUser" />;
    () => <CAdminMethod model={listVm} for={getUser} />;
    //@ts-expect-error method doesn't exist
    () => <CAdminMethod model={listVm} for="invalid" />;
    //@ts-expect-error method from wrong type
    () => <CAdminMethod model={listVm} for={getWeather} />;

    () => <CAdminMethod model={serviceVm} for="getWeather" />;
    () => <CAdminMethod model={serviceVm} for={getWeather} />;
    //@ts-expect-error method doesn't exist
    () => <CAdminMethod model={serviceVm} for="invalid" />;
    //@ts-expect-error method from wrong type
    () => <CAdminMethod model={serviceVm} for={rename} />;
  });

  test("renders registered admin input/display overrides for params and return", async () => {
    const vm = new PersonViewModel();

    const CustomInput = defineComponent({
      name: "CustomMethodInput",
      setup(_, { slots }) {
        return () =>
          h("div", { class: "custom-method-input" }, slots.default?.());
      },
    });

    const CustomDisplay = defineComponent({
      name: "CustomMethodDisplay",
      setup() {
        return () => h("div", { class: "custom-method-display" }, "custom");
      },
    });

    const wrapper = mountWithCoalesceOptions(
      () => <CAdminMethod model={vm} for="rename" />,
      undefined,
      {
        adminOverrides: [
          [
            $metadata.types.Person.methods.rename.params.name,
            { input: CustomInput },
          ],
          [
            $metadata.types.Person.methods.rename.return,
            { display: CustomDisplay },
          ],
        ],
      },
    );

    expect(wrapper.find(".custom-method-input").exists()).toBeTruthy();
    vm.rename.args.name = "A";
    await vm.rename.invokeWithArgs();
    await nextTick();
    expect(wrapper.find(".custom-method-display").exists()).toBeTruthy();
  });
});
