

import Vue from 'vue';

import { ItemMethod } from '../src/metadata'
import { AxiosClient, ItemResult, ItemResultPromise, ListParameters, AxiosListResult } from '../src/api-client'
import { StudentApiClient } from './targets.apiclients';
import { Student as StudentMeta } from './targets.metadata'
import { AxiosError, AxiosResponse, AxiosAdapter } from 'axios';
import { Student, Advisor } from './targets.models';
import { delay } from './test-utils';


function makeEndpointMock() {
  return jest.fn<ItemResultPromise<string>, [number | undefined | null]>().mockResolvedValue({ data: <ItemResult>{
    wasSuccessful: true, object: "foo"
  }, status: 200, statusText: "OK", headers: {}, config:{}});
}


describe("error handling", () => {
  test("throws error when server returns raw string instead of object", async () => {
    AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: "<!doctype html><html><body></body></html>",
        status: 200
      })

    await expect(new StudentApiClient().get(1))
      .rejects.toThrow("Unexpected raw string response from server.")
  })
})

describe("$withSimultaneousRequestCaching", () => {
  test("uses proper cache key for standard method", async () => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockImplementation(async () => {
        // Delay so the calls don't complete instantly (which would subvert request caching).
        await delay(30);
        return <AxiosListResult<Student>>{
          data: {wasSuccessful: true, list: [] as Student[], page: 1, pageCount: 0, pageSize: 10 },
          status: 200
        }
      })

    var client = new StudentApiClient()
      .$withSimultaneousRequestCaching();

    const invoker = (nameStart: string) => {
      const params = new ListParameters;
      const ds = params.dataSource = new Student.DataSources.Search
      ds.nameStart = nameStart
      params.fields = ["name", "birthDate"];
      return client.list(params);
    }

    // Act
    await Promise.all([
      invoker("bob"),
      invoker("bob"),
      invoker("steve"),
      invoker("steve"),
      invoker("bob"),
    ])

    // Assert
    expect(mock).toBeCalledTimes(2); // 2 distinct sets of parameters => 2 calls
    const actualParams = mock.mock.calls[0] as Parameters<AxiosAdapter>;
    expect(actualParams[0].params["dataSource"]).toBe("Search")
    expect(actualParams[0].params["dataSource.nameStart"]).toBe("bob")
    expect(actualParams[0].params["fields"]).toBe("name,birthDate")
    expect(actualParams[0].params["page"]).toBe("1")
    expect(actualParams[0].params["pageSize"]).toBe('10')
  })


  test("uses proper cache key for custom method", async () => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockImplementation(async () => {
        // Delay so the calls don't complete instantly (which would subvert request caching).
        await delay(30);
        return <AxiosResponse<any>>{
          data: {wasSuccessful: true, object: ''},
          status: 200
        }
      })

    var client = new StudentApiClient()
      .$withSimultaneousRequestCaching();

    const invoker = (advisorId: number) => client.$invoke(
      StudentMeta.methods.getWithObjParam,
      { advisor: new Advisor({name: 'Ad Visor', advisorId}) }
    )

    // Act
    await Promise.all([
      invoker(3),
      invoker(3),
      invoker(4),
      invoker(4),
    ])

    // Assert
    expect(mock).toBeCalledTimes(2); // 2 distinct sets of parameters => 2 calls
    const actualParams = mock.mock.calls[0] as Parameters<AxiosAdapter>;
    expect(actualParams[0].params).toMatchObject({
      "advisor": {
        "advisorId": 3, 
        "name": "Ad Visor", 
        "studentWrapperObject": null
      }
    })
  })
})


describe("$invoke", () => {
  test("doesn't error when params are missing", async () => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: ''},
        status: 200
      })

    // The use case here is to allow optional params to be missing.
    // Technically the one we're omitting ('id') would be a required param,
    // but the client code doesn't make that distinction - the server does, 
    // and we're mocking the server.
    await expect(new StudentApiClient().$invoke(
        StudentMeta.methods.fullNameAndAge,
        { } as any
      ))
      .resolves.toBeTruthy()

      expect(mock.mock.calls[0][0]).toMatchObject({ params: { id: undefined } });
  })

  test("doesn't error when unexpected params are provided", async () => {
    // The use case here is to allow, for e.g., a component that always provides a "search"
    // param to an API to still function even if that API doesn't use or care about a "search" param.
    // This might seem like a dumb case to test, but it was actually broken because we were iterating
    // over the actual provided params when mapping the params, instead of over the method's metadata.
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: ''},
        status: 200
      })

    await expect(new StudentApiClient().$invoke(
        StudentMeta.methods.fullNameAndAge,
        // Our types are actually so good that they will catch this as an error, so we cast to any.
        { id: 1, extraParam: '' } as any 
      ))
      .resolves.toBeTruthy()

    expect(mock.mock.calls[0][0]).toMatchObject({ params: { id: 1 } });
  })

  test("passes single file as FormData", async() => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: ''},
        status: 200
      })

    const methodMeta: ItemMethod = {
      name: 'test', 
      displayName: '', 
      httpMethod: 'POST', 
      return: { displayName: '', name: '$return', type: 'void', role: 'value' },
      transportType: 'item',
      params: {
        id: { type: 'number', role: 'value', displayName: '', name: 'id' },
        file: { type: 'file', role: 'value', displayName: '', name: 'file' },
        student: { type: 'model', role: 'value', displayName: '', name: 'student', typeDef: StudentMeta }
      }
    };
    const file = new File([new ArrayBuffer(1)], "fileName", { type: "application/pdf" });

    var response = await new StudentApiClient().$invoke(methodMeta, { 
      id:42, file, student: <Student>{ name: "bob&bob=bob", studentAdvisorId: null },
    });

    expect(mock).toBeCalledTimes(1);
    const formData = (mock.mock.calls[0][0].data as FormData);
    expect(formData).toBeInstanceOf(FormData)
    expect(formData.get('id')).toBe("42")
    expect(formData.get('file')).toBe(file)
    expect(formData.get('student[name]')).toBe("bob&bob=bob")
    expect(formData.get('student[studentAdvisorId]')).toBe("")
  })

  test("passes file array as FormData", async() => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: ''},
        status: 200
      })

    const methodMeta: ItemMethod = {
      name: 'test', 
      displayName: '', 
      httpMethod: 'POST', 
      return: { displayName: '', name: '$return', type: 'void', role: 'value' },
      transportType: 'item',
      params: {
        id: { type: 'number', role: 'value', displayName: '', name: 'id' },
        files: { type: 'collection', role: 'value', displayName: '', name: 'file', itemType: {
          name: "$collectionItem",
          displayName: '',
          role: 'value',
          type: "file",
        } },
      }
    };

    const file1 = new File([new ArrayBuffer(1)], "fileName1", { type: "application/pdf" });
    const file2 = new File([new ArrayBuffer(2)], "fileName2", { type: "application/pdf" });

    var response = await new StudentApiClient().$invoke(methodMeta, { 
      id:42, files: [file1, file2]
    });

    expect(mock).toBeCalledTimes(1);
    const formData = (mock.mock.calls[0][0].data as FormData);
    expect(formData).toBeInstanceOf(FormData)
    expect(formData.get('id')).toBe("42")
    // Aspnetcore will not bind the files correctly if they're keyed as 
    // files[0] and files[1]. They must use the same key (matching the param name).
    // Untested: keying them as `files[]` might also work.
    expect(formData.getAll('files')).toEqual([file1, file2])
  })

  test("passes Uint8Array as FormData", async() => {
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: ''},
        status: 200
      })

    const methodMeta: ItemMethod = {
      name: 'test', 
      displayName: '', 
      httpMethod: 'POST', 
      return: { displayName: '', name: '$return', type: 'void', role: 'value' },
      transportType: 'item',
      params: {
        id: { type: 'number', role: 'value', displayName: '', name: 'id' },
        bin: { type: 'binary', role: 'value', displayName: '', name: 'file' },
      }
    };

    const bin = new Uint8Array([0x11, 0x22, 0x33]);
    var response = await new StudentApiClient().$invoke(methodMeta, { id:42, bin });

    expect(mock).toBeCalledTimes(1);
    const formData = (mock.mock.calls[0][0].data as FormData);
    expect(formData).toBeInstanceOf(FormData)
    expect(formData.get('id')).toBe("42")

    // This can only assert on size, not content,
    // since jsdom's Blob/File classes don't properly implement the spec
    // (https://w3c.github.io/FileAPI/#stream-method-algo). 
    // https://github.com/jsdom/jsdom/issues/2555.
    // This is cast as File because FormData converts Blob to File.
    expect((formData.get('bin') as File).size).toBe(3)
  })

  test("POST passes null correctly", async () => {
    // This makes sure that we correctly send NULL fields the server.
    // When I migrated away from the `qs` lib, I broke this... oops.
    const mock = AxiosClient.defaults.adapter = 
      jest.fn().mockResolvedValue(<AxiosResponse<any>>{
        data: {wasSuccessful: true, object: {}},
        status: 200
      })

    var result = await new StudentApiClient().save(new Student({
      studentAdvisorId: null,
      name: "bob"
    }), {fields: ["studentAdvisorId", "name"]});

    expect(mock.mock.calls[0][0].data).toBe("studentAdvisorId=&name=bob");
  })
})

describe("$makeCaller", () => {
  test("passes parameters to invoker func", () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient().$makeCaller("item", (c, num: number) => {
      endpointMock(num)
    })

    const arg = 42;
    caller(arg);
    expect(endpointMock.mock.calls[0][0]).toBe(arg);
  })

  test("failed requests re-throw errors", async () => {
    const caller = new StudentApiClient().$makeCaller("item", (c, num: number) => { 
      throw {
          name: 'mock error',
          message: 'mocked throw'
        } as AxiosError }
      )

    await expect(caller(42)).rejects.toBeTruthy();
  })

  test("allows return undefined from invoker func", () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient().$makeCaller("item", (c, num: number) => {
      if (num == 42) {
        return;
      }
      return (endpointMock(num) as ItemResultPromise<string>)
    })

    const arg = 42;
    const result = caller(arg);

    expect(result).resolves.toBeUndefined();
    expect(endpointMock.mock.calls.length).toBe(0);
  })

  test("onFulfilled callbacks are awaited when promises returned", async () => {
    const endpointMock = makeEndpointMock();
    let awaited = false;
    const model = {
      caller: new StudentApiClient()
        .$makeCaller("item", function(this: any, c) { return endpointMock(42) })
        .onFulfilled(async () => {
          await delay(50);
          awaited = true;
        })
    }

    await model.caller();
    expect(awaited).toBeTruthy();
    expect(model.caller.isLoading).toBeFalsy();
  })

  test("onRejected callbacks are awaited when promises returned", async () => {
    let awaited = false;
    const model = {
      caller: new StudentApiClient()
        .$makeCaller("item", function(this: any, c) { throw Error() })
        .onRejected(async () => {
          await delay(50);
          awaited = true;
        })
    }

    await expect(model.caller()).rejects.toBeTruthy()
    expect(awaited).toBeTruthy();
    expect(model.caller.isLoading).toBeFalsy();
  })

  test("passes this to invoker func", async () => {
    const endpointMock = makeEndpointMock();
    type Model = { value: number, caller: () => Promise<any> }
    const fulfilledMock = jest.fn()
    const model = <Model>{
      value: 42,
      caller: new StudentApiClient()
        .$makeCaller("item", function(this: Model, c) { return endpointMock(this.value) })
        .onFulfilled(fulfilledMock)
    }

    await model.caller();
    expect(endpointMock.mock.calls[0][0]).toBe(model.value);
    expect(fulfilledMock.mock.instances[0]).toBe(model);
    expect(fulfilledMock.mock.calls[0][0]).toBe(model.caller);
  })

  test("preserves getter/setter behavior on ApiState after _makeReactive()", () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient().$makeCaller("item", (c, num: number) => endpointMock(num))

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
    const concurrencyModeProp = Object.getOwnPropertyDescriptor(caller, 'concurrencyMode');
    expect(concurrencyModeProp).toBeUndefined();

  })

  test("concurrencyMode 'debounce' ignores redundant requests when resolving", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient()
      .$makeCaller("item", async (c, param: number) => { 
        await delay(20)
        return await endpointMock(param) 
      })
      .setConcurrency("debounce")

    const calls = []
    calls.push(caller(1))
    calls.push(caller(2))
    calls.push(caller(3))

    await Promise.all(calls)

    // Should only be two calls. 
    // The first one should have completed,
    // The second should be skipped because it was overwritten by the 3rd,
    // and the 3rd should complete.
    expect(endpointMock.mock.calls.length).toBe(2);
    expect(endpointMock.mock.calls[0][0]).toBe(1);
    expect(endpointMock.mock.calls[1][0]).toBe(3);
    
    expect(calls[0]).resolves.toBeTruthy();
    expect(calls[1]).resolves.toBeFalsy();
    expect(calls[2]).resolves.toBeTruthy();
  })

  test("concurrencyMode 'debounce' ignores redundant requests when throwing", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient()
      .$makeCaller("item", async (c, param: number) => { 
        await delay(20)
        // endpointMock in this case is just being used to record our parameter's value.
        // In a real world case, the endpoint itself would throw.
        await endpointMock(param)
        throw "thrown"
      })
      .setConcurrency("debounce")

    const calls = []
    calls.push(caller(1))
    calls.push(caller(2))
    calls.push(caller(3))

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
    
    expect(calls[0]).rejects.toBe("thrown");
    
    // Aborted calls don't throw/reject, since their aborting
    // is normal, expected behavior.
    // They resolve to nothing.
    expect(calls[1]).resolves.toBeFalsy();

    expect(calls[2]).rejects.toBe("thrown");
  })

  test("concurrencyMode 'cancel' cancels all previous requests", async () => {
    
    AxiosClient.defaults.adapter = 
      jest.fn().mockImplementation(async () => {
        await delay(20);
        return <AxiosResponse<any>>{
          data: { wasSuccessful: true, object: { personId: 1 }},
          status: 200
        }
      })

    const caller = new StudentApiClient()
      .$makeCaller("item", (c, param: number) => c.get(1) )
      .setConcurrency("cancel")

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
  })

  test("handles successful file response", async () => {
    let blob = new Blob(["foo"]);

    AxiosClient.defaults.adapter = jest.fn().mockImplementation(async () => <AxiosResponse<any>>{
      data: blob,
      status: 200,
      statusText: "OK",
      config: {},
      headers: {'content-disposition': "attachment; filename=\"sample-mp4-file small.mp4\"; filename*=UTF-8''sample-mp4-file%20small.mp4"}
    })

    const caller = new StudentApiClient().$makeCaller("item", (c) => c.getFile(42, 'bob'))

    await caller();

    expect(caller.wasSuccessful).toBeTruthy();
    expect(caller.result!.size).toBe(3);
    expect(caller.result!.name).toBe('sample-mp4-file small.mp4');
  })

  test("handles failed file response", async () => {
    let blob = new Blob(['{ "wasSuccessful": false, "message": "broken" }'], {type: "application/json"});

    AxiosClient.defaults.adapter = jest.fn().mockImplementation(async () => {
      throw {
        isAxiosError: true,
        code: 400,
        response: <AxiosResponse<any>>{
          data: blob,
          status: 400,
        }
      }
    })

    const caller = new StudentApiClient().$makeCaller("item", (c) => c.getFile(42, 'bob'))

    await expect(caller()).rejects.toBeTruthy()

    expect(caller.result).toBeNull();
    expect(caller.wasSuccessful).toBeFalsy();
    expect(caller.message).toBe('broken');
  })

  test("getResultObjectUrl", async () => {
    let currentBlob = new Blob(["foo"]);

    AxiosClient.defaults.adapter = jest.fn().mockImplementation(async () => <AxiosResponse<any>>{
      data: currentBlob,
      status: 200,
      headers: {}
    })

    const createUrlMock = URL.createObjectURL = jest.fn().mockImplementation(() =>
      `blob://${Math.random().toString(36).slice(2)}`)
    const revokeUrlMock = URL.revokeObjectURL = jest.fn()
    
    const caller = new StudentApiClient().$makeCaller(
      "item", 
      (c) => c.getFile(42, 'bob'),
    )
    const vue = new Vue();
    const $onSpy = jest.spyOn(vue, "$on");
    const $offSpy = jest.spyOn(vue, "$off");

    // Act/assert - before any invocation.
    expect(caller.getResultObjectUrl(vue)).toBeUndefined();
    expect($onSpy).toBeCalledTimes(0);
    expect($offSpy).toBeCalledTimes(0);

    // Act/Assert - first invocation
    await caller();
    const url1 = caller.getResultObjectUrl(vue);
    expect(createUrlMock).toBeCalledWith(caller.result);
    expect($onSpy).toBeCalledTimes(1);
    expect($offSpy).toBeCalledTimes(0);

    // Act/Assert - second invocation revokes first url and creates a second
    currentBlob = new Blob(["bar"]);
    await caller();
    const url2 = caller.getResultObjectUrl(vue);
    expect(revokeUrlMock).toBeCalledWith(url1);
    expect(createUrlMock).toBeCalledWith(caller.result);
    expect($onSpy).toBeCalledTimes(2);
    expect($offSpy).toBeCalledTimes(1);

    // Multiple invocations keep the same URL
    const url2_again = caller.getResultObjectUrl(vue);
    expect(url2_again).toBe(url2);

    // Act/Assert - Teardown
    vue.$destroy();
    expect(revokeUrlMock).toBeCalledWith(url2);
    expect($onSpy).toBeCalledTimes(2);
    expect($offSpy).toBeCalledTimes(3); // This is 3 because vue itself will also call $off() with no params when a component is destroyed.
  })
})


describe("$makeCaller with args object", () => {
  test("is typed properly", async () => {
    const endpointMock = makeEndpointMock();
    const caller = new StudentApiClient().$makeCaller(
      "item", 
      (c, num: number) => endpointMock(num),
      () => ({num: null as number | null}),
      (c, args) => endpointMock(args.num),
    )

    caller.args.num = 42;
    const result = await caller.invokeWithArgs();
    expect(result.data.object).toBe("foo");
  })

  test("item caller url returns correct url", async () => {
    // Should never be called:
    const adapter = AxiosClient.defaults.adapter = jest.fn().mockResolvedValue('foo')

    const caller = new StudentApiClient().$makeCaller(
      "item", 
      (c) => c.getFile(42, 'bob'),
      () => ({}),
      (c, args) => c.getFile(42, 'bob'),
    )

    expect(caller.url).toBe("/api/Students/getFile?id=42&etag=bob")
    expect(adapter).toBeCalledTimes(0);
  })

  describe.each(["item", "list"] as const)("for %s transport", (type) => {
    const makeCaller = (endpointMock: ReturnType<typeof makeEndpointMock>) =>
      new StudentApiClient().$makeCaller(
        type, 
        (c, num: number) => endpointMock(num),
        () => ({num: null as number | null}),
        (c, args) => endpointMock(args.num),
      );

    test("uses own args if args not specified", () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      caller.invokeWithArgs();
      expect(endpointMock.mock.calls[0][0]).toBe(42);
    })

    test("own args are reactive", async () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      const vue = new Vue({
        data: {
          caller
        }
      });

      let called = false;
      vue.$watch('caller.args.num', () => {
        called = true;
      });

      await vue.$nextTick();
      expect(called).toBe(false);
      caller.args.num = 42;
      expect(called).toBe(false);

      await vue.$nextTick();
      expect(called).toBe(true);
    })

    test("uses custom args if specified", () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      caller.invokeWithArgs({
        num: 17
      });
      expect(endpointMock.mock.calls[0][0]).toBe(17);
    })

    test("sets state properties appropriately", async () => {
      const endpointMock = makeEndpointMock();
      const caller = makeCaller(endpointMock);

      caller.args.num = 42;
      const promise = caller.invokeWithArgs();

      await promise;

      expect(caller.wasSuccessful).toBeTruthy();
    })
    
    test("debounce ignores redundant requests when resolving", async () => {
      const endpointMock = makeEndpointMock();
      const caller = new StudentApiClient()
        .$makeCaller(
          type, 
          (c, num: number) => endpointMock(num),
          () => ({num: null as number | null}),
          async (c, args) => {
            await delay(20)
            return await endpointMock(args.num) 
          },
        )
        .setConcurrency("debounce")

      const calls = []
      caller.args.num = 1;
      calls.push(caller.invokeWithArgs())
      caller.args.num = 2;
      calls.push(caller.invokeWithArgs())
      caller.args.num = 3;
      calls.push(caller.invokeWithArgs())

      await Promise.all(calls)

      // Should only be two calls. 
      // The first one should have completed,
      // The second should be skipped because it was overwritten by the 3rd,
      // and the 3rd should complete.
      expect(endpointMock.mock.calls.length).toBe(2);
      expect(endpointMock.mock.calls[0][0]).toBe(1);
      expect(endpointMock.mock.calls[1][0]).toBe(3);
      
      expect(calls[0]).resolves.toBeTruthy();
      expect(calls[1]).resolves.toBeFalsy();
      expect(calls[2]).resolves.toBeTruthy();
    })
  })
})