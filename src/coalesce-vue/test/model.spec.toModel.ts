
import * as model from "../src/model";
import * as $metadata from "./shared.metadata";
import { ModelValue, ObjectValue, Value, ObjectType, CollectionValue } from "../src/metadata";
import { shortStringify } from "./test-utils";
import { Indexable } from "../src/util";
import { twoWayConversions, studentValue, MappingData } from "./model.shared";

const studentProps = $metadata.Student.props;

function unparsable(meta: Value, ...values: any[]) {
  return values.map(value => {
    return { meta, dto: value, error: `unparsable .* for ${meta.type} '${meta.name}'` };
  });
}

/** Set of data to run simple mapping tests on.
 * Will be ran against both mapToModel and convertToModel variants.
 */

const dtoToModelMappings = <MappingData[]>[
  ...twoWayConversions,

  // String
  { meta: studentProps.name, dto: 123, model: "123" },
  { meta: studentProps.name, dto: true, model: "true" },
  { meta: studentProps.name, dto: undefined, model: null },
  ...unparsable(
    studentProps.name,
    {},
    []
  ),

  // Number
  { meta: studentProps.studentId, dto: "1", model: 1 },
  ...unparsable(
    studentProps.studentId,
    true,
    "abc",
    {},
    []
  ),

  // Boolean
  { meta: studentProps.isEnrolled, dto: "true", model: true },
  { meta: studentProps.isEnrolled, dto: "false", model: false },
  ...unparsable(
    studentProps.isEnrolled,
    123,
    "abc",
    {},
    []
  ),

  // Enum
  { meta: studentProps.grade, dto: "11", model: 11 },
  // Enums should parse any number - not just valid enum values. This allows for flags enums.
  { meta: studentProps.grade, dto: 123, model: 123 },
  ...unparsable(studentProps.grade, "abc", {}, [], true),

  // Date
  {
    meta: studentProps.birthDate,
    dto: new Date("1990-01-02T03:04:05.000-08:00"),
    model: new Date("1990-01-02T03:04:05.000-08:00")
  },
  ...unparsable(
    studentProps.birthDate,
    "abc",
    123,
    {},
    [],
    true,
    new Date("!!Invalid")
  ),

  // Collection
  ...unparsable(studentProps.courses, "abc", 123, {}, true),

  // Model
  ...unparsable(
    studentProps.advisor,
    "abc",
    123,
    [],
    true
  ),
  {
    meta: studentProps.advisor,
    dto: { advisorId: 1, extraneousProp: true },
    model: { advisorId: 1, $metadata: $metadata.Advisor }
  },

  // Object
  {
    meta: studentValue,
    dto: { studentId: 1, extraneousProp: true },
    model: { studentId: 1, $metadata: $metadata.Student }
  },
  ...unparsable(
    studentValue,
    "abc",
    123,
    [],
    true
  ),
];

// Test for both `map` and `convert` using the cases above.
dtoToModelMappings.forEach(
  ({
    meta: value,
    model: modelValue,
    dto: dtoValue,
    error
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



// Test both `map` and `convert` cases 
// for starting with an object via `<>ToModel` methods.
describe.each(["convertToModel", "mapToModel"])
("%s", (methodName: "convertToModel" | "mapToModel") => {

  test("preserves circular references", () => {
    const childDto = {} as any
    childDto.parent = {
      children: [
        childDto
      ]
    };

    const parentMeta = <ObjectType>{
      type: "object",
      name: "parent", displayName: "parent",
      props: {
        children: <CollectionValue>{
          name: "children", displayName: "",
          role: "value", type: "collection",
          itemType: {
            type: "object",
            name: "$child", displayName: "",
            role: "value"
          }
        }
      }
    }
    const childMeta = <ObjectType>{
      type: "object",
      name: "child", displayName: "child",
      props: {
        parent: <ObjectValue>{
          name: "parent", displayName: "",
          role: "value", type: "object",
          typeDef: parentMeta
        }
      }
    };
    ((parentMeta.props.children as CollectionValue).itemType as any).typeDef = childMeta;

    const mapped = model[methodName](childDto, childMeta);

    // In the new object, the nested circular reference to itself
    // should be the same reference as the root object - the circular relationship
    // should be preserved.
    expect((mapped as Indexable<typeof mapped>).parent.children[0]).toBe(mapped);
  })
})


describe("mapToModel", () => {
  test("ignores mismatched existing metadata", () => {
    const mapped = model.mapToModel({
      studentId: 1,
      $metadata: $metadata.Advisor // intentionally mismatched.
    }, $metadata.Student);

    // Because we're mapping to a new object, we don't care
    // about stuff already on the object that we don't need.
    // Since we don't need existing metadata, and we're not 
    // setting metadata on some existing object,
    // the code should always set the correct medata on the resulting object.
    expect(mapped).toMatchObject({
      studentId: 1,
      $metadata: expect.objectContaining({name: $metadata.Student.name, type: $metadata.Student.type })
    })
  })
})


describe("convertToModel", () => {
  test("rejects mismatched existing metadata", () => {

    // If metadata on the incoming object doesn't match what 
    // the resulting metadata is supposed to be, then something's wrong.
    // We want this to throw instead of blindly changing the metadata
    // of an object to some different type.
    expect(() => {
      model.convertToModel({
        studentId: 1,
        $metadata: $metadata.Advisor // intentionally mismatched.
      }, $metadata.Student)
    }).toThrowError(/found metadata for advisor where metadata for student was expected/i)
  })

  test("converts object, preserving references", () => {
    const dto = {
      studentId: 1,
      birthDate: new Date()
    }
    const converted = model.convertToModel(dto, $metadata.Student) as any;

    // Expect reference equality for both
    expect(converted).toBe(dto);
    expect(converted.birthDate).toBe(dto.birthDate);
    expect(converted.$metadata).toBe($metadata.Student);
  })
})


