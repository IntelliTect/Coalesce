import { AxiosAdapter, AxiosRequestConfig, AxiosResponse } from "axios";
//@ts-expect-error untyped
import settle from "axios/lib/core/settle.js";

import { AxiosClient, ItemResult, ListResult } from "./api-client";
import {
  ItemMethod,
  ListMethod,
  Method,
  TypeDiscriminatorToType,
} from "./metadata";

let originalAdapter: AxiosAdapter | undefined;

type PromiseOrSync<T> = T | Promise<T>;

type MockAdapter = {
  mocks: {
    endpoint: EndpointSpec;
    adapter: (
      config: AxiosRequestConfig
    ) => PromiseOrSync<ItemResult | ListResult>;
  }[];
  __coalesce_mock: boolean;
} & AxiosAdapter;

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

/** Mock a coalesce API endpoint, providing your own implementation for the API request.
 
 * Example:
 * ```
 * mockEndpoint(
      "/Course/list",
      vitest.fn((req) => ({
        wasSuccessful: true,
        list: [
          { courseId: 1, title: "CS 101" },
          { courseId: 2, title: "CS 102" },
        ],
      }))
    )
 * ```
 * 
 * Example:
 * ```
 * mockEndpoint(
      new Student().$metadata.methods.getName,
      vitest.fn((req) => ({
        wasSuccessful: true,
        object: "Steve",
      }))
    )
 * ```
 */
export function mockEndpoint<
  TEndpoint extends EndpointSpec,
  TMock extends EndpointMock<TEndpoint>
>(endpoint: TEndpoint, mock: TMock) {
  let mockAdapter: MockAdapter;
  if (
    !AxiosClient.defaults.adapter ||
    !("__coalesce_mock" in AxiosClient.defaults.adapter)
  ) {
    originalAdapter = AxiosClient.defaults.adapter;

    mockAdapter = AxiosClient.defaults.adapter = Object.assign(
      async function (config: AxiosRequestConfig) {
        // Iterate in reverse so most recent take priority.
        for (let i = mockAdapter.mocks.length - 1; i >= 0; i--) {
          const mock = mockAdapter.mocks[i];
          if (
            (typeof mock.endpoint == "string" &&
              config.url
                ?.toLowerCase()
                .startsWith(mock.endpoint.toLowerCase())) ||
            (typeof mock.endpoint == "object" &&
              "__coalesce_method_meta" in config &&
              (config.__coalesce_method_meta as any)() == mock.endpoint)
          ) {
            const result = await mock.adapter(config);
            if ("list" in result) {
              result.page ??= 1;
              result.totalCount ??= result.list?.length ?? 0;
              result.pageSize ??= Math.max(10, result.totalCount);
              result.pageCount ??= Math.ceil(
                result.totalCount / result.pageSize
              );
            }
            return new Promise((resolve, reject) =>
              settle(resolve, reject, {
                status: result.wasSuccessful ? 200 : 400,
                data: result,
                config,
                headers: {},
              } as AxiosResponse)
            );
          }
        }
        console.warn(
          `coalesce's mockEndpoint was used, but no mock matching request ${config.url} was found.`
        );
        return originalAdapter!(config) as any;
      },
      {
        mocks: [],
        __coalesce_mock: true,
      }
    );
  } else {
    mockAdapter = AxiosClient.defaults.adapter as any;
  }

  const mockData = {
    adapter: mock,
    endpoint,
  };
  mockAdapter.mocks.push(mockData);

  // Return the passed in mock so it can be wrapped in a `vitest.fn`
  // so the user can spy on it.
  return Object.assign(mock, {
    /** Tear down the mock, removing the handler from the global axios adapter. */
    destroy() {
      mockAdapter.mocks.splice(mockAdapter.mocks.indexOf(mockData), 1);
    },
  });
}
