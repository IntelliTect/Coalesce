import { CAdminMethods } from "..";
import {
  PersonViewModel,
  PersonListViewModel,
  WeatherServiceViewModel,
} from "@test-targets/viewmodels.g";

describe("CAdminMethods", () => {
  test("types", () => {
    const vm = new PersonViewModel();
    const listVm = new PersonListViewModel();
    const serviceVm = new WeatherServiceViewModel();

    const rename = vm.$metadata.methods.rename;

    () => <CAdminMethods model={vm} />;
    () => <CAdminMethods model={listVm} />;
    () => <CAdminMethods model={serviceVm} />;

    // @ts-expect-error not a model
    () => <CAdminMethods model={rename} />;
  });
});
