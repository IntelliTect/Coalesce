import { CAdminMethod } from "..";
import {
  PersonViewModel,
  PersonListViewModel,
  WeatherServiceViewModel,
} from "@test-targets/viewmodels.g";

describe("CAdminMethod", () => {
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
});
