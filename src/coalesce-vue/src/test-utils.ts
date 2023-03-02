import { AxiosRequestConfig, AxiosResponse } from "axios";

import { AxiosClient, ItemResult, ListResult } from "./api-client.js";
import type {
  ItemMethod,
  ListMethod,
  Method,
  TypeDiscriminatorToType,
} from "./metadata.js";

type PromiseOrSync<T> = T | Promise<T>;

type EndpointSpec =
  | Method
  | `/${string}/${"list" | "get" | "count" | "save" | "delete" | string}`;

type EndpointMock<TEndpoint extends EndpointSpec> = (
  request: AxiosRequestConfig
) => PromiseOrSync<
  TEndpoint extends ListMethod | `/${string}/${"list"}`
    ? ListResult
    : TEndpoint extends `/${string}/${"count"}`
    ? ItemResult<number>
    : TEndpoint extends ItemMethod
    ? ItemResult<TypeDiscriminatorToType<TEndpoint["return"]["type"]>>
    : ItemResult | ListResult
>;

let mocks: {
  endpoint: EndpointSpec;
  adapter: (
    config: AxiosRequestConfig
  ) => PromiseOrSync<ItemResult | ListResult>;
}[];

/** Mock a coalesce API endpoint, providing your own implementation for the API request.
 
 * Example:
 * ```
 * mockEndpoint(
      "/Course/list",
      (req) => ({
        wasSuccessful: true,
        list: [
          { courseId: 1, title: "CS 101" },
          { courseId: 2, title: "CS 102" },
        ],
      })
    )
 * ```
 * 
 * Example:
 * ```
 * const fn = mockEndpoint(
      new Student().$metadata.methods.getName,
      vitest.fn((req) => ({
        wasSuccessful: true,
        object: "Steve",
      }))
    )
    // Do testing ...
    expect(fn).toHaveBeenCalledOnce();
    fn.destroy();
 * ```
 */
export function mockEndpoint<
  TEndpoint extends EndpointSpec,
  TMock extends EndpointMock<TEndpoint>
>(endpoint: TEndpoint, mock: TMock) {
  if (!mocks) {
    mocks = [];
    AxiosClient.interceptors.request.use((config) => {
      for (let i = mocks.length - 1; i >= 0; i--) {
        const mock = mocks[i];
        if (
          (typeof mock.endpoint == "string" &&
            config.url
              ?.toLowerCase()
              .startsWith(mock.endpoint.toLowerCase())) ||
          (typeof mock.endpoint == "object" &&
            "__coalesce_method_meta" in config &&
            (config.__coalesce_method_meta as any)() == mock.endpoint)
        ) {
          // This request matches a registered mock.
          // Substitute out the adapter for our mock.
          config.adapter = async function (config: AxiosRequestConfig) {
            const result = await mock.adapter(config);

            // If the result is a ListResult and the mock didn't set pagination fields,
            // fill them in so that server behavior is emulated more accurately without requiring
            // the user to have to do this tediousness on every mock.
            if ("list" in result) {
              result.page ??= 1;
              result.totalCount ??= result.list?.length ?? 0;
              result.pageSize ??= Math.max(10, result.totalCount);
              result.pageCount ??= Math.ceil(
                result.totalCount / result.pageSize
              );
            }

            const response = {
              status: result.wasSuccessful ? 200 : 400,
              statusText: result.wasSuccessful ? "OK" : "Bad Request",
              data: result,
              config,
              headers: { "Content-Type": "application/json" },
            } as AxiosResponse;

            const validateStatus = response.config.validateStatus;
            if (!validateStatus || validateStatus(response.status)) {
              return response;
            } else {
              var error = new Error(
                "Request failed with status code " + response.status
              ) as any;
              error.isAxiosError = true;
              error.response = response;
              throw error;
            }
          };
          return config;
        }
      }
      return config;
    });
  }

  const mockData = {
    adapter: mock,
    endpoint,
  };
  mocks.push(mockData);

  // Return the passed in mock so it can be wrapped in a `vitest.fn`
  // so the user can spy on it.
  return Object.assign(mock, {
    /** Tear down the mock, removing the handler from the global axios adapter. */
    destroy() {
      mocks.splice(mocks.indexOf(mockData), 1);
    },
  });
}
