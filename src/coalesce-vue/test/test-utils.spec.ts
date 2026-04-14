import { mockEndpoint } from "../src/test-utils";
import { PersonApiClient } from "@test-targets/api-clients.g";
import { Person } from "@test-targets/metadata.g";
import {
  PersonViewModel,
  PersonListViewModel,
} from "@test-targets/viewmodels.g";

describe("mockEndpoint", () => {
  test("mocks standard method via string spec", async () => {
    const mock = mockEndpoint(
      "/Person/get",
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: {
          personId: 1,
          firstName: "bob",
        },
      })),
    );

    const vm = new PersonViewModel();
    await vm.$load(1);

    expect(vm.firstName).toBe("bob");
    expect(mock).toHaveBeenCalledOnce();
  });

  test("mocks custom method via string spec", async () => {
    const mock = mockEndpoint(
      "/Person/getUser",
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob",
      })),
    );

    const result = await new PersonApiClient().getUser();
    expect(mock).toHaveBeenCalledOnce();
    expect(result.data.object).toBe("bob");
  });

  test("mocks custom method via metadata", async () => {
    const mock = mockEndpoint(
      Person.methods.getUser,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob",
      })),
    );

    const result = await new PersonApiClient().getUser();
    expect(mock).toHaveBeenCalledOnce();
    expect(result.data.object).toBe("bob");
  });

  test("typechecks metadata object", async () => {
    const mock = mockEndpoint(Person.methods.getUser, (config) => ({
      wasSuccessful: true,
      //@ts-expect-error: `object` is expected to be a string.
      object: 3,
    }));
  });

  test("typechecks list", async () => {
    mockEndpoint(
      "/Person/list",
      //@ts-expect-error: `list` is expected to be an array.
      vitest.fn((config) => ({
        wasSuccessful: true,
        list: "",
      })),
    );
  });

  test("types for un-typeable metadata return type are usable", async () => {
    mockEndpoint(
      Person.methods.rename,
      vitest.fn((config) => ({
        wasSuccessful: true,
        // The metadata layer can't pull types down from the model layer,
        // so this test is just asserting that the types aren't obnoxious and
        // don't *prevent* us from doing things.
        object: {
          whatever: "anything",
        },
      })),
    );
  });

  test("mocks failure", async () => {
    const mock = mockEndpoint(
      "/Person/list",
      vitest.fn(async (config) => ({
        wasSuccessful: false,
        message: "something went wrong",
      })),
    );

    const vm = new PersonListViewModel();
    try {
      await vm.$load();
    } catch (e: any) {
      expect(e.message).toBe("Request failed with status code 400");
    }

    expect(vm.$load.wasSuccessful).toBe(false);
    expect(vm.$load.message).toBe("something went wrong");
    expect(mock).toHaveBeenCalledOnce();
  });

  test("fills in list properties", async () => {
    const mock = mockEndpoint(
      "/Person/list",
      vitest.fn(async (config) => ({
        wasSuccessful: true,
        list: [
          { personId: 1, firstName: "bob" },
          { personId: 2, firstName: "sue" },
        ],
      })),
    );

    const vm = new PersonListViewModel();
    await vm.$load();

    expect(vm.$load.page).toBe(1);
    expect(vm.$load.pageSize).toBe(10);
    expect(vm.$load.pageCount).toBe(1);
    expect(vm.$load.totalCount).toBe(2);
  });

  test("destroyable", async () => {
    const client = new PersonApiClient();
    const mock1 = mockEndpoint(
      Person.methods.getUser,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob",
      })),
    );

    const mock2 = mockEndpoint(
      Person.methods.getUser,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "steve",
      })),
    );

    let result = await client.getUser();
    expect(result.data.object).toBe("steve");
    mock2.destroy();

    result = await client.getUser();
    expect(result.data.object).toBe("bob");
    mock1.destroy();

    expect(mock2).toHaveBeenCalledOnce();
    expect(mock1).toHaveBeenCalledOnce();
  });
});
