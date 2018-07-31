

import { ItemResult } from '../src/api-client'
import { StudentApiClient } from './targets.apiclients';

describe("$makeCaller", () => {

  const wait = async (wait: number) => await new Promise(resolve => setTimeout(resolve, wait))

  const endpointMock = jest.fn().mockResolvedValue({ data: <ItemResult>{
    wasSuccessful: true, object: "foo"
  }})

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

})