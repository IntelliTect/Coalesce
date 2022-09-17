import { ItemResult, ModelApiClient } from "../src/api-client";
import * as $models from "./targets.models";
import * as $metadata from "./targets.metadata";
import { AxiosPromise, AxiosRequestConfig } from "axios";

export class StudentApiClient extends ModelApiClient<$models.Student> {
  constructor() {
    super($metadata.Student);
  }

  public fullNameAndAge(
    id: number,
    $config?: AxiosRequestConfig
  ): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.fullNameAndAge;
    const $params = {
      id,
    };
    return this.$invoke($method, $params, $config);
  }

  public getWithObjParam(
    id: number,
    advisor?: $models.Advisor,
    $config?: AxiosRequestConfig
  ): AxiosPromise<ItemResult<$models.Advisor>> {
    const $method = this.$metadata.methods.getWithObjParam;
    const $params = {
      id,
      advisor,
    };
    return this.$invoke($method, $params, $config);
  }

  public getFile(
    id: number,
    etag: string | null,
    $config?: AxiosRequestConfig
  ): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.getFile;
    const $params = {
      id,
      etag,
    };
    return this.$invoke($method, $params, $config);
  }
}

export class CourseApiClient extends ModelApiClient<$models.Course> {
  constructor() {
    super($metadata.Course);
  }
}

export class AdvisorApiClient extends ModelApiClient<$models.Advisor> {
  constructor() {
    super($metadata.Advisor);
  }
}
