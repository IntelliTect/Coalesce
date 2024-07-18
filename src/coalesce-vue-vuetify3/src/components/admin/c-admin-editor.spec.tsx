import { Person } from "@test-targets/models.g";
import { CAdminEditor } from "..";
import {
  PersonViewModel,
  PersonListViewModel,
} from "@test-targets/viewmodels.g";

describe("CAdminEditor", () => {
  test("types", () => {
    const model = new Person();
    const vm = new PersonViewModel();
    const list = new PersonListViewModel();

    () => <CAdminEditor model={vm} />;
    //@ts-expect-error plain model not allowed
    () => <CAdminEditor model={model} />;
    //@ts-expect-error list not allowed
    () => <CAdminEditor model={list} />;
  });
});
