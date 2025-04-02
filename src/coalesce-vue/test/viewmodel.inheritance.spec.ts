import { mockEndpoint } from "./test-utils";

import {
  PersonViewModel,
  AbstractImpl1ViewModel,
  AbstractModelPersonViewModel,
} from "../../test-targets/viewmodels.g";
import { AbstractImpl1 } from "@test-targets/models.g";
import { ViewModelFactory } from "../src";

describe("ViewModel", () => {
  test("$bulkSave creation of TPH entity with many-to-many that references base type", async () => {
    const bulkSaveEndpoint = mockEndpoint(
      "/AbstractImpl1/bulkSave",
      vitest.fn((req) => ({
        wasSuccessful: true,
        object: null,
      }))
    );

    // This tests the fact that the metadata instance of
    // `AbstractImpl1ViewModel.abstractModelPeople.$metadata` is the one owned by `AbstractImpl1`'s metadata,
    // but the metadata instance of `AbstractModelPerson.$metadata.props.abstractModel.inverseNavigation`
    // is the copy of the same collection that is owned by `AbstractModel`'s metadata.
    //
    // The collection navigation fixup logic in bulk saves has to allow for this type to be different.

    const person = new PersonViewModel();
    person.$loadCleanData({ personId: 1, name: "bob" });
    const vm = new AbstractImpl1ViewModel({ impl1OnlyField: "fieldval" });
    vm.abstractModelPeople.push({ person });
    await vm.$bulkSave();

    expect(JSON.parse(bulkSaveEndpoint.mock.calls[0][0].data)).toMatchObject({
      items: [
        {
          action: "save",
          type: "AbstractImpl1",
          data: { id: null, impl1OnlyField: "fieldval" },
          refs: { id: vm.$stableId },
          root: true,
        },
        {
          action: "save",
          type: "AbstractModelPerson",
          data: { id: null },
          refs: {
            id: vm.abstractModelPeople[0].$stableId,
            abstractModelId: vm.$stableId,
          },
        },
      ],
    });

    bulkSaveEndpoint.destroy();
  });

  test("ViewModelFactory uses real type when base type is requested", () => {
    const impl = new AbstractImpl1({ impl1OnlyField: "foo" });

    // Request base type but provide instance of derived type as data.
    const vm = ViewModelFactory.get("AbstractModel", impl, true);
    expect(vm).toBeInstanceOf(AbstractImpl1ViewModel);
  });

  test("ViewModel model prop setter allows derived type", () => {
    const vm = new AbstractModelPersonViewModel();
    const impl = new AbstractImpl1({ impl1OnlyField: "foo", id: 42 });

    vm.abstractModel = impl;

    expect(vm.abstractModel).toBeInstanceOf(AbstractImpl1ViewModel);
    expect(vm.abstractModelId).toBe(42);
  });
});
