

import { ItemResult } from '../src/api-client'
import { StudentApiClient } from './targets.apiclients';

describe("$makeCaller", () => {

  const wait = async (wait: number) => await new Promise(resolve => setTimeout(resolve, wait))

  let endpointMock: jest.Mock<any>;
  beforeEach(() => {
    endpointMock = jest.fn().mockResolvedValue({ data: <ItemResult>{
      wasSuccessful: true, object: "foo"
    }})
  })

  test("passes parameters to invoker func", () => {
    const caller = new StudentApiClient().$makeCaller("item", (c, num: number) => endpointMock(num))

    const arg = 42;
    caller(arg);
    expect(endpointMock.mock.calls[0][0]).toBe(arg);
  })

  test("passes this to invoker func", async () => {
    type Model = { value: number, caller: () => any }
    const fulfilledMock = jest.fn()
    const model = <Model>{
      value: 42,
      caller: new StudentApiClient()
        .$makeCaller("item", function(this: Model, c) { return endpointMock(this.value) })
        .onFulfilled(fulfilledMock)
    }

    model.caller();
    expect(endpointMock.mock.calls[0][0]).toBe(model.value);

    await wait(1); // Wait a tick for the promise to fulfill.
    expect(fulfilledMock.mock.instances[0]).toBe(model);
    expect(fulfilledMock.mock.calls[0][0]).toBe(model.caller);
  })


  test("preserves getter/setter behavior on ApiState after _makeReactive()", () => {
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

  test("debounce ignores redundant requests when resolving", async () => {
    const caller = new StudentApiClient()
      .$makeCaller("item", async (c, param: number) => { 
        await wait(20)
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

  test("debounce ignores redundant requests when throwing", async () => {
    const caller = new StudentApiClient()
      .$makeCaller("item", async (c, param: number) => { 
        await wait(20)
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