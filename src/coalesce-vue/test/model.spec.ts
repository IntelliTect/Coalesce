

import * as model from "../src/model";
import * as $metadata from "./targets.metadata";
import { Indexable } from "../src/util";

describe("updateFromModel", () => {
  test("shallow copies properties", () => {
    const original = {
      $metadata: $metadata.Student,
      studentId: 1,
      courses: [{courseId: 1}]
    }

    const update = model.convertToModel({
      studentId: 2,
      courses: [],
    }, $metadata.Student) as Indexable<model.Model<typeof $metadata.Student>>

    const updated = model.updateFromModel(original, update) as Indexable<model.Model<typeof $metadata.Student>>;

    expect(updated).toBe(original);
    expect(updated.studentId).toBe(update.studentId);
    expect(updated.courses).toBe(update.courses);
    expect(updated.courses.length).toBe(0);
  });
});

