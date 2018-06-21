import * as model from "@/model";
import * as $metadata from "./shared.metadata";

const basicStudent = {
  $metadata: $metadata.Student,
  studentId: 1,
  name: "Bob"
};

describe("modelDisplay", () => {
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
    expect(model.propDisplay(basicStudent, $metadata.Student.props.name)).toBe(
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
  ["friend", null, null],
  ["friend", { ...basicStudent, name: "Steve" }, "Steve"]
])("propDisplay - displays property", (propName, value, expected) => {
  const prop = $metadata.Student.props[propName];

  test(`for ${prop.type} ${value}, returns ${expected}`, () => {
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

describe("convertToModel", () => {
  // TODO
});

describe("convertValueToModel", () => {
  // TODO
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

describe.each([
  ["birthDate", null, null],
  [
    "birthDate",
    new Date("1990-01-02T03:04:05.000-08:00"),
    "1990-01-02T03:04:05.000-08:00"
  ],
  ["name", null, null],
  ["name", "Bob", "Bob"],
  ["studentId", null, null],
  ["studentId", 1, 1],
  ["grade", null, null],
  ["grade", 11, 11],
  ["courses", null, null],
  ["courses", [], []],
  [
    "courses",
    ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        $metadata: $metadata.Course,
        name: name,
        courseId: i
      };
    }),
    ["CSCD 210", "CSCD 211", "MATH 301"].map((name, i) => {
      return {
        name: name,
        courseId: i
      };
    })
  ],

  [
    "friend",
    {
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve",
      friend: { ...basicStudent, name: "Joe" },
      courses: [{ $metadata: $metadata.Course, name: "CS 101", courseId: 1 }]
    },
    { name: "Steve", studentId: 1 }
  ]
])("mapValueToDto", (propName: string, value, expected) => {
  // First data value here is a property name, but this could just as easily be
  // an actual `Value` metadata object. We're just using properties on Student
  // because they exist and are easy to use here.
  const prop = $metadata.Student.props[propName];

  test(`for ${prop.type} ${value}, returns ${expected}`, () => {
    if (typeof expected == "object" && expected != null) {
      expect(model.mapValueToDto(value, prop)).toMatchObject(expected);
    } else {
      expect(model.mapValueToDto(value, prop)).toBe(expected);
    }
  });
});
