import * as model from "../src/model";
import * as $metadata from "./targets.metadata";
import { Value } from "../src/metadata";
import { shortStringify } from "./test-utils";
import { twoWayConversions, studentValue, MappingData } from "./model.shared";
import { format } from "date-fns";

const studentProps = $metadata.Student.props;


function unmappable(meta: Value, ...values: any[]) {
  return values.map(value => {
    return { meta, model: value, error: `unparsable .* for ${meta.type} '${meta.name}'` };
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

  // String
  { meta: studentProps.name, model: 123, dto: "123" },
  { meta: studentProps.name, model: true, dto: "true" },
  ...unmappable(studentProps.name, new Date(), [], {} ),

  // Number
  { meta: studentProps.studentId, model: "123", dto: 123 },
  ...unmappable(studentProps.studentId, new Date(), [], {}, "abc" ),
  
  // Enum
  { meta: studentProps.grade, model: "123", dto: 123 },
  ...unmappable(studentProps.grade, new Date(), [], {}, "abc" ),

  // Boolean
  { meta: studentProps.isEnrolled, model: "true", dto: true },
  { meta: studentProps.isEnrolled, model: "false", dto: false },
  ...unmappable(studentProps.isEnrolled, new Date(), [], {}, "abc", 123 ),

  // Date
  {
    meta: studentProps.birthDate,
    model: "1990-01-02T03:04:05.000-08:00",
    // We define the expected using date-fns's format to make this test timezone-independent.
    dto: format(new Date("1990-01-02T03:04:05.000-08:00"), "yyyy-MM-dd'T'HH:mm:ss.SSSXXX")
  },
  ...unmappable(studentProps.birthDate, new Date("!!Invalid"), 123, "abc", [], {} ),

])(
  "mapValueToDto",
  ({ meta: value, model: modelValue, dto: dtoValue, error }) => {
    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(dtoValue)}`;

    const testTitle = `for ${shortStringify(modelValue)}, ${expectedOutcomeDesc}`;

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
    expect(() => model.mapToDto({} as any))
      .toThrowError(/has no \$metadata/)
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
      courses: [{$metadata: $metadata.Course, courseId: 1, name: "CS 101"}],
      advisorId: null,
      advisor: {
        $metadata: $metadata.Advisor,
        advisorId: 1,
        name: "Joe",
      }
    });

    expect(mapped).toMatchObject({ 
      name: "Steve", 
      studentId: 1, 
      advisorId: 1, 
    })
  });
});
