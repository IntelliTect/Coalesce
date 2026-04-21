import * as model from "../src/model";
import * as $metadata from "@test-targets/metadata.g";
import type { Value } from "../src/metadata";
import { shortStringify } from "./test-utils";
import {
  twoWayConversions,
  complexModelValue,
  DisplaysModelSerializesChild,
  type MappingData,
} from "./model.shared";

const cmProps = $metadata.ComplexModel.props;

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
    meta: complexModelValue,
    model: {
      $metadata: $metadata.ComplexModel,
      complexModelId: 1,
      name: "Steve",
      tests: [{ $metadata: $metadata.Test, testId: 1, testName: "CS 101" }],
      singleTestId: null,
      singleTest: {
        $metadata: $metadata.Test,
        testId: 1,
        testName: "Joe",
      },
    },
    dto: { name: "Steve", complexModelId: 1, singleTestId: 1 },
  },

  // Serialized child object with a non-null value
  {
    meta: {
      role: "value",
      typeDef: DisplaysModelSerializesChild,
      type: "object",
    },
    model: {
      $metadata: DisplaysModelSerializesChild,
      name: "Steve",
      model: {
        $metadata: $metadata.ComplexModel,
        name: "Steve",
      },
    },
    dto: { name: "Steve", model: { name: "Steve" } },
  },
  // Serialized child object with a null value
  {
    meta: {
      role: "value",
      typeDef: DisplaysModelSerializesChild,
      type: "object",
    },
    model: {
      $metadata: DisplaysModelSerializesChild,
      name: "Steve",
      model: null,
    },
    dto: { name: "Steve", model: null },
  },
  // Null object
  { meta: complexModelValue, model: null, dto: null },

  // String
  { meta: cmProps.name, model: 123, dto: "123" },
  { meta: cmProps.name, model: true, dto: "true" },
  ...unmappable(cmProps.name, new Date(), [], {}),

  // Number
  { meta: cmProps.complexModelId, model: "123", dto: 123 },
  { meta: cmProps.complexModelId, model: "", dto: null },
  { meta: cmProps.complexModelId, model: " ", dto: null },
  ...unmappable(cmProps.complexModelId, new Date(), [], {}, "abc"),

  // Enum
  { meta: cmProps.enumWithDefault, model: "123", dto: 123 },
  // String enum member names should resolve to their numeric value
  { meta: cmProps.enumWithDefault, model: "Value10", dto: 10 },
  ...unmappable(cmProps.enumWithDefault, new Date(), [], {}, "abc"),

  // String-serialized enum: string values pass through as strings
  { meta: cmProps.stringEnum, model: "FirstValue", dto: "FirstValue" },
  // Numeric values are converted to string enum member names
  { meta: cmProps.stringEnum, model: 1, dto: "FirstValue" },
  // Numeric strings should also resolve to string enum member names
  { meta: cmProps.stringEnum, model: "1", dto: "FirstValue" },
  // Unknown string values pass through (server may have newer enum members)
  { meta: cmProps.stringEnum, model: "UnknownValue", dto: "UnknownValue" },
  ...unmappable(cmProps.stringEnum, new Date(), [], {}),

  // Boolean
  { meta: cmProps.isActive, model: "true", dto: true },
  { meta: cmProps.isActive, model: "false", dto: false },
  ...unmappable(cmProps.isActive, new Date(), [], {}, "abc", 123),

  // Date
  ...unmappable(
    cmProps.dateTimeOffset,
    new Date("!!Invalid"),
    123,
    "abc",
    [],
    {},
  ),
])(
  "mapValueToDto",
  ({ meta: value, model: modelValue, dto: dtoValue, error }) => {
    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(dtoValue)}`;

    const testTitle = `for ${shortStringify(
      modelValue,
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
  },
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
      $metadata: $metadata.ComplexModel,
      complexModelId: 1,
      name: "Steve",
      tests: [{ $metadata: $metadata.Test, testId: 1, testName: "CS 101" }],
      singleTestId: null,
      singleTest: {
        $metadata: $metadata.Test,
        testId: 1,
        testName: "Joe",
      },
    });

    expect(mapped).toMatchObject({
      name: "Steve",
      complexModelId: 1,
      singleTestId: 1,
    });
  });

  test("for object with specific props, drops extra props", () => {
    const mapped = model.mapToDtoFiltered(
      {
        $metadata: $metadata.ComplexModel,
        complexModelId: 1,
        name: "Steve",
        singleTestId: 3,
      },
      ["name", "complexModelId"],
    );

    expect(mapped).toMatchObject({
      name: "Steve",
      complexModelId: 1,
    });
  });

  test("for date with offset and defaultTimeZone set, serializes as defaultTimeZone", () => {
    model.setDefaultTimeZone("America/Adak");

    const mapped = model.mapToDto({
      $metadata: $metadata.ComplexModel,
      dateTimeOffset: new Date("2014-10-25T13:46:20+02:00"),
    }) as any;

    // Output is shifted from UTC+2 to UTC-9, a total of 11 hours (1300 hours to 0200 hours).
    expect(mapped.dateTimeOffset).toBe("2014-10-25T02:46:20.000-09:00");
  });
});
