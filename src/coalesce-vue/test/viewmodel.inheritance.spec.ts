import { mockEndpoint } from "./test-utils";

import {
  PersonViewModel,
  AbstractImpl1ViewModel,
  AbstractModelPersonViewModel,
  AbstractModelViewModel,
} from "../../test-targets/viewmodels.g";
import { AbstractImpl1 } from "@test-targets/models.g";
import { mapToDto } from "../src/model";
import * as $metadata from "@test-targets/metadata.g";
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
      })),
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

  test("preserves derived type of a polymorphic object nested in a request body", async () => {
    // When a polymorphic object is carried as a child of a request payload (e.g.
    // a save/bulkSave), the payload is mapped to a DTO once by the caller and then
    // run through mapToDto a *second* time by getRequestBody. That second pass sees
    // a plain POJO whose type is described by a `$type` string (not `$metadata`),
    // and must not downgrade it to the base type or drop derived-only props.
    const echoEndpoint = mockEndpoint(
      "/AbstractImpl1/echoAbstractModel",
      vitest.fn((req) => ({ wasSuccessful: true, object: null })),
    );

    // Mimic the pre-mapped child object that `mapToDtoFiltered`/`mapToDto` produces
    // for a nested polymorphic value before it reaches the request pipeline.
    const dtoChild = mapToDto(
      new AbstractImpl1({ id: 1, impl1OnlyField: "hello" }),
      {
        name: "model",
        displayName: "Model",
        type: "model",
        role: "value",
        typeDef: $metadata.AbstractModel,
      } as any,
    );

    await new AbstractImpl1ApiClient().echoAbstractModel(dtoChild as any);

    expect(JSON.parse(echoEndpoint.mock.calls[0][0].data)).toMatchObject({
      model: {
        $type: "AbstractImpl1",
        id: 1,
        impl1OnlyField: "hello",
      },
    });

    echoEndpoint.destroy();
  });

  test("$save of a derived type sends its discriminator and derived props", async () => {
    // The save pipeline serializes the model to a DTO once (mapToDtoFiltered) and
    // must not serialize it a second time in getRequestBody, which would downgrade
    // a polymorphic model to its base type and drop derived-only props.
    const saveEndpoint = mockEndpoint(
      "/AbstractImpl1/save",
      vitest.fn((req) => ({ wasSuccessful: true, object: null })),
    );

    const vm = new AbstractImpl1ViewModel({ id: 1, impl1OnlyField: "hello" });
    vm.$setPropDirty("impl1OnlyField");
    await vm.$save();

    expect(JSON.parse(saveEndpoint.mock.calls[0][0].data)).toMatchObject({
      $type: "AbstractImpl1",
      id: 1,
      impl1OnlyField: "hello",
    });

    saveEndpoint.destroy();
  });
});

describe("abstract loader", () => {
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
      })),
    );
  }

  test("load produces concrete ViewModel in result", async () => {
    mockGet();

    const vm = await AbstractModelViewModel.load(1);
    assertIsImpl1(vm!);
  });

  test("load caller has standard reactive state", async () => {
    mockGet();

    const loader = AbstractModelViewModel.load(1);
    expect(loader.isLoading).toBe(true);
    expect(loader.wasSuccessful).toBeNull();
    expect(loader.result).toBeNull();

    await loader.getPromise();

    expect(loader.isLoading).toBe(false);
    expect(loader.wasSuccessful).toBe(true);
    expect(loader.result).not.toBeNull();
    assertIsImpl1(loader.result!);
  });

  test("load result is reactive", async () => {
    mockGet();

    const loader = AbstractModelViewModel.load(1);

    const dummyRef = ref(1);
    const isLoaded = computed(() => dummyRef.value && loader.result != null);
    expect(isLoaded.value).toBe(false);

    await loader.getPromise();
    expect(isLoaded.value).toBe(true);
  });

  function assertIsImpl1(vm: AbstractModelViewModel) {
    expect(vm.id).toBe(1);
    expect(vm.discriminator).toBe("discrim1");
    expect(vm).toBeInstanceOf(AbstractImpl1);
    expect(vm).toBeInstanceOf(AbstractImpl1ViewModel);
    expect(vm.constructor.name).toBe("AbstractImpl1ViewModel");

    expect(vm.$metadata).toStrictEqual(new AbstractImpl1().$metadata);
    expect(vm.$apiClient).toBeInstanceOf(AbstractImpl1ApiClient);
    expect(vm.$apiClient.$metadata).toStrictEqual(
      new AbstractImpl1().$metadata,
    );

    // $data.$metadata must also reflect the concrete type
    expect((vm as any).$data.$metadata).toStrictEqual(
      new AbstractImpl1().$metadata,
    );

    const impl1 = vm as AbstractImpl1ViewModel;
    expect(impl1.impl1OnlyField).toBe("foo");

    // Data was loaded clean (not dirty)
    expect(vm.$getPropDirty("id")).toBe(false);
  }
});
