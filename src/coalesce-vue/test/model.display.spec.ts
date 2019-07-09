import * as model from "../src/model";
import * as $metadata from "./targets.metadata";
import { ObjectValue, Value } from "../src/metadata";
import { shortStringify } from "./test-utils";
import { format, subYears } from "date-fns";

const studentProps = $metadata.Student.props;

const basicStudent = {
  $metadata: $metadata.Student,
  studentId: 1,
  name: "Bob"
};

describe("modelDisplay", () => {

  test("for object without $metadata, throws", () => {
    expect(() => model.modelDisplay({} as any))
      .toThrowError(/has no \$metadata/)
  });

  test("returns value for value prop", () => {
    const meta = basicStudent.$metadata;
    expect(meta.displayProp).toBe(meta.props.name);

    expect(model.modelDisplay(basicStudent)).toBe(basicStudent.name);
  });

  test("returns value for object prop", () => {
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

describe("display", () => {
  test("returns json for object without displayProp", () => {
    const typeMeta = {
      ...$metadata.DisplaysStudent,
      displayProp: undefined,
    }
    const instance = {
      $metadata: typeMeta,
      name: "Bob",
    }
    expect(model.modelDisplay(instance)).toBe("{\"name\":\"Bob\"}");
  });
});


interface DisplayData {
    meta: Value;
    model?: any;
    display: string | null;
    options?: model.DisplayOptions;
    error?: string;
}

function undisplayable(meta: Value, ...values: any[]) {
  return values.map(value => {
    return { meta, model: value, error: `unparsable .* for ${meta.type} '${meta.name}'`,  } as DisplayData;
  });
}

describe.each(<DisplayData[]>[
  { meta: studentProps.studentId, model: null, display: null },
  { meta: studentProps.studentId, model: undefined, display: null },
  { meta: studentProps.studentId, model: 1, display: "1" },
  { meta: studentProps.studentId, model: "1", display: "1" },
  ...undisplayable(studentProps.studentId, "abc", {}, [], true, new Date() ),

  { meta: studentProps.name, model: null, display: null },
  { meta: studentProps.name, model: undefined, display: null },
  { meta: studentProps.name, model: "Bob", display: "Bob" },
  { meta: studentProps.name, model: 123, display: "123" },
  { meta: studentProps.name, model: true, display: "true" },
  ...undisplayable(studentProps.name, {}, [], new Date() ),

  { meta: studentProps.birthDate, model: null, display: null },
  { meta: studentProps.birthDate, model: undefined, display: null },
  { meta: studentProps.birthDate, model: new Date(1990, 0, 2, 3, 4, 5), display: "1/2/1990 3:04:05 AM" },
  { meta: studentProps.birthDate, model: new Date(1990, 0, 2, 3, 4, 5), display: "1990", options: {format: 'yyyy'} },
  { meta: studentProps.birthDate, model: subYears(new Date(), 2), display: "about 2 years ago", options: {format: {distance: true}} },
  { meta: studentProps.birthDate, model: subYears(new Date(), 2), display: "about 2 years", options: {format: {distance: true, addSuffix: false}} },
  { meta: studentProps.birthDate, 
    model: "1990-01-02T03:04:05.000-08:00", 
    // We define the expected using date-fns's format to make this test timezone-independent.
    display: format(new Date("1990-01-02T03:04:05.000-08:00"), "M/d/yyyy h:mm:ss aaa") },
  ...undisplayable(studentProps.birthDate, true, 123, "abc", {}, [], new Date("!!Invalid") ),
  
  { meta: studentProps.grade, model: undefined, display: null },
  { meta: studentProps.grade, model: null, display: null },
  { meta: studentProps.grade, model: 11, display: "Junior" },
  { meta: studentProps.grade, model: "11", display: "Junior" },
  // Non-explicitly-defined enums should display the number value
  // since there's nothing better to show:
  { meta: studentProps.grade, model: 111, display: "111" },
  ...undisplayable(studentProps.grade, true, "abc", {}, [], new Date() ),

  { meta: studentProps.isEnrolled, model: undefined, display: null },
  { meta: studentProps.isEnrolled, model: null, display: null },
  { meta: studentProps.isEnrolled, model: true, display: "true" },
  { meta: studentProps.isEnrolled, model: "true", display: "true" },
  ...undisplayable(studentProps.isEnrolled, 123, "abc", {}, [], new Date() ),

  // Collection
  { meta: studentProps.courses, model: undefined, display: null },
  { meta: studentProps.courses, model: null, display: null },
  { meta: studentProps.courses, model: [], display: "" },
  {
    meta: studentProps.courses,
    model: ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: name,
        courseId: i
      };
    }),
    display: "CSCD 210, CSCD 211, MATH 301"
  },
  {
    meta: studentProps.courses,
    model: Array(10).map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: "CS10" + i,
        courseId: i
      };
    }),
    display: "10 items"
  },
  ...undisplayable(studentProps.courses, "abc", 123, {}, true, new Date() ),


  { meta: studentProps.advisor, model: null, display: null },
  { meta: studentProps.advisor, model: { advisorId: 1, name: "Steve" }, display: "Steve" },
])("valueDisplay", (x) => {

  const { meta, model: modelValue, display, error, options } = x as typeof x & {error?: string};

  describe(meta.type, () => {

    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(display)}`;

    test(`for ${shortStringify(modelValue)}, ${expectedOutcomeDesc}`, () => {
      const doMap = () => model.valueDisplay(modelValue, meta, options);
      if (error) {
        expect(doMap).toThrowError(new RegExp(error));
        return;
      }
      
      expect(doMap()).toBe(display);
    });
  })
});
