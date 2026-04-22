import { type ComponentPublicInstance, effectScope } from "vue";
import axios, {
  AxiosError,
  type AxiosAdapter,
  type AxiosResponse,
} from "axios";
import { mount } from "@vue/test-utils";

import { type ItemMethod } from "../src/metadata";
import {
  AxiosClient,
  ListParameters,
  type ItemResult,
  type ItemResultPromise,
  type ListResult,
  type AxiosRequestConfig,
  type AxiosListResult,
  type ListResultPromise,
  mapParamsToDto,
  mapQueryToParams,
  getMessageForError,
  useAppUpdateCheck,
} from "../src/api-client";
import { getInternalInstance } from "../src/util";
import { delay, mountData, mockEndpoint } from "./test-utils";

import {
  ComplexModelApiClient,
  PersonApiClient,
  DateOnlyPkApiClient,
  StringIdentityApiClient,
} from "@test-targets/api-clients.g";
import {
  ComplexModel as ComplexModelMeta,
  Person as PersonMeta,
} from "@test-targets/metadata.g";
import {
  ComplexModelViewModel,
  PersonListViewModel,
} from "@test-targets/viewmodels.g";
import {
  ComplexModel,
  ExternalParent,
  Genders,
  Person,
  PersonCriteria,
  Statuses,
} from "@test-targets/models.g";

function makeAdapterMock(result?: any) {
  return makeEndpointMock<AxiosRequestConfig>(result);
}

function makeEndpointMock<TParam = number | undefined | null>(result?: any) {
  return vitest.fn((arg?: TParam) => {
    return Promise.resolve({
      data: <ItemResult>{ wasSuccessful: true, object: result ?? arg },
      status: 200,
      statusText: "OK",
      headers: {},
      config: {},
    }) as ItemResultPromise<number>;
  });
}

describe("error handling", () => {
  test("throws error when server returns raw string instead of object", async () => {
    AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: "<!doctype html><html><body></body></html>",
      status: 200,
      statusText: "OK",
      config: {} as any,
      headers: { "Content-Type": "text/html" },
    });

    await expect(new PersonApiClient().get(1)).rejects.toThrow(
      "Unexpected text/html string response from server.",
    );
  });
});

describe("getMessageForError", () => {
  test("returns string error as-is", () => {
    expect(getMessageForError("Custom error message")).toBe(
      "Custom error message",
    );
  });

  test("returns standard Error message", () => {
    expect(getMessageForError(new Error("Error occurred"))).toBe(
      "Error occurred",
    );
  });

  test("returns message from standard Coalesce API response", () => {
    const axiosError = new AxiosError("Request failed");
    axiosError.response = {
      data: { wasSuccessful: false, message: "Validation failed" },
      status: 400,
      statusText: "Bad Request",
      headers: {},
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe("Validation failed");
  });

  test("returns 'Unknown Error' when Coalesce response has empty message", () => {
    const axiosError = new AxiosError("Request failed");
    axiosError.response = {
      data: { wasSuccessful: false, message: "" },
      status: 400,
      statusText: "Bad Request",
      headers: {},
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe("Unknown Error");
  });

  test("returns detail from application/problem+json response", () => {
    const axiosError = new AxiosError("Request failed");
    axiosError.response = {
      data: {
        type: "https://example.com/probs/out-of-credit",
        title: "You do not have enough credit.",
        detail: "Your current balance is 30, but that costs 50.",
        status: 403,
      },
      status: 403,
      statusText: "Forbidden",
      headers: { "content-type": "application/problem+json; charset=utf-8" },
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe(
      "Your current balance is 30, but that costs 50.",
    );
  });

  test("returns title from application/problem+json when detail is missing", () => {
    const axiosError = new AxiosError("Request failed");
    axiosError.response = {
      data: {
        type: "https://example.com/probs/forbidden",
        title: "Access denied",
        status: 403,
      },
      status: 403,
      statusText: "Forbidden",
      headers: { "content-type": "application/problem+json" },
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe("Access denied");
  });

  test("returns axios message with status text appended", () => {
    const axiosError = new AxiosError("Request failed with status code 403");
    axiosError.response = {
      data: {},
      status: 403,
      statusText: "Forbidden",
      headers: {},
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe(
      "Request failed with status code 403 (Forbidden)",
    );
  });

  test("does not append status text if already in message", () => {
    const axiosError = new AxiosError("Request failed: Forbidden");
    axiosError.response = {
      data: {},
      status: 403,
      statusText: "Forbidden",
      headers: {},
      config: {} as any,
    };

    expect(getMessageForError(axiosError)).toBe("Request failed: Forbidden");
  });

  test("returns generic message for axios error without response", () => {
    const axiosError = new AxiosError("Network Error");
    // No response set
    expect(getMessageForError(axiosError)).toBe("Network Error");
  });

  test("returns 'An unknown error occurred' for null", () => {
    expect(getMessageForError(null)).toBe("An unknown error occurred");
  });

  test("returns 'An unknown error occurred' for undefined", () => {
    expect(getMessageForError(undefined)).toBe("An unknown error occurred");
  });
});

describe("$useSimultaneousRequestCaching", () => {
  test("uses proper cache key for standard method", async () => {
    const mock = (AxiosClient.defaults.adapter = vitest
      .fn()
      .mockImplementation(async () => {
        // Delay so the calls don't complete instantly (which would subvert request caching).
        await delay(30);
        return <AxiosListResult<Person>>{
          data: {
            wasSuccessful: true,
            list: [] as Person[],
            page: 1,
            pageCount: 0,
            pageSize: 10,
          },
          status: 200,
        };
      }));

    const client = new PersonApiClient().$useSimultaneousRequestCaching();

    const invoker = (status: Statuses) => {
      const params = new ListParameters();
      const ds = (params.dataSource =
        new Person.DataSources.NamesStartingWithAWithCases());
      ds.allowedStatuses = [status];
      params.fields = ["firstName", "birthDate"];
      return client.list(params);
    };

    // Act
    await Promise.all([
      invoker(Statuses.Open),
      invoker(Statuses.Open),
      invoker(Statuses.InProgress),
      invoker(Statuses.InProgress),
      invoker(Statuses.Open),
    ]);

    // Assert
    expect(mock).toBeCalledTimes(2); // 2 distinct sets of parameters => 2 calls
    const actualParams = mock.mock.calls[0] as Parameters<AxiosAdapter>;
    expect(actualParams[0].params["dataSource"]).toBe(
      "NamesStartingWithAWithCases",
    );
    expect(actualParams[0].params["dataSource.allowedStatuses"]).toStrictEqual([
      0,
    ]);
    expect(actualParams[0].params["fields"]).toBe("firstName,birthDate");
    expect(actualParams[0].params["page"]).toBe("1");
    expect(actualParams[0].params["pageSize"]).toBe("10");
  });

  test("uses proper cache key for custom method", async () => {
    const mock = (AxiosClient.defaults.adapter = vitest
      .fn()
      .mockImplementation(async () => {
        // Delay so the calls don't complete instantly (which would subvert request caching).
        await delay(30);
        return <AxiosResponse<any>>{
          data: { wasSuccessful: true, object: {} },
          status: 200,
        };
      }));

    const client = new ComplexModelApiClient().$useSimultaneousRequestCaching();

    const invoker = (id: number) =>
      client.$invoke(ComplexModelMeta.methods.instanceGetMethodWithObjParam, {
        id,
        obj: new ExternalParent({ stringList: ["hello"] }),
      });

    // Act
    await Promise.all([invoker(3), invoker(3), invoker(4), invoker(4)]);

    // Assert
    expect(mock).toBeCalledTimes(2); // 2 distinct sets of parameters => 2 calls
    const actualParams = mock.mock.calls[0] as Parameters<AxiosAdapter>;
    expect(actualParams[0].params).toMatchObject({
      id: 3,
      obj: {
        stringList: ["hello"],
      },
    });
  });

  test("functions when enabled via ApiState", async () => {
    const invoker = new PersonApiClient()
      .$makeCaller("item", (c, letter: string) => c.namesStartingWith(letter))
      .useSimultaneousRequestCaching()
      .setConcurrency("allow");

    const mock = mockEndpoint(
      "/Person/namesStartingWith",
      vitest.fn(async (req) => {
        await delay(30);
        return { wasSuccessful: true, object: [] };
      }),
    );

    // Act
    await Promise.all([invoker("a"), invoker("a"), invoker("b"), invoker("b")]);

    // Assert
    expect(mock).toBeCalledTimes(2); // 2 distinct sets of parameters => 2 calls
    expect(mock.mock.calls[0][0].params).toMatchObject({ characters: "a" });
    expect(mock.mock.calls[1][0].params).toMatchObject({ characters: "b" });
  });
});

describe("$invoke", () => {
  test("doesn't error when params are missing", async () => {
    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "" },
      status: 200,
    }));

    // The use case here is to allow optional params to be missing.
    // Technically the one we're omitting ('id') would be a required param,
    // but the client code doesn't make that distinction - the server does,
    // and we're mocking the server.
    await expect(
      new PersonApiClient().$invoke(PersonMeta.methods.rename, {} as any),
    ).resolves.toBeTruthy();

    expect(mock.mock.calls[0][0]).toMatchObject({ params: {} });
  });

  test("does not send omitted optional parameters", async () => {
    const mock = mockEndpoint(
      "/ComplexModel/methodWithOptionalParams",
      vitest.fn((req) => ({
        wasSuccessful: true,
      })),
    );

    await new ComplexModelApiClient().methodWithOptionalParams(1, 42);

    // The request payload should have only included the parameters we actually provided.
    // The others should have been omitted entirely.
    const req: AxiosRequestConfig = mock.mock.lastCall?.[0];
    expect(req.data).toEqual('{"id":1,"requiredInt":42}');
  });

  test("does not send null value top level params", async () => {
    // Much like how null values on entity models end up mapping to their default value
    // when the DTO maps to the entity, we should similarly treat nulls in the method args
    // object as "not set" (effectively undefined, but using `null` instead since that's
    // how that has worked in Coalesce since Vue 2).

    const mock = mockEndpoint(
      "/ComplexModel/methodWithManyParams",
      vitest.fn((req) => ({
        wasSuccessful: true,
      })),
    );

    const caller = new ComplexModelViewModel().methodWithManyParams;
    await caller.invokeWithArgs();

    const req: AxiosRequestConfig = mock.mock.lastCall?.[0];
    expect(req.data).toEqual("{}");
  });

  test("doesn't error when unexpected params are provided", async () => {
    // The use case here is to allow, for e.g., a component that always provides a "search"
    // param to an API to still function even if that API doesn't use or care about a "search" param.
    // This might seem like a dumb case to test, but it was actually broken because we were iterating
    // over the actual provided params when mapping the params, instead of over the method's metadata.
    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "Bob 42" },
      status: 200,
    }));

    const res = await new PersonApiClient().$invoke(
      PersonMeta.methods.fullNameAndAge,
      // Our types are actually so good that they will catch this as an error, so we cast to any.
      { id: 1, extraParam: "" } as any,
    );

    expect(res.data.object).toBe("Bob 42");
    expect(mock.mock.calls[0][0]).toMatchObject({ params: { id: 1 } });
  });

  test("passes file in method with only other scalar parameters as FormData", async () => {
    // RATIONALE: File methods with only other scalar parameters can benefit from reduced request size
    // by sending the file in multipart formdata since it requires no size-increasing encoding like base64.

    // Since all other parameters are only simple scalar values

    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "" },
      status: 200,
    }));

    const methodMeta: ItemMethod = {
      name: "test",
      displayName: "",
      httpMethod: "POST",
      return: { displayName: "", name: "$return", type: "void", role: "value" },
      transportType: "item",
      params: {
        id: { type: "number", role: "value", displayName: "", name: "id" },
        file: { type: "file", role: "value", displayName: "", name: "file" },
      },
    };
    const file = new File([new ArrayBuffer(1)], "fileName", {
      type: "application/pdf",
    });

    await new PersonApiClient().$invoke(methodMeta, {
      id: 42,
      file,
    });

    expect(mock).toBeCalledTimes(1);
    const formData = mock.mock.calls[0][0].data as FormData;
    expect(formData).toBeInstanceOf(FormData);
    expect(formData.get("id")).toBe("42");
    expect(formData.get("file")).toBe(file);
  });

  test("passes file in method with other complex parameters as JSON", async () => {
    // RATIONALE: Methods with complex parameters might have a large number of key-value
    // pairs to pass as form data. If we sent the request as multipart formdata,
    // this makes the request VERY large and verbose in order to transport all the nested properties
    // of the complex type parameters.

    // If we instead send these as JSON, the file content takes up a bit more space as base64,
    // but the overall request is simpler, and we also don't need to worry about
    // all the strange idiosyncrasies with sending complex objects as form data.

    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "" },
      status: 200,
    }));

    const methodMeta: ItemMethod = {
      name: "test",
      displayName: "",
      httpMethod: "POST",
      return: { displayName: "", name: "$return", type: "void", role: "value" },
      transportType: "item",
      params: {
        id: { type: "number", role: "value", displayName: "", name: "id" },
        file: { type: "file", role: "value", displayName: "", name: "file" },
        student: {
          type: "model",
          role: "value",
          displayName: "",
          name: "student",
          typeDef: ComplexModelMeta,
        },
      },
    };
    const file = new File([new ArrayBuffer(1)], "fileName", {
      type: "application/pdf",
    });

    await new PersonApiClient().$invoke(methodMeta, {
      id: 42,
      file,
      student: <ComplexModel>{ name: "bob&bob=bob", singleTestId: null },
    });

    expect(mock).toBeCalledTimes(1);
    expect(mock.mock.calls[0][0].data).toBe(
      '{"id":42,"file":{"content":"AA==","contentType":"application/pdf","name":"fileName"},"student":{"singleTestId":null,"name":"bob&bob=bob"}}',
    );
  });

  test("passes file array as FormData", async () => {
    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "" },
      status: 200,
    }));

    const methodMeta: ItemMethod = {
      name: "test",
      displayName: "",
      httpMethod: "POST",
      return: { displayName: "", name: "$return", type: "void", role: "value" },
      transportType: "item",
      params: {
        id: { type: "number", role: "value", displayName: "", name: "id" },
        files: {
          type: "collection",
          role: "value",
          displayName: "",
          name: "file",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "file",
          },
        },
      },
    };

    const file1 = new File([new ArrayBuffer(1)], "fileName1", {
      type: "application/pdf",
    });
    const file2 = new File([new ArrayBuffer(2)], "fileName2", {
      type: "application/pdf",
    });

    await new PersonApiClient().$invoke(methodMeta, {
      id: 42,
      files: [file1, file2],
    });

    expect(mock).toBeCalledTimes(1);
    const formData = mock.mock.calls[0][0].data as FormData;
    expect(formData).toBeInstanceOf(FormData);
    expect(formData.get("id")).toBe("42");
    // Aspnetcore will not bind the files correctly if they're keyed as
    // files[0] and files[1]. They must use the same key (matching the param name).
    // Untested: keying them as `files[]` might also work.
    expect(formData.getAll("files")).toEqual([file1, file2]);
  });

  test("passes Uint8Array as JSON", async () => {
    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: "" },
      status: 200,
    }));

    const methodMeta: ItemMethod = {
      name: "test",
      displayName: "",
      httpMethod: "POST",
      return: { displayName: "", name: "$return", type: "void", role: "value" },
      transportType: "item",
      params: {
        id: { type: "number", role: "value", displayName: "", name: "id" },
        bin: { type: "binary", role: "value", displayName: "", name: "file" },
      },
    };

    const bin = new Uint8Array([0x11, 0x22, 0x33]);
    await new PersonApiClient().$invoke(methodMeta, {
      id: 42,
      bin,
    });

    expect(mock.mock.calls[0][0].data).toBe('{"id":42,"bin":"ESIz"}');
  });

  test("POST passes null correctly", async () => {
    // This makes sure that we correctly send NULL fields the server.
    // When I migrated away from the `qs` lib, I broke this... oops.
    const mock = (AxiosClient.defaults.adapter = vitest.fn().mockResolvedValue(<
      AxiosResponse<any>
    >{
      data: { wasSuccessful: true, object: {} },
      status: 200,
    }));

    await new PersonApiClient().save(
      new Person({
        companyId: null,
        firstName: "bob",
      }),
      { fields: ["companyId", "firstName"] },
    );

    expect(mock.mock.calls[0][0].data).toBe(
      '{"firstName":"bob","companyId":null}',
    );
  });

  test("data source collection parameter", async () => {
    const mock = mockEndpoint(
      "/Person/list",
      vitest.fn((req: AxiosRequestConfig) => {
        return {
          wasSuccessful: true,
          list: [],
        };
      }),
    );

    const personList = new PersonListViewModel();
    personList.$dataSource = new Person.DataSources.NamesStartingWithAWithCases(
      {
        allowedStatuses: [Statuses.Open, Statuses.InProgress],
      },
    );
    await personList.$load();

    expect(AxiosClient.getUri(mock.mock.lastCall![0])).toBe(
      "/api/Person/list?page=1&pageSize=10&dataSource=NamesStartingWithAWithCases&dataSource.allowedStatuses=[0,1]",
    );
  });

  test("data source object parameter", async () => {
    const mock = mockEndpoint(
      "/Person/list",
      vitest.fn((req: AxiosRequestConfig) => {
        return {
          wasSuccessful: true,
          list: [],
        };
      }),
    );

    const personList = new PersonListViewModel();
    personList.$dataSource = new Person.DataSources.ParameterTestsSource({
      personCriterion: new PersonCriteria({
        gender: Genders.Female,
        name: "Grace",
        personIds: [1, 2, 3],
        subCriteria: [
          new PersonCriteria({
            name: "Bob Newbie",
            personIds: [],
          }),
        ],
      }),
    });
    await personList.$load();

    expect(AxiosClient.getUri(mock.mock.lastCall![0])).toBe(
      `/api/Person/list?page=1&pageSize=10&dataSource=ParameterTestsSource&dataSource.personCriterion={"personIds":[1,2,3],"name":"Grace","subCriteria":[{"personIds":[],"name":"Bob Newbie"}],"gender":2}`,
    );
  });

  test.each([
    [true, "&dataSource.hasEmail=true"],
    [false, "&dataSource.hasEmail=false"],
    // The parameter must be fully omitted for aspnetcore to not throw an error in modelbinding
    [null, ""],
    [undefined, ""],
  ] as const)("data source bool parameter &s", async (value, expected) => {
    const mock = mockEndpoint(
      "/Person/list",
      vitest.fn((req: AxiosRequestConfig) => {
        return {
          wasSuccessful: true,
          list: [],
        };
      }),
    );

    const personList = new PersonListViewModel();
    personList.$dataSource = new Person.DataSources.NamesStartingWithAWithCases(
      {
        hasEmail: value,
      },
    );
    await personList.$load();

    expect(AxiosClient.getUri(mock.mock.lastCall![0])).toBe(
      "/api/Person/list?page=1&pageSize=10&dataSource=NamesStartingWithAWithCases" +
        expected,
    );
  });
});

describe("$makeCaller", () => {
  test("passes parameters to invoker func", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      (c, num: number) => {
        return endpointMock(num);
      },
    );

    const arg = 42;
    const result = await caller(arg);
    expect(endpointMock.mock.calls[0][0]).toBe(arg);
    expect(caller.result).toBe(arg);
    expect(result).toBe(arg);
    expect(caller.rawResponse.data.object).toBe(arg);
    expect(caller.rawResponse.status).toBe(200);

    // Typescript typing tests - all of these are valid types of `result`.
    caller.result = null;
    caller.result = 13;
    //@ts-expect-error `undefined` is not a valid value of `result`
    caller.result = undefined;
  });

  test("failed requests re-throw errors", async () => {
    const caller = new PersonApiClient().$makeCaller(
      "item",
      (c, num: number) => {
        throw {
          name: "mock error",
          message: "mocked throw",
        } as AxiosError;
      },
    );

    await expect(caller(42)).rejects.toBeTruthy();
  });

  test("allows return undefined from invoker func", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      (c, num: number) => {
        if (num == 42) {
          return;
        }
        return endpointMock(num);
      },
    );

    const arg = 42;
    const result = await caller(arg);

    // The typings are actually wrong at the moment - `undefined` is not one of the types of `result`, but it should be.
    expect(result).toBeUndefined();

    expect(endpointMock.mock.calls.length).toBe(0);
    expect(caller.result).toBeNull();

    // Typescript typing tests - all of these are valid types of `result`.
    // Note that Typescript intellisense in VS code seems to be really messed up
    // right now and shows that `result` is only `string`.:
    caller.result = null;
    caller.result = 13;

    //@ts-expect-error `undefined` is not a valid value of `result`, even if the invoker doesn't always return.
    caller.result = undefined;
  });

  test("types: allows return undefined from async invoker func", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      async (c, num: number) => {
        if (num == 42) {
          return;
        }
        const res = await endpointMock(num);
        return res;
      },
    );

    const arg = 42;
    const result = await caller(arg);

    // The typings are actually wrong at the moment - `undefined` is not one of the types of `result`, but it should be.
    expect(result).toBeUndefined();

    expect(endpointMock.mock.calls.length).toBe(0);
    expect(caller.result).toBeNull();

    // Typescript typing tests - all of these are valid types of `result`.
    // Note that Typescript intellisense in VS code seems to be really messed up
    // right now and shows that `result` is only `string`.:
    caller.result = null;
    caller.result = 13;

    //@ts-expect-error `undefined` is not a valid value of `result`, even if the invoker doesn't always return.
    caller.result = undefined;
  });

  test("onFulfilled callbacks are awaited when promises returned", async () => {
    const endpointMock = makeEndpointMock();
    let awaited = false;
    const model = {
      caller: new PersonApiClient()
        .$makeCaller("item", function (this: any, c) {
          return endpointMock(42);
        })
        .onFulfilled(async (caller) => {
          expect(caller.isLoading).toBe(true);

          // @ts-expect-error Assert that the parameter is typed properly.
          caller.propThatDoesNotExist?.toString();
          // @ts-expect-error Assert that the parameter is typed properly.
          caller.result?.includes?.("x");
          // Assert that the parameter is typed properly.
          caller.result?.toExponential();

          await delay(50);
          awaited = true;
        }),
    };

    await model.caller();
    expect(awaited).toBeTruthy();
    expect(model.caller.isLoading).toBeFalsy();
  });

  test("onRejected callbacks are awaited when promises returned", async () => {
    let awaited = false;
    const model = {
      caller: new PersonApiClient()
        .$makeCaller("item", function (this: any, c) {
          throw Error();
        })
        .onRejected(async () => {
          await delay(50);
          awaited = true;
        }),
    };

    await expect(model.caller()).rejects.toBeTruthy();
    expect(awaited).toBeTruthy();
    expect(model.caller.isLoading).toBeFalsy();
  });

  test("passes this to invoker func", async () => {
    const endpointMock = makeEndpointMock();
    type Model = { value: number; caller: () => Promise<any> };
    const fulfilledMock = vitest.fn();
    const model = <Model>{
      value: 42,
      caller: new PersonApiClient()
        .$makeCaller("item", function (this: Model, c) {
          return endpointMock(this.value);
        })
        .onFulfilled(fulfilledMock),
    };

    await model.caller();
    expect(endpointMock.mock.calls[0][0]).toBe(model.value);
    expect(fulfilledMock.mock.instances[0]).toBe(model);
    expect(fulfilledMock.mock.calls[0][0]).toBe(model.caller);
  });

  test("preserves getter/setter behavior on ApiState after _makeReactive()", () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller("item", (c, num: number) =>
      endpointMock(num),
    );

    // Precondition: Ensures that our test doesn't accidentally set the property with the same value
    // should the default for concurrencyMode ever become something other than "disallow".
    // This can be anything as long as its different from what we set it to below.
    expect(caller.concurrencyMode).toBe("disallow");

    caller.concurrencyMode = "allow";

    // Check that the getter returns the value we just set.
    // Just a sanity check - this part always worked.
    expect(caller.concurrencyMode).toBe("allow");

    // This is the important test - we need to make sure
    // that the internal variable behind `concurrencyMode` is actually being set.
    // Before this test and the fix that went with it,
    // Vue would completely take over the concurrencyMode property and its getter/setter,
    // completely circumventing our backing private property.
    // This was happening because we were calling Vue's `defineReactive` on a getter/setter property,
    // which isn't something its intended to support.
    // The fix for this issue is the same as the one done for https://github.com/vuejs/vue/issues/3610
    // in https://github.com/vuejs/vue/commit/4c7a87e2ef9c76b5b75d85102662a27165a23ec7
    expect((caller as any)._concurrencyMode).toBe("allow");

    // We should also be able to observe that concurrencyMode is not declared directly on the caller.
    // If we were accidentally clobbering this property, concurrencyModel would be a getter/setter declared on itself
    // (i.e. Object.getOwnPropertyDescriptor(caller, 'concurrencyMode') !== undefined)
    const concurrencyModeProp = Object.getOwnPropertyDescriptor(
      caller,
      "concurrencyMode",
    );
    expect(concurrencyModeProp).toBeUndefined();
  });

  test("concurrencyMode 'debounce' ignores redundant requests when resolving", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient()
      .$makeCaller("item", async (c, param: number) => {
        await delay(20);
        return await endpointMock(param);
      })
      .setConcurrency("debounce");

    const calls = [];
    calls.push(caller(1));
    calls.push(caller(2));
    calls.push(caller(3));

    await Promise.all(calls);

    // Should only be two calls.
    // The first one should have completed,
    // The second should be skipped because it was overwritten by the 3rd,
    // and the 3rd should complete.
    expect(endpointMock.mock.calls.length).toBe(2);
    expect(endpointMock.mock.calls[0][0]).toBe(1);
    expect(endpointMock.mock.calls[1][0]).toBe(3);

    await expect(calls[0]).resolves.toBeTruthy();
    await expect(calls[1]).resolves.toBeFalsy();
    await expect(calls[2]).resolves.toBeTruthy();
  });

  test("concurrencyMode 'debounce' ignores redundant requests when throwing", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient()
      .$makeCaller("item", async (c, param: number) => {
        await delay(20);
        // endpointMock in this case is just being used to record our parameter's value.
        // In a real world case, the endpoint itself would throw.
        await endpointMock(param);
        throw "thrown";
      })
      .setConcurrency("debounce");

    const calls = [];
    calls.push(caller(1));
    calls.push(caller(2));
    calls.push(caller(3));

    // Wait for all the promises to reject.
    for (let i = 0; i < calls.length; i++) {
      try {
        await calls[i];
      } catch {
        // will throw;
      }
    }

    // Should only be two calls.
    // The first one should have completed,
    // The second should be skipped because it was overwritten by the 3rd,
    // and the 3rd should complete.
    expect(endpointMock.mock.calls.length).toBe(2);
    expect(endpointMock.mock.calls[0][0]).toBe(1);
    expect(endpointMock.mock.calls[1][0]).toBe(3);

    await expect(calls[0]).rejects.toBe("thrown");

    // Aborted calls don't throw/reject, since their aborting
    // is normal, expected behavior.
    // They resolve to nothing.
    await expect(calls[1]).resolves.toBeFalsy();

    await expect(calls[2]).rejects.toBe("thrown");
  });

  test("concurrencyMode 'cancel' cancels all previous requests", async () => {
    AxiosClient.defaults.adapter = vitest.fn().mockImplementation(async () => {
      await delay(20);
      return <AxiosResponse<any>>{
        data: { wasSuccessful: true, object: { personId: 1 } },
        status: 200,
      };
    });

    const caller = new PersonApiClient()
      .$makeCaller("item", (c, param: number) => c.get(1))
      .setConcurrency("cancel");

    expect(caller.isLoading).toBeFalsy();
    const prom1 = caller(1);
    expect(caller.isLoading).toBeTruthy();
    const prom2 = caller(2);
    expect(caller.isLoading).toBeTruthy();
    const prom3 = caller(3);
    expect(caller.isLoading).toBeTruthy();

    const res1 = await prom1;
    expect(caller.isLoading).toBeTruthy();
    const res2 = await prom2;
    expect(caller.isLoading).toBeTruthy();
    const res3 = await prom3;
    // isLoading should not become false until the last request (the not-cancelled one)
    // has finished.
    expect(caller.isLoading).toBeFalsy();

    expect(res1).toBeUndefined();
    expect(res2).toBeUndefined();
    expect(res3).toBeTruthy();
  });

  test("getPromise returns undefined when no request is pending", () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller("item", (c, num: number) =>
      endpointMock(num),
    );
    expect(caller.getPromise()).toBeUndefined();
  });

  test("getPromise returns the invocation promise", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      async (c, num: number) => {
        await delay(20);
        return await endpointMock(num);
      },
    );

    const invocation = caller(1);
    expect(caller.getPromise()).toBe(invocation);

    const result = await caller.getPromise();
    const _typeCheck: number | undefined = result;
    expect(result).toBe(1);
    expect(caller.isLoading).toBe(false);
    expect(caller.wasSuccessful).toBe(true);

    // Should be undefined after completion
    expect(caller.getPromise()).toBeUndefined();
  });

  test("getPromise on list caller returns array type", async () => {
    const endpointMock = vitest.fn(() => {
      return Promise.resolve({
        data: <ListResult<number>>{
          wasSuccessful: true,
          list: [1, 2, 3],
        },
        status: 200,
        statusText: "OK",
        headers: {},
        config: {},
      }) as ListResultPromise<number>;
    });
    const caller = new PersonApiClient().$makeCaller("list", async (c) => {
      await delay(20);
      return await endpointMock();
    });

    caller();
    const result = await caller.getPromise();
    const _typeCheck: number[] | undefined = result;
    expect(result).toEqual([1, 2, 3]);
  });

  test("getPromise rejects when request fails", async () => {
    const caller = new PersonApiClient().$makeCaller("item", async () => {
      await delay(20);
      throw new Error("fail");
    });

    const invocation = caller().catch(() => {});
    const pending = caller.getPromise();
    expect(pending).toBeDefined();

    await expect(pending).rejects.toThrow("fail");
    expect(caller.isLoading).toBe(false);

    // Should be undefined after completion
    expect(caller.getPromise()).toBeUndefined();

    await invocation;
  });

  test("getPromise tracks the latest request with cancel concurrency", async () => {
    AxiosClient.defaults.adapter = vitest.fn().mockImplementation(async () => {
      await delay(20);
      return <AxiosResponse<any>>{
        data: { wasSuccessful: true, object: { personId: 1 } },
        status: 200,
      };
    });

    const caller = new PersonApiClient()
      .$makeCaller("item", (c) => c.get(1))
      .setConcurrency("cancel");

    const prom1 = caller();
    expect(caller.getPromise()).toBe(prom1);

    // Second call cancels the first; getPromise should now track the second
    const prom2 = caller();
    expect(caller.getPromise()).toBe(prom2);
    expect(caller.getPromise()).not.toBe(prom1);

    await prom1;
    await prom2;
    expect(caller.isLoading).toBe(false);
    expect(caller.getPromise()).toBeUndefined();
  });

  test("handles successful file response", async () => {
    const blob = new Blob(["foo"]);

    AxiosClient.defaults.adapter = vitest.fn().mockImplementation(async (c) => {
      const resp: AxiosResponse<any> = {
        data: blob,
        status: 200,
        statusText: "OK",
        config: c,
        headers: {
          "content-disposition":
            "attachment; filename=\"sample-mp4-file small.mp4\"; filename*=UTF-8''sample-mp4-file%20small.mp4",
        },
      };
      return resp;
    });

    const caller = new ComplexModelApiClient().$makeCaller("item", (c) =>
      c.downloadAttachment_VaryString(42, "bob"),
    );

    await caller();

    expect(caller.wasSuccessful).toBeTruthy();
    expect(caller.result!.size).toBe(3);
    expect(caller.result!.name).toBe("sample-mp4-file small.mp4");
  });

  test("handles failed file response", async () => {
    const blob = new Blob(['{ "wasSuccessful": false, "message": "broken" }'], {
      type: "application/json",
    });

    AxiosClient.defaults.adapter = vitest.fn().mockImplementation(async () => {
      throw {
        isAxiosError: true,
        code: 400,
        response: <AxiosResponse<any>>{
          data: blob,
          status: 400,
        },
      };
    });

    const caller = new ComplexModelApiClient().$makeCaller("item", (c) =>
      c.downloadAttachment_VaryString(42, "bob"),
    );

    await expect(caller()).rejects.toBeTruthy();

    expect(caller.result).toBeNull();
    expect(caller.wasSuccessful).toBeFalsy();
    expect(caller.message).toBe("broken");
  });

  test("getResultObjectUrl", async () => {
    let currentBlob = new Blob(["foo"]);

    AxiosClient.defaults.adapter = vitest.fn().mockImplementation(
      async () =>
        <AxiosResponse<any>>{
          data: currentBlob,
          status: 200,
          headers: {},
        },
    );

    const createUrlMock = (URL.createObjectURL = vitest
      .fn()
      .mockImplementation(
        () => `blob://${Math.random().toString(36).slice(2)}`,
      ));
    const revokeUrlMock = (URL.revokeObjectURL = vitest.fn());

    const caller = new ComplexModelApiClient().$makeCaller("item", (c) =>
      c.downloadAttachment_VaryString(42, "bob"),
    );

    const wrapper = mount({ template: "<div></div>" });
    const vue = wrapper.vm as ComponentPublicInstance;

    // @ts-expect-error vue internal property
    const beforeUnmountHooks = () => getInternalInstance(vue)["bum"];

    // Act/assert - before any invocation.
    expect(caller.getResultObjectUrl(vue)).toBeUndefined();
    expect(beforeUnmountHooks()).toBeFalsy();

    // Act/Assert - first invocation
    await caller();
    const url1 = caller.getResultObjectUrl(vue);
    expect(createUrlMock).toBeCalledWith(caller.result);
    expect(beforeUnmountHooks()).toHaveLength(1);

    // Act/Assert - second invocation revokes first url and creates a second
    currentBlob = new Blob(["bar"]);
    await caller();
    const url2 = caller.getResultObjectUrl(vue);
    expect(revokeUrlMock).toBeCalledWith(url1);
    expect(createUrlMock).toBeCalledWith(caller.result);
    expect(beforeUnmountHooks()).toHaveLength(1);

    // Multiple invocations keep the same URL
    const url2_again = caller.getResultObjectUrl(vue);
    expect(url2_again).toBe(url2);

    // Act/Assert - Teardown

    wrapper.unmount();

    expect(revokeUrlMock).toBeCalledWith(url2);
    expect(beforeUnmountHooks()).toHaveLength(1);
  });

  describe("useResponseCaching", () => {
    test("dehydrates and hydrates object results", async () => {
      let requestNum = 1;

      const mock = mockEndpoint(
        new ComplexModelViewModel().$metadata.methods
          .instanceGetMethodWithObjParam,
        vitest.fn(async (req) => {
          return {
            wasSuccessful: true,
            object: {
              ...req.params.obj,
              valueArray: [requestNum++],
            },
          };
        }),
      );

      const runTest = () => {
        const caller = new ComplexModelApiClient().$makeCaller(
          "item",
          (c, param: number) =>
            c.instanceGetMethodWithObjParam(
              param,
              new ExternalParent({ stringList: ["foo"] }),
            ),
        );
        caller.useResponseCaching();
        return caller;
      };

      // Make the first caller and invoke it, which will populate the cache.
      const caller1 = runTest();
      expect(caller1.result).toBeNull();
      await caller1(42);
      expect(caller1.result).not.toBeNull();
      expect(caller1.result).toMatchObject(
        new ExternalParent({ stringList: ["foo"], valueArray: [1] }),
      );
      const cacheValue = Object.values(sessionStorage)[0];
      expect(cacheValue).not.toBeFalsy();
      expect(cacheValue).not.toContain("$metadata");

      expect(mock).toBeCalledTimes(1);
      expect(mock.mock.calls[0][0].params).toMatchObject({
        id: 42,
        obj: { stringList: ["foo"] },
      });

      // Make another caller. It will be dormant until invoked.
      const caller2 = runTest();
      expect(caller2.result).toBeNull();
      expect(caller2.wasSuccessful).toBe(null);
      expect(caller2.hasResult).toBe(false);
      expect(caller2.isLoading).toBe(false);

      // Invoke the caller. At this point, the cached response will get loaded.
      const caller2Promise = caller2(42);
      expect(caller2.result).toMatchObject(
        new ExternalParent({ stringList: ["foo"], valueArray: [1] }),
      );
      expect(caller2.wasSuccessful).toBe(true);
      expect(caller2.hasResult).toBe(true);
      expect(caller2.isLoading).toBe(true);

      // Wait for the HTTP request to finish.
      // Observe that the results are set with the new api response.
      await caller2Promise;
      expect(caller2.result).toMatchObject(
        new ExternalParent({ stringList: ["foo"], valueArray: [2] }),
      );
      expect(caller2.wasSuccessful).toBe(true);
      expect(caller2.hasResult).toBe(true);
      expect(caller2.isLoading).toBe(false);
      expect(mock).toBeCalledTimes(2);
      expect(mock.mock.calls[1][0].params).toMatchObject({
        id: 42,
        obj: { stringList: ["foo"] },
      });
    });

    test("respects stored max age", async () => {
      AxiosClient.defaults.adapter = () => makeEndpointMock("asdf")();

      const runTest = () => {
        const caller = new PersonApiClient().$makeCaller("item", (c) =>
          c.fullNameAndAge(42),
        );
        caller.useResponseCaching({ maxAgeSeconds: 0.4 });
        return caller;
      };

      // Make the first caller and invoke it, which will populate the cache.
      await runTest()();

      // Make another caller. Since essentially no time has passed,
      // the cached result will still be used.
      const caller2 = runTest();
      caller2();
      expect(caller2.result).toBe("asdf");

      // Make another caller, but this time wait for the cache to expire.
      const caller3 = runTest();
      // Since the stored max age is smaller, it is used instead of the configured max age.
      caller3.useResponseCaching({ maxAgeSeconds: 20 });
      await delay(500);
      caller3();
      // Since the stored value should now be expired, invoking the caller
      // will not have hydrated `result` with a cached value.
      expect(caller3.result).toBe(null);
    });

    test("respects configured max age if smaller than stored", async () => {
      AxiosClient.defaults.adapter = () => makeEndpointMock("asdf")();

      const runTest = () => {
        const caller = new PersonApiClient().$makeCaller("item", (c) =>
          c.fullNameAndAge(42),
        );
        caller.useResponseCaching({ maxAgeSeconds: 20 });
        return caller;
      };

      // Make the first caller and invoke it, which will populate the cache.
      await runTest()();

      // Make another caller. Since essentially no time has passed,
      // the cached result will still be used.
      const caller2 = runTest();
      caller2();
      expect(caller2.result).toBe("asdf");

      // Make another caller, but this time wait for the cache to expire.
      const caller3 = runTest();
      // Since the current max age is smaller, it is used instead of the stored max age.
      caller3.useResponseCaching({ maxAgeSeconds: 0.4 });
      await delay(500);
      caller3();
      // Since the stored value should now be expired, invoking the caller
      // will not have hydrated `result` with a cached value.
      expect(caller3.result).toBe(null);
    });

    test("does not use cached response when another response is already loaded", async () => {
      AxiosClient.defaults.adapter = () => makeEndpointMock("response1")();

      const runTest = () => {
        const caller = new PersonApiClient().$makeCaller("item", async (c) => {
          const res = c.fullNameAndAge(42);
          await delay(10);
          return await res;
        });
        caller.useResponseCaching({ maxAgeSeconds: 20 });
        return caller;
      };

      // Make the first caller and invoke it, which will populate the cache.
      await runTest()();

      // Make another caller.
      const caller2 = runTest();

      // Upon first invocation, the cache will be used.
      await caller2();
      expect(caller2.result).toBe("response1");

      // Change the result of the caller.
      // Real world scenario here is perhaps a ListViewModel where we've
      // since added items locally to its $items collection, and we don't want those items overwritten.
      caller2.result = "response2";

      // Start another invocation. It should not overwrite the value in `caller2.result`,
      // because the caller has already loaded at least once.
      caller2();
      expect(caller2.result).toBe("response2");

      // Check again in case of any promise weirdness.
      await delay(1);
      expect(caller2.result).toBe("response2");
    });
  });
});

describe("$makeCaller with args object", () => {
  test("is typed properly", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      (c, num: number) => endpointMock(num),
      () => ({ num: null as number | null }),
      (c, args) => endpointMock(args.num),
    );

    caller.args.num = 42;
    const result = await caller.invokeWithArgs();
    expect(result).toBe(42);
    expect(caller.rawResponse.data.object).toBe(42);
    expect(caller.rawResponse.status).toBe(200);
  });

  test("confirm is typed properly", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      "item",
      (c, num: number) => endpointMock(num),
      () => ({ num: null as number | null }),
      (c, args) => endpointMock(args.num),
    );

    window.confirm = vitest.fn(() => true);
    const result = await caller.confirmInvokeWithArgs("Are you sure?", {
      num: 10,
    });
    expect(result).toBe(10);
    expect(caller.rawResponse.data.object).toBe(10);
    expect(caller.rawResponse.status).toBe(200);
    expect(window.confirm).toBeCalledTimes(1);
  });

  test("allows return undefined from args invoker func", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new PersonApiClient().$makeCaller(
      PersonMeta.methods.rename,
      (c, num: number) => endpointMock(num),
      () => ({ num: null as null | number }),
      (c, args) => {
        if (args.num == 42) {
          return;
        }
        return endpointMock(args.num);
      },
    );

    const arg = 42;
    const result = await caller.invokeWithArgs({ num: arg });

    // The typings are actually wrong at the moment - `undefined` is not one of the types of `result`, but it should be.
    expect(result).toBeUndefined();

    expect(endpointMock.mock.calls.length).toBe(0);
    expect(caller.result).toBeNull();

    // Typescript typing tests - all of these are valid types of `result`.
    // Note that Typescript intellisense in VS code seems to be really messed up
    // right now and shows that `result` is only `string`.:
    caller.result = null;
    caller.result = 13;

    //@ts-expect-error `undefined` is not a valid value of `result`, even if the invoker doesn't always return.
    caller.result = undefined;
  });

  test("item caller url returns correct url", async () => {
    // Should never be called:
    const adapter = (AxiosClient.defaults.adapter = vitest
      .fn()
      .mockResolvedValue("foo"));

    const caller = new ComplexModelApiClient().$makeCaller(
      "item",
      (c) => c.downloadAttachment_VaryString(42, null),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryString(42, "bob+/"),
    );

    expect(caller.url).toBe(
      "/api/ComplexModel/downloadAttachment_VaryString?id=42&etag=bob%2B%2F",
    );
    expect(adapter).toBeCalledTimes(0);
  });

  test("list caller types are correct", async () => {
    const endpointMock = vitest.fn((arg?: number | null) => {
      return Promise.resolve({
        status: 200,
        data: {
          wasSuccessful: true,
          list: [arg],
          page: 1,
          pageCount: 100,
          pageSize: 1,
          totalCount: 100,
        },
      }) as ListResultPromise<number>;
    });

    const caller = new PersonApiClient().$makeCaller(
      "list",
      (c, num: number) => endpointMock(num),
      () => ({ num: null as number | null }),
      (c, args) => endpointMock(args.num),
    );

    const _result: number[] | null = caller.result;
    const _page: number | null = caller.page;
    const _pageCount: number | null = caller.pageCount;
    const _pageSize: number | null = caller.pageSize;
    const _totalCount: number | null = caller.totalCount;

    const promiseResult: number[] = await caller(42);
    expect(promiseResult).toStrictEqual([42]);
    expect(caller.result).toStrictEqual([42]);
    expect(caller.rawResponse.data.list?.[0]).toBe(42);
    expect(caller.rawResponse.status).toBe(200);

    await caller.invoke(42);
    await caller.confirmInvoke("Are you sure?", 42);
    await caller.invokeWithArgs({ num: 42 });
    //@ts-expect-error number expected
    await caller("asdf");
    //@ts-expect-error `num` expects number
    await caller.invokeWithArgs({ num: "asdf" });
  });

  describe.each(["item", "list"] as const)("for %s transport", (type) => {
    const makeCaller = (
      endpointMock: ReturnType<
        typeof makeEndpointMock<number | null | undefined>
      >,
    ) =>
      new PersonApiClient().$makeCaller(
        type,
        (c, num: number) => endpointMock(num),
        () => ({ num: null as number | null }),
        (c, args) => endpointMock(args.num),
      );

    test("uses own args if args not specified", () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      caller.invokeWithArgs();
      expect(endpointMock.mock.calls[0][0]).toBe(42);
    });

    test("own args are reactive", async () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      const vue = mountData({ caller });

      let called = false;
      vue.$watch("caller.args.num", () => {
        called = true;
      });

      await vue.$nextTick();
      expect(called).toBe(false);
      caller.args.num = 42;
      expect(called).toBe(false);

      await vue.$nextTick();
      expect(called).toBe(true);
    });

    test("uses custom args if specified", () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      caller.invokeWithArgs({
        num: 17,
      });
      expect(endpointMock.mock.calls[0][0]).toBe(17);
    });

    test("sets state properties appropriately", async () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      const promise = caller.invokeWithArgs();

      await promise;

      expect(caller.wasSuccessful).toBeTruthy();
    });

    test("debounce ignores redundant requests when resolving", async () => {
      const endpointMock = makeEndpointMock();
      const caller = new PersonApiClient()
        .$makeCaller(
          type,
          (c, num: number) => endpointMock(num),
          () => ({ num: null as number | null }),
          async (c, args) => {
            await delay(20);
            return await endpointMock(args.num);
          },
        )
        .setConcurrency("debounce");

      const calls = [];
      caller.args.num = 1;
      calls.push(caller.invokeWithArgs());
      caller.args.num = 2;
      calls.push(caller.invokeWithArgs());
      caller.args.num = 3;
      calls.push(caller.invokeWithArgs());

      await Promise.all(calls);

      // Should only be two calls.
      // The first one should have completed,
      // The second should be skipped because it was overwritten by the 3rd,
      // and the 3rd should complete.
      expect(endpointMock.mock.calls.length).toBe(2);
      expect(endpointMock.mock.calls[0][0]).toBe(1);
      expect(endpointMock.mock.calls[1][0]).toBe(3);

      await expect(calls[0]).resolves.toBeTruthy();
      await expect(calls[1]).resolves.toBeFalsy();
      await expect(calls[2]).resolves.toBeTruthy();
    });
  });
});

describe("mapQueryToParams", () => {
  test("round-trips", () => {
    const params = new ListParameters();
    params.page = 2;
    params.pageSize = 3;
    params.fields = ["test1", "test2"];
    params.includes = "includes";
    params.search = "needle";
    params.filter = {
      name: "bob",
      int: "1",
      bool: "true",
    };
    params.noCount = true;
    params.orderBy = "orderBy";
    params.orderByDescending = "orderByDesc";
    params.dataSource = new Person.DataSources.ParameterTestsSource({
      bytes: "SGVsbG8gV29ybGQ=",
      intArray: [1, 2, 3],
      personCriterion: new PersonCriteria({
        gender: Genders.Female,
        name: "Grace",
      }),
      personCriteriaArray: [
        new PersonCriteria({
          gender: Genders.Female,
          name: "Grace",
        }),
      ],
    });

    const string = JSON.stringify(mapParamsToDto(params));
    const parsed = mapQueryToParams(
      JSON.parse(string),
      ListParameters,
      new Person().$metadata,
    );

    expect(parsed).toMatchObject(params);
    expect(string).toBe(JSON.stringify(mapParamsToDto(parsed)));
  });
});

describe("ModelApiClient", () => {
  describe("get method", () => {
    test.each([
      {
        description: "date primary keys",
        client: DateOnlyPkApiClient,
        id: new Date(2023, 4, 15), // May 15, 2023 in local timezone (month is 0-indexed)
        expectedUrl: "/DateOnlyPk/get/2023-05-15",
      },
      {
        description: "string primary keys",
        client: StringIdentityApiClient,
        id: "test-id",
        expectedUrl: "/StringIdentity/get/test-id",
      },
    ])(
      "uses mapToDto to convert ID for $description",
      async ({ client, id, expectedUrl }) => {
        const mock = (AxiosClient.defaults.adapter = makeAdapterMock());

        const apiClient = new client();
        await apiClient.get(id as any);

        expect(mock).toHaveBeenCalledWith(
          expect.objectContaining({
            url: expectedUrl,
          }),
        );
      },
    );
  });

  describe("delete method", () => {
    test.each([
      {
        description: "date primary keys",
        client: DateOnlyPkApiClient,
        id: new Date(2023, 4, 15), // May 15, 2023 in local timezone (month is 0-indexed)
        expectedUrl: "/DateOnlyPk/delete/2023-05-15",
      },
      {
        description: "string primary keys",
        client: StringIdentityApiClient,
        id: "test-id",
        expectedUrl: "/StringIdentity/delete/test-id",
      },
    ])(
      "uses mapToDto to convert ID for $description",
      async ({ client, id, expectedUrl }) => {
        const mock = (AxiosClient.defaults.adapter = makeAdapterMock());

        const apiClient = new client();
        await apiClient.delete(id as any);

        expect(mock).toHaveBeenCalledWith(
          expect.objectContaining({
            url: expectedUrl,
          }),
        );
      },
    );
  });

  describe("refResponse functionality", () => {
    test("$useRefResponse sets Accept header on ApiClient", async () => {
      const mock = (AxiosClient.defaults.adapter = vitest
        .fn()
        .mockResolvedValue(<AxiosResponse<any>>{
          data: { wasSuccessful: true, object: {} },
          status: 200,
        }));

      const client = new PersonApiClient().$useRefResponse();

      await client.get(1);

      expect(mock).toHaveBeenCalledWith(
        expect.objectContaining({
          headers: expect.objectContaining({
            Accept: ["application/json+ref", "application/json"],
          }),
        }),
      );
    });

    test("useRefResponse on API caller sets Accept header", async () => {
      const mock = (AxiosClient.defaults.adapter = vitest
        .fn()
        .mockResolvedValue(<AxiosResponse<any>>{
          data: { wasSuccessful: true, object: {} },
          status: 200,
        }));

      const client = new PersonApiClient();
      const caller = client
        .$makeCaller("item", (c) => c.get(1))
        .useRefResponse();

      await caller();

      expect(mock).toHaveBeenCalledWith(
        expect.objectContaining({
          headers: expect.objectContaining({
            Accept: ["application/json+ref", "application/json"],
          }),
        }),
      );
    });

    test("refResponse returns this for method chaining", () => {
      const client = new PersonApiClient();
      const result = client.$useRefResponse();
      expect(result).toBe(client);

      const caller = client.$makeCaller("item", (c) => c.get(1));
      const callerResult = caller.useRefResponse();
      expect(callerResult).toBe(caller);
    });

    test("refResponse is ignored for file-returning methods", async () => {
      const mock = (AxiosClient.defaults.adapter = vitest
        .fn()
        .mockResolvedValue(<AxiosResponse<any>>{
          data: new Blob(),
          status: 200,
        }));

      const client = new ComplexModelApiClient();

      // Create a caller with refResponse enabled for a file-returning method
      const caller = client
        .$makeCaller("item", (c) => c.downloadAttachment(1))
        .useRefResponse();

      await caller();

      expect(mock).toHaveBeenCalledWith(
        expect.not.objectContaining({
          headers: expect.objectContaining({
            Accept: ["application/json+ref", "application/json"],
          }),
        }),
      );
    });
  });
});

describe("useAppUpdateCheck", () => {
  const APP_BUILD_HEADER = "x-app-build";

  function makeAxiosWithMockAdapter(headers: Record<string, string> = {}) {
    const instance = axios.create();
    instance.defaults.adapter = vitest.fn().mockResolvedValue(<AxiosResponse>{
      data: { wasSuccessful: true },
      status: 200,
      statusText: "OK",
      headers,
      config: {} as any,
    });
    return instance;
  }

  test("records initial build value and isUpdateAvailable starts false", async () => {
    const axiosInstance = makeAxiosWithMockAdapter({
      [APP_BUILD_HEADER]: "build-1",
    });
    const scope = effectScope();
    let result!: ReturnType<typeof useAppUpdateCheck>;

    scope.run(() => {
      result = useAppUpdateCheck(axiosInstance);
    });

    expect(result.isUpdateAvailable.value).toBe(false);
    await axiosInstance.get("/test");
    expect(result.isUpdateAvailable.value).toBe(false);

    scope.stop();
  });

  test("isUpdateAvailable becomes true when build header changes", async () => {
    let buildHeader = "build-1";
    const instance = axios.create();
    instance.defaults.adapter = vitest.fn().mockImplementation(() =>
      Promise.resolve(<AxiosResponse>{
        data: { wasSuccessful: true },
        status: 200,
        statusText: "OK",
        headers: { [APP_BUILD_HEADER]: buildHeader },
        config: {} as any,
      }),
    );

    const scope = effectScope();
    let result!: ReturnType<typeof useAppUpdateCheck>;

    scope.run(() => {
      result = useAppUpdateCheck(instance);
    });

    await instance.get("/test");
    expect(result.isUpdateAvailable.value).toBe(false);

    buildHeader = "build-2";
    await instance.get("/test");
    expect(result.isUpdateAvailable.value).toBe(true);

    scope.stop();
  });

  test("isUpdateAvailable becomes true when error response has different build header", async () => {
    let buildHeader = "build-1";
    let shouldError = false;
    const instance = axios.create();
    instance.defaults.adapter = vitest.fn().mockImplementation(() => {
      if (shouldError) {
        const err = new AxiosError("Server Error");
        err.response = {
          data: {},
          status: 500,
          statusText: "Internal Server Error",
          headers: { [APP_BUILD_HEADER]: buildHeader },
          config: {} as any,
        };
        return Promise.reject(err);
      }
      return Promise.resolve(<AxiosResponse>{
        data: { wasSuccessful: true },
        status: 200,
        statusText: "OK",
        headers: { [APP_BUILD_HEADER]: buildHeader },
        config: {} as any,
      });
    });

    const scope = effectScope();
    let result!: ReturnType<typeof useAppUpdateCheck>;

    scope.run(() => {
      result = useAppUpdateCheck(instance);
    });

    await instance.get("/test");
    expect(result.isUpdateAvailable.value).toBe(false);

    buildHeader = "build-2";
    shouldError = true;
    await instance.get("/test").catch(() => {});
    expect(result.isUpdateAvailable.value).toBe(true);

    scope.stop();
  });

  test("interceptor is ejected when scope is stopped", async () => {
    const instance = axios.create();
    instance.defaults.adapter = vitest.fn().mockResolvedValue(<AxiosResponse>{
      data: { wasSuccessful: true },
      status: 200,
      statusText: "OK",
      headers: { [APP_BUILD_HEADER]: "build-1" },
      config: {} as any,
    });

    const scope = effectScope();

    scope.run(() => {
      useAppUpdateCheck(instance);
    });

    const interceptorCountBefore = (
      instance.interceptors.response as any
    ).handlers.filter(Boolean).length;

    scope.stop();

    const interceptorCountAfter = (
      instance.interceptors.response as any
    ).handlers.filter(Boolean).length;

    expect(interceptorCountAfter).toBe(interceptorCountBefore - 1);
  });
});
