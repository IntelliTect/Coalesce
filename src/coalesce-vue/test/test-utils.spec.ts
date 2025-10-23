import { mockEndpoint } from "../src/test-utils";
import { StudentApiClient } from "./targets.apiclients";
import { Student } from "./targets.metadata";
import { StudentListViewModel, StudentViewModel } from "./targets.viewmodels";

describe("mockEndpoint", () => {
  test("mocks standard method via string spec", async () => {
    const mock = mockEndpoint(
      "/Students/get",
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: {
          studentId: 1,
          name: "bob",
        },
      })),
    );

    const vm = new StudentViewModel();
    await vm.$load(1);

    expect(vm.name).toBe("bob");
    expect(mock).toHaveBeenCalledOnce();
  });

  test("mocks custom method via string spec", async () => {
    const mock = mockEndpoint(
      "/Students/fullNameAndAge",
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob, 42",
      })),
    );

    const result = await new StudentApiClient().fullNameAndAge(1);
    expect(mock).toHaveBeenCalledOnce();
    expect(result.data.object).toBe("bob, 42");
  });

  test("mocks custom method via metadata", async () => {
    const mock = mockEndpoint(
      Student.methods.fullNameAndAge,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob, 23",
      })),
    );

    const result = await new StudentApiClient().fullNameAndAge(1);
    expect(mock).toHaveBeenCalledOnce();
    expect(result.data.object).toBe("bob, 23");
  });

  test("typechecks metadata object", async () => {
    const mock = mockEndpoint(Student.methods.fullNameAndAge, (config) => ({
      wasSuccessful: true,
      //@ts-expect-error: `object` is expected to be a string.
      object: 3,
    }));
  });

  test("typechecks list", async () => {
    mockEndpoint(
      "/Students/list",
      //@ts-expect-error: `list` is expected to be an array.
      vitest.fn((config) => ({
        wasSuccessful: true,
        list: "",
      })),
    );
  });

  test("types for un-typeable metadata return type are usable", async () => {
    mockEndpoint(
      Student.methods.getWithObjParam,
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
      "/Students/list",
      vitest.fn(async (config) => ({
        wasSuccessful: false,
        message: "something went wrong",
      })),
    );

    const vm = new StudentListViewModel();
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
      "/Students/list",
      vitest.fn(async (config) => ({
        wasSuccessful: true,
        list: [
          { studentId: 1, name: "bob" },
          { studentId: 2, name: "sue" },
        ],
      })),
    );

    const vm = new StudentListViewModel();
    await vm.$load();

    expect(vm.$load.page).toBe(1);
    expect(vm.$load.pageSize).toBe(10);
    expect(vm.$load.pageCount).toBe(1);
    expect(vm.$load.totalCount).toBe(2);
  });

  test("destroyable", async () => {
    const client = new StudentApiClient();
    const mock1 = mockEndpoint(
      Student.methods.fullNameAndAge,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "bob",
      })),
    );

    const mock2 = mockEndpoint(
      Student.methods.fullNameAndAge,
      vitest.fn((config) => ({
        wasSuccessful: true,
        object: "steve",
      })),
    );

    let result = await client.fullNameAndAge(1);
    expect(result.data.object).toBe("steve");
    mock2.destroy();

    result = await client.fullNameAndAge(1);
    expect(result.data.object).toBe("bob");
    mock1.destroy();

    expect(mock2).toHaveBeenCalledOnce();
    expect(mock1).toHaveBeenCalledOnce();
  });
});
