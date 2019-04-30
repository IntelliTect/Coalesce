import { ModelApiClient } from "../src/api-client";
import * as $models from "./targets.models"
import * as $metadata from "./targets.metadata"

export class StudentApiClient extends ModelApiClient<$models.Student> {
  constructor() { super($metadata.Student) }
}

export class CourseApiClient extends ModelApiClient<$models.Course> {
  constructor() { super($metadata.Course) }
}

export class AdvisorApiClient extends ModelApiClient<$models.Advisor> {
  constructor() { super($metadata.Advisor) }
}


// class Foo {

//   caller = new StudentApiClient().$makeCaller("item", (c, id: number) => c.get(id))

//   caller2 = new StudentApiClient().$makeCaller("list", function (this: Foo, c, a: number) {
//     return c.list()
//   })

//   constructor() {
//     this.caller2(1);
//   }
// }







