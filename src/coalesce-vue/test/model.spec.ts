import * as model from "../src/model";
import * as $metadata from "./shared.metadata";
import { ModelValue, ObjectValue, Value } from "../src/metadata";
import { shortStringify } from "./test-utils";

const studentProps = $metadata.Student.props;
const studentValue = <ObjectValue>{
  name: "student",
  displayName: "Student",
  role: "value",
  type: "object",
  typeDef: $metadata.Student
};

const basicStudent = {
  $metadata: $metadata.Student,
  studentId: 1,
  name: "Bob"
};

describe("modelDisplay - returns value for", () => {
  test("value prop", () => {
    const meta = basicStudent.$metadata;
    expect(meta.displayProp).toBe(meta.props.name);

    expect(model.modelDisplay(basicStudent)).toBe(basicStudent.name);
  });

  test("object prop", () => {
    expect(
      model.modelDisplay({
        $metadata: $metadata.DisplaysStudent,
        student: basicStudent
      })
    ).toBe(basicStudent.name);
  });
});

describe("propDisplay - resolves property", () => {
  test("by name", () => {
    expect(model.propDisplay(basicStudent, "name")).toBe(basicStudent.name);
  });

  test("by object", () => {
    expect(model.propDisplay(basicStudent, studentProps.name)).toBe(
      basicStudent.name
    );
  });
});

describe.each([
  ["studentId", null, null],
  ["studentId", 1, "1"],
  ["name", null, null],
  ["name", "Bob", "Bob"],
  ["birthDate", null, null],
  ["birthDate", new Date(1990, 0, 2, 3, 4, 5), "1990-1-2 03:04:05"],
  ["grade", null, null],
  ["grade", 11, "Junior"],
  // Non-explicitly-defined enums should display the number value
  // since there's nothing better to show:
  ["grade", 111, "111"],
  ["isEnrolled", null, null],
  ["isEnrolled", true, "true"],
  ["courses", null, null],
  ["courses", [], ""],
  [
    "courses",
    ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: name,
        courseId: i
      };
    }),
    "CSCD 210, CSCD 211, MATH 301"
  ],
  [
    "courses",
    Array(10).map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: "CS10" + i,
        courseId: i
      };
    }),
    "10 items"
  ],
  ["advisor", null, null],
  ["advisor", { advisorId: 1, name: "Steve" }, "Steve"]
])("propDisplay - displays property", (propName, value, expected) => {
  const prop = studentProps[propName];

  test(`for ${prop.type} ${shortStringify(value)}, returns ${shortStringify(
    expected
  )}`, () => {
    expect(
      model.propDisplay(
        {
          ...basicStudent,
          [propName]: value
        },
        propName
      )
    ).toBe(expected);
  });
});

describe("mapToModel", () => {
  // TODO
});

describe("updateFromModel", () => {
  // TODO
});

describe("mapToDto", () => {
  // TODO
});

/** Conversions which map the same in either direction between model and DTOs */
const twoWayConversions = [
  { meta: studentProps.birthDate, model: null, dto: null },
  {
    meta: studentProps.birthDate,
    model: new Date("1990-01-02T03:04:05.000-08:00"),
    dto: "1990-01-02T03:04:05.000-08:00"
  },
  { meta: studentProps.name, model: null, dto: null },
  { meta: studentProps.name, model: "Bob", dto: "Bob" },
  { meta: studentProps.studentId, model: null, dto: null },
  { meta: studentProps.studentId, model: 1, dto: 1 },
  { meta: studentProps.grade, model: null, dto: null },
  { meta: studentProps.grade, model: 11, dto: 11 },
  { meta: studentProps.isEnrolled, model: null, dto: null },
  { meta: studentProps.isEnrolled, model: true, dto: true },
  { meta: studentProps.isEnrolled, model: false, dto: false },
  { meta: studentProps.courses, model: null, dto: null },
  { meta: studentProps.courses, model: [], dto: [] },
  {
    meta: studentProps.courses,
    model: ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: name,
        courseId: i
      };
    }),
    dto: ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        name: name,
        courseId: i
      };
    })
  },

  {
    meta: studentValue,
    model: {
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve"
    },
    dto: { name: "Steve", studentId: 1 }
  },

  { meta: studentValue, model: null, dto: null }
];

/**
 * MODEL --> DTO TESTS
 */
describe.each([
  ...twoWayConversions,
  
  // Ensure child objects and collections are dropped.
  // Also test that foreign keys are auto-populated
  // from their reference prop if the FK itself is null.
  {
    meta: studentValue,
    model: {
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve",
      courses: [{$metadata: $metadata.Course, courseId: 1, name: "CS 101"}],
      advisorId: null,
      advisor: {
        $metadata: $metadata.Advisor,
        advisorId: 1,
        name: "Joe",
      }
    },
    dto: { name: "Steve", studentId: 1, advisorId: 1, }
  },
])(
  "mapValueToDto",
  ({ meta: value, model: modelValue, dto: dtoValue }) => {
    describe(value.type, () => {
      test(`for ${shortStringify(modelValue)}, returns ${shortStringify(
        dtoValue
      )}`, () => {
        const mapped = model.mapValueToDto(modelValue, value);

        if (typeof dtoValue == "object" && dtoValue != null) {
          expect(mapped).toMatchObject(dtoValue);
        } else {
          expect(mapped).toBe(dtoValue);
        }
      });
    });
  }
);

function unparsable(meta: Value, error: string, ...values: any[]) {
  return values.map(value => {
    return { meta, dto: value, error };
  });
}

/**
 * DTO --> MODEL TESTS
 */
describe.each([
  ...twoWayConversions,
  { meta: studentProps.name, dto: 123, model: "123" },
  { meta: studentProps.name, dto: {}, model: "[object Object]" },
  { meta: studentProps.name, dto: [], model: "" },

  { meta: studentProps.studentId, dto: "1", model: 1 },
  ...unparsable(
    studentProps.studentId,
    "unparsable number",
    true,
    "abc",
    {},
    []
  ),

  { meta: studentProps.isEnrolled, dto: "true", model: true },
  { meta: studentProps.isEnrolled, dto: "false", model: false },
  ...unparsable(
    studentProps.isEnrolled,
    "unparsable boolean",
    123,
    "abc",
    {},
    []
  ),

  { meta: studentProps.grade, dto: "11", model: 11 },
  // Enums should parse any number - not just valid enum values. This allows for flags enums.
  { meta: studentProps.grade, dto: 123, model: 123 },
  ...unparsable(studentProps.grade, "unparsable enum", "abc", {}, [], true),

  // Date
  {
    meta: studentProps.birthDate,
    dto: new Date("1990-01-02T03:04:05.000-08:00"),
    model: new Date("1990-01-02T03:04:05.000-08:00")
  },
  ...unparsable(
    studentProps.birthDate,
    "unparsable date",
    "abc",
    123,
    {},
    [],
    true
  ),

  // Collection
  ...unparsable(studentProps.courses, "was not an array", "abc", 123, {}, true),

  // Model
  ...unparsable(
    studentProps.advisor,
    "was not an object",
    "abc",
    123,
    [],
    true
  ),

  // Object
  {
    meta: studentValue,
    dto: { studentId: 1, extraneousProp: true },
    model: { studentId: 1, $metadata: $metadata.Student }
  }
])(
  "dto --> model",
  ({
    meta: value,
    model: modelValue,
    dto: dtoValue,
    error
  }: {
    meta: Value;
    model: any;
    dto: any;
    error?: string;
  }) => {
    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(modelValue)}`;

    const testTitle = `for ${shortStringify(dtoValue)}, ${expectedOutcomeDesc}`;

    describe("mapValueToModel", () => {
      describe(value.type, () => {
        test(testTitle, () => {
          const doMap = () => model.mapValueToModel(dtoValue, value);
          if (error) {
            expect(doMap).toThrowError(new RegExp(error));
            return;
          }
          const mapped = doMap();

          if (typeof modelValue == "object" && modelValue != null) {
            // Expected model is an object, and not a value type.
            // Deep-compare the object.

            // Prevent stack overflows (CTRL+F: "Maximum call stack exceeded") due to circular metadata
            // by replacing expected $metadata objects with jest magic.
            // Currently, this is only replacing at the top-level.
            // If these tests get more intricate, we might need to get fancier.
            if (modelValue.$metadata) {
              modelValue = {
                ...modelValue,
                $metadata: expect.objectContaining({name: modelValue.$metadata.name}),
              }
            }

            expect(mapped).toMatchObject(modelValue);
          } else {
            expect(mapped).toBe(modelValue);
          }
        });
      });
    });

    describe("convertValueToModel", () => {
      describe(value.type, () => {
        test(testTitle, () => {
          const doMap = () => model.convertValueToModel(dtoValue, value);
          if (error) {
            expect(doMap).toThrowError(new RegExp(error));
            return;
          }

          const mapped = doMap();

          if (typeof modelValue == "object" && modelValue != null) {
            if (typeof dtoValue == "object") {
              // When both side are objects, check for === equality with the input,
              // since `convertValueToModel` mutates its input - it shouldn't be
              // returning new objects/arrays.
              // Dates won't enter this case when mapping from a string, because
              // the date DTO value is a string - this behavior is desired and in fact required.
              expect(mapped).toBe(dtoValue);
            }
            expect(mapped).toMatchObject(modelValue);
          } else {
            expect(mapped).toBe(modelValue);
          }
        });
      });
    });
  }
);
