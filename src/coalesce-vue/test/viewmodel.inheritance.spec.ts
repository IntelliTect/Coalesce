import { delay, mockEndpoint } from "./test-utils";

import {
  PersonViewModel,
  AbstractImpl1ViewModel,
  AbstractModelPersonViewModel,
  AbstractModelViewModel,
} from "../../test-targets/viewmodels.g";
import { AbstractImpl1 } from "@test-targets/models.g";
import { ViewModelFactory } from "../src";
import { AbstractImpl1ApiClient } from "@test-targets/api-clients.g";
import { computed, ref } from "vue";

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

describe("abstract proxy", () => {
  const expectedData = {
    id: 1,
    discriminator: "discrim1",
    impl1OnlyField: "foo",
  };

  function mockGet() {
    mockEndpoint(
      "/AbstractModel/get/1",
      vitest.fn((req) => ({
        wasSuccessful: true,
        object: {
          $type: "AbstractImpl1",
          ...expectedData,
        },
      }))
    );
  }

  test("class name", async () => {
    const vm = new AbstractModelViewModel();
    expect(vm.constructor.name).toBe("AbstractModelViewModelProxy");
  });

  test("becomes concrete type when loaded with initial data", async () => {
    const vm = new AbstractModelViewModel(new AbstractImpl1(expectedData));

    expect(vm.$getPropDirty("id")).toBeTruthy();
    assertIsImpl1(vm);
  });

  test("can load when ID is provided by initial data", async () => {
    mockGet();

    const vm = new AbstractModelViewModel({ id: 1 });
    expect(vm.id).toBe(1);
    await vm.$load();

    assertIsImpl1(vm);
    assertLoaded(vm);
  });

  test("becomes concrete type after $load", async () => {
    mockGet();

    const vm = new AbstractModelViewModel();
    await vm.$load(1);

    assertIsImpl1(vm);
    assertLoaded(vm);
  });

  test("instanceof is reactive", async () => {
    mockGet();

    const vm = new AbstractModelViewModel();

    // Dummy ref because Vue will always recompute a computed on every access if the computed has no deps.
    const dummyRef = ref(1);

    const isImpl1 = computed(
      () => dummyRef.value && vm instanceof AbstractImpl1ViewModel
    );
    expect(isImpl1.value).toBeFalsy();

    await vm.$load(1);
    expect(isImpl1.value).toBeTruthy();
  });

  test("$metadata is reactive", async () => {
    mockGet();

    const vm = new AbstractModelViewModel();

    // Dummy ref because Vue will always recompute a computed on every access if the computed has no deps.
    const dummyRef = ref(1);

    const isImpl1 = computed(
      () => dummyRef.value && vm.$metadata.name == "AbstractImpl1"
    );
    expect(isImpl1.value).toBeFalsy();

    await vm.$load(1);
    expect(isImpl1.value).toBeTruthy();
  });

  function assertLoaded(vm: AbstractModelViewModel) {
    expect(vm.$load.wasSuccessful).toBe(true);
    expect(vm.$load.isLoading).toBe(false);
    expect(vm.$load.result).toBeInstanceOf(AbstractImpl1);
    expect(vm.$load.message).toBeFalsy();
    expect(vm.$load.hasResult).toBeTruthy();
  }

  function assertIsImpl1(vm: AbstractModelViewModel) {
    expect(vm.id).toBe(1);
    expect(vm.discriminator).toBe("discrim1");
    expect(vm).toBeInstanceOf(AbstractImpl1);
    expect(vm).toBeInstanceOf(AbstractImpl1ViewModel);
    expect(vm.constructor.name).toBe("AbstractImpl1ViewModel");

    expect(vm.$metadata).toStrictEqual(new AbstractImpl1().$metadata);
    expect(vm.$apiClient).toBeInstanceOf(AbstractImpl1ApiClient);
    expect(vm.$apiClient.$metadata).toStrictEqual(
      new AbstractImpl1().$metadata
    );

    const impl1 = vm as AbstractImpl1ViewModel;
    expect(impl1.impl1OnlyField).toBe("foo");
  }
});
