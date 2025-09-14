/// <reference types="vite/client" />
/// <reference types="vitest/globals" />

declare global {
  declare const BUILD_DATE: Date | null | undefined;
  declare const ASPNETCORE_ENVIRONMENT: string;
  //#if AppInsights
  declare const APPLICATIONINSIGHTS_CONNECTION_STRING:
    | string
    | null
    | undefined;
  //#endif
}

export {};
