import * as model from "../src/model";
import * as $metadata from "./targets.metadata";
import { Value, ModelType } from "../src/metadata";
import { shortStringify } from "./test-utils";
import { twoWayConversions, studentValue, MappingData } from "./model.shared";

const studentProps = $metadata.Student.props;

function unmappable(meta: Value, ...values: any[]) {
  return values.map((value) => {
    return {
      meta,
      model: value,
      error: `unparsable .* for ${meta.type} '${meta.name}'`,
    };
  });
}

// Test some simple mappings from model to DTO.
describe.each(<MappingData[]>[
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
      courses: [{ $metadata: $metadata.Course, courseId: 1, name: "CS 101" }],
      studentAdvisorId: null,
      advisor: {
        $metadata: $metadata.Advisor,
        advisorId: 1,
        name: "Joe",
      },
    },
    dto: { name: "Steve", studentId: 1, studentAdvisorId: 1 },
  },

  // Serialized child object with a non-null value
  {
    meta: {
      role: "value",
      typeDef: $metadata.DisplaysStudentSerializesChild,
      type: "object",
    },
    model: {
      $metadata: $metadata.DisplaysStudentSerializesChild,
      name: "Steve",
      student: {
        $metadata: $metadata.Student,
        name: "Steve",
      },
    },
    dto: { name: "Steve", student: { name: "Steve" } },
  },
  // Serialized child object with a null value
  {
    meta: {
      role: "value",
      typeDef: $metadata.DisplaysStudentSerializesChild,
      type: "object",
    },
    model: {
      $metadata: $metadata.DisplaysStudentSerializesChild,
      name: "Steve",
      student: null,
    },
    dto: { name: "Steve", student: null },
  },
  // Null object
  { meta: studentValue, model: null, dto: null },

  // String
  { meta: studentProps.name, model: 123, dto: "123" },
  { meta: studentProps.name, model: true, dto: "true" },
  ...unmappable(studentProps.name, new Date(), [], {}),

  // Number
  { meta: studentProps.studentId, model: "123", dto: 123 },
  { meta: studentProps.studentId, model: "", dto: null },
  { meta: studentProps.studentId, model: " ", dto: null },
  ...unmappable(studentProps.studentId, new Date(), [], {}, "abc"),

  // Enum
  { meta: studentProps.grade, model: "123", dto: 123 },
  ...unmappable(studentProps.grade, new Date(), [], {}, "abc"),

  // Boolean
  { meta: studentProps.isEnrolled, model: "true", dto: true },
  { meta: studentProps.isEnrolled, model: "false", dto: false },
  ...unmappable(studentProps.isEnrolled, new Date(), [], {}, "abc", 123),

  // Date
  ...unmappable(
    studentProps.birthDate,
    new Date("!!Invalid"),
    123,
    "abc",
    [],
    {}
  ),
])(
  "mapValueToDto",
  ({ meta: value, model: modelValue, dto: dtoValue, error }) => {
    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(dtoValue)}`;

    const testTitle = `for ${shortStringify(
      modelValue
    )}, ${expectedOutcomeDesc}`;

    describe(value.type, () => {
      test(testTitle, () => {
        const doMap = () => model.mapValueToDto(modelValue, value);
        if (error) {
          expect(doMap).toThrowError(new RegExp(error, "i"));
          return;
        }
        const mapped = doMap();

        if (typeof dtoValue == "object" && dtoValue != null) {
          expect(mapped).toMatchObject(dtoValue);
        } else {
          expect(mapped).toBe(dtoValue);
        }
      });
    });
  }
);

describe("mapToDto", () => {
  test("for object without $metadata, throws", () => {
    expect(() => model.mapToDto({} as any)).toThrowError(/requires metadata/);
  });

  test("for no value, returns null", () => {
    expect(model.mapToDto(null)).toBe(null);
    expect(model.mapToDto(undefined)).toBe(null);
  });

  test("for object, maps object", () => {
    const mapped = model.mapToDto({
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve",
      courses: [{ $metadata: $metadata.Course, courseId: 1, name: "CS 101" }],
      studentAdvisorId: null,
      advisor: {
        $metadata: $metadata.Advisor,
        advisorId: 1,
        name: "Joe",
      },
    });

    expect(mapped).toMatchObject({
      name: "Steve",
      studentId: 1,
      studentAdvisorId: 1,
    });
  });

  test("for object with specific props, drops extra props", () => {
    const mapped = model.mapToDtoFiltered(
      {
        $metadata: $metadata.Student,
        studentId: 1,
        name: "Steve",
        studentAdvisorId: 3,
      },
      ["name", "studentId"]
    );

    expect(mapped).toMatchObject({
      name: "Steve",
      studentId: 1,
    });
  });

  test("for date with offset and defaultTimeZone set, serializes as defaultTimeZone", () => {
    model.setDefaultTimeZone("America/Adak");

    const mapped = model.mapToDto({
      $metadata: $metadata.Student,
      birthDate: new Date("2014-10-25T13:46:20+02:00"),
    }) as any;

    // Output is shifted from UTC+2 to UTC-9, a total of 11 hours (1300 hours to 0200 hours).
    expect(mapped.birthDate).toBe("2014-10-25T02:46:20.000-09:00");
  });
});
