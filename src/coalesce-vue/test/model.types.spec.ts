import { AbstractImpl1, type AbstractModel } from "@test-targets/models.g";
import {
  AbstractImpl1ViewModel,
  AbstractModelPersonViewModel,
  type AbstractModelViewModel,
} from "@test-targets/viewmodels.g";

describe("types", () => {
  test("derived is assignable to base", () => {
    const model: AbstractModel = new AbstractImpl1();
  });

  test("derived is assignable to base ViewModel prop", () => {
    const vm = new AbstractModelPersonViewModel();
    vm.abstractModel = new AbstractImpl1();
  });

  test("derived vm is assignable to base ViewModel", () => {
    const vm: AbstractModelViewModel = new AbstractImpl1ViewModel();
  });
});
