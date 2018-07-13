import { ModelApiClient } from "../src/api-client";
import * as $models from "./targets.models"
import * as $metadata from "./targets.metadata"

export class StudentApiClient extends ModelApiClient<$models.Student> {
  constructor() { super($metadata.Student) }
}