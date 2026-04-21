import type {
  ObjectType,
  ObjectValue,
  Value,
  ModelValue,
} from "../src/metadata";
import { format } from "date-fns";
import * as $metadata from "@test-targets/metadata.g";
import * as $models from "@test-targets/models.g";

const cmProps = $metadata.ComplexModel.props;
const semProps = $metadata.StringEnumModel.props;

export const complexModelValue = <ModelValue>{
  name: "complexModel",
  displayName: "Complex Model",
  role: "value",
  type: "model",
  typeDef: $metadata.ComplexModel,
};

/** An ObjectType whose displayProp is a model reference. */
export const DisplaysModel = <ObjectType>{
  name: "DisplaysModel",
  displayName: "Displays Model",
  type: "object",
  get displayProp() {
    return this.props.model;
  },
  props: {
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    model: {
      name: "model",
      displayName: "Model",
      type: "model",
      role: "value",
      dontSerialize: true,
      get typeDef() {
        return $metadata.ComplexModel;
      },
    },
  },
};

/** Same as DisplaysModel, but the child model prop is serializable. */
export const DisplaysModelSerializesChild = <ObjectType>{
  ...DisplaysModel,
  props: {
    ...DisplaysModel.props,
    model: {
      ...DisplaysModel.props.model,
      dontSerialize: false,
    },
  },
};

export const displaysModelValue = <ObjectValue>{
  name: "displaysModel",
  displayName: "Displays Model",
  role: "value",
  type: "object",
  typeDef: DisplaysModel,
};

const unknownValue = <Value>{
  name: "unknownObj",
  displayName: "Unknown Obj",
  type: "unknown",
  role: "value",
};

/** Conversions which map the same in either direction between model and DTOs */
export interface MappingData {
  meta: Value;
  model?: any;
  dto: any;
  error?: string;
}

export const twoWayConversions = <MappingData[]>[
  { meta: cmProps.dateTimeOffset, model: null, dto: null },
  {
    meta: cmProps.dateTimeOffset,
    model: new Date("1990-01-02T03:04:05.000-08:00"),
    // We define the expected using date-fns's format to make this test timezone-independent.
    dto: format(
      new Date("1990-01-02T03:04:05.000-08:00"),
      "yyyy-MM-dd'T'HH:mm:ss.SSSXXX",
    ),
  },
  {
    meta: cmProps.dateTime,
    model: new Date("1990-01-02T03:04:05.000-08:00"),
    // We define the expected using date-fns's format to make this test timezone-independent.
    dto: format(
      new Date("1990-01-02T03:04:05.000-08:00"),
      "yyyy-MM-dd'T'HH:mm:ss.SSS",
    ),
  },
  { meta: cmProps.name, model: null, dto: null },
  { meta: cmProps.name, model: "Bob", dto: "Bob" },
  { meta: cmProps.complexModelId, model: null, dto: null },
  { meta: cmProps.complexModelId, model: 1, dto: 1 },
  { meta: cmProps.enumWithDefault, model: null, dto: null },
  { meta: cmProps.enumWithDefault, model: 10, dto: 10 },
  // String-serialized enum: values are strings both ways
  { meta: semProps.stringEnum, model: null, dto: null },
  { meta: semProps.stringEnum, model: "FirstValue", dto: "FirstValue" },
  { meta: semProps.stringEnum, model: "SecondValue", dto: "SecondValue" },
  { meta: cmProps.isActive, model: null, dto: null },
  { meta: cmProps.isActive, model: true, dto: true },
  { meta: cmProps.isActive, model: false, dto: false },

  // Collection
  { meta: cmProps.tests, model: null, dto: null },
  { meta: cmProps.tests, model: [], dto: [] },
  {
    meta: cmProps.tests,
    model: ["CSCD 210", "CSCD 211", "MATH 301"].map((testName, i) => {
      return {
        $metadata: $metadata.Test,
        testName: testName,
        testId: i,
        complexModelId: null,
        complexModel: null,
      };
    }),
    dto: ["CSCD 210", "CSCD 211", "MATH 301"].map((testName, i) => {
      return {
        testName: testName,
        testId: i,
        complexModelId: null,
      };
    }),
  },

  // Model
  // valued-props off of the root object
  {
    meta: complexModelValue,
    model: {
      $metadata: $metadata.ComplexModel,
      complexModelId: 1,
      name: "Steve",
      dateTimeOffset: new Date("1990-01-02T03:04:05.000-08:00"),
    },
    dto: {
      name: "Steve",
      complexModelId: 1,
      // We define the expected using date-fns's format to make this test timezone-independent.
      dateTimeOffset: format(
        new Date("1990-01-02T03:04:05.000-08:00"),
        "yyyy-MM-dd'T'HH:mm:ss.SSSXXX",
      ),
    },
  },
  {
    // Expected base type, but with actual derived type.
    // Should round-trip as the derived type.
    meta: $metadata.AbstractModel,
    dto: { $type: "AbstractImpl1", id: 1, impl1OnlyField: "foo" },
    model: new $models.AbstractImpl1({ id: 1, impl1OnlyField: "foo" }),
  },

  // Unknown
  {
    meta: unknownValue,
    dto: { anyProp: "bob", num: 2, anyOtherProp: null },
    model: { anyProp: "bob", num: 2, anyOtherProp: null },
  },

  // null props off of the root object
  {
    meta: complexModelValue,
    model: {
      $metadata: $metadata.ComplexModel,
      complexModelId: null,
      name: null,
      dateTimeOffset: null,
      isActive: null,
      enumWithDefault: null,
      tests: null,
    },
    dto: {
      name: null,
      complexModelId: null,
      dateTimeOffset: null,
      isActive: null,
      enumWithDefault: null,
    },
  },
  // null root objects
  { meta: complexModelValue, model: null, dto: null },

  // Object (model covers most of the cases here...
  // just need to test the few special branches at the start for model values)
  { meta: displaysModelValue, model: null, dto: null },
];
