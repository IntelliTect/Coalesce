import * as model from "@/model";
import * as $metadata from "./shared.metadata";
import { ModelValue, ObjectValue, Value } from "@/metadata";

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


const twoWayConversions = [
  {meta: $metadata.Student.props.birthDate, model: null, dto: null },
  {
    meta: $metadata.Student.props.birthDate,
    model: new Date("1990-01-02T03:04:05.000-08:00"),
    dto: "1990-01-02T03:04:05.000-08:00"
  },
  {meta: $metadata.Student.props.name, model: null, dto: null},
  {meta: $metadata.Student.props.name, model: "Bob", dto: "Bob"},
  {meta: $metadata.Student.props.studentId, model: null, dto: null},
  {meta: $metadata.Student.props.studentId, model: 1, dto: 1},
  {meta: $metadata.Student.props.grade, model: null, dto: null},
  {meta: $metadata.Student.props.grade, model: 11, dto: 11},
  {meta: $metadata.Student.props.isEnrolled, model: null, dto: null},
  {meta: $metadata.Student.props.isEnrolled, model: true, dto: true},
  {meta: $metadata.Student.props.isEnrolled, model: false, dto: false},
  {meta: $metadata.Student.props.courses, model: null, dto: null},
  {meta: $metadata.Student.props.courses, model: [], dto: []},
  {
    meta: $metadata.Student.props.courses,
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
    meta: <ObjectValue>{
      name: "student",
      displayName: "Student",
      role: "value",
      type: "object",
      typeDef: $metadata.Student
    },
    model: {
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve",
    },
    dto: { name: "Steve", studentId: 1 }
  },

  {
    meta: <ObjectValue>{
      name: "student",
      displayName: "Student",
      role: "value",
      type: "object",
      typeDef: $metadata.Student
    },
    model: null, dto: null
  }
];

const shortStringify = (value: any) => {
  let str = JSON.stringify(value)
  const maxLen = 40;
  if (str.length > maxLen) {
    str = str.substr(0, maxLen) + "..."
  }
  return str;
};

describe.each([
  ...twoWayConversions
])("mapValueToDto", ({meta: value, model: modelValue, dto: dtoValue}) => {
  describe(value.type, () => {
    test(`for ${shortStringify(modelValue)}, returns ${shortStringify(dtoValue)}`, () => {
      const mapped = model.mapValueToDto(modelValue, value)

      if (typeof dtoValue == "object" && dtoValue != null) {
        expect(mapped).toMatchObject(dtoValue);
      } else {
        expect(mapped).toBe(dtoValue);
      }
    });
  });
})

function unparsable(meta: Value, error: string, ...values: any[]){
  return values.map(value => {
    return { meta, dto: value, error }
  })
}
describe.each([
  ...twoWayConversions,
  {meta: $metadata.Student.props.name, model: "123", dto: 123},
  {meta: $metadata.Student.props.name, model: "[object Object]", dto: {}},
  {meta: $metadata.Student.props.name, model: "", dto: []},

  {meta: $metadata.Student.props.studentId, model: 1, dto: "1"},
  ...unparsable($metadata.Student.props.studentId, "unparsable number", 
    true, "abc", {}, []),

  {meta: $metadata.Student.props.isEnrolled, model: true, dto: "true", }, 
  {meta: $metadata.Student.props.isEnrolled, model: false, dto: "false", }, 
  ...unparsable($metadata.Student.props.isEnrolled, "unparsable boolean", 
    123, "abc", {}, []),

  {meta: $metadata.Student.props.grade, model: 11, dto: "11"},
  // Enums should parse any number - not just valid enum values. This allows for flags enums.
  {meta: $metadata.Student.props.grade, model: 123, dto: 123, }, 
  ...unparsable($metadata.Student.props.grade, "unparsable enum", 
    "abc", {}, [], true),
  
  ...unparsable($metadata.Student.props.birthDate, "unparsable date", 
    "abc", 123, {}, [], true),

  ...unparsable($metadata.Student.props.courses, "was not an array", 
    "abc", 123, {}, true),

  ...unparsable($metadata.Student.props.advisor, "was not an object", 
    "abc", 123, [], true),

    
  {
    meta: <ObjectValue>{
      name: "student",
      displayName: "Student",
      role: "value",
      type: "object",
      typeDef: $metadata.Student
    },
    model: { studentId: 1, $metadata: $metadata.Student }, 
    dto: { studentId: 1, extraneousProp: true }
  }

])("dto --> model", ({meta: value, model: modelValue, dto: dtoValue, error}: {meta: Value, model: any, dto: any, error?: string}) => {
  const expectedOutcomeDesc = error
    ? `throws /${error}/`
    : `returns ${shortStringify(modelValue)}`

  const testTitle = `for ${shortStringify(dtoValue)}, ${expectedOutcomeDesc}`

  describe("mapValueToModel", () => {
    describe(value.type, () => {
      test(testTitle, () => {
        
        const doMap = () => model.mapValueToModel(dtoValue, value);
        if (error){
          expect(doMap).toThrowError(new RegExp(error))
          return;
        }
        const mapped = doMap()

        if (typeof modelValue == "object" && modelValue != null) {
          expect(mapped).toMatchObject(modelValue);
        } else {
          expect(mapped).toBe(modelValue);
        }
      });
    });
  })

  describe("convertValueToModel", () => {
    describe(value.type, () => {
      test(testTitle, () => {

        const doMap = () => model.convertValueToModel(dtoValue, value);
        if (error){
          expect(doMap).toThrowError(new RegExp(error))
          return;
        }

        const mapped = doMap()

        if ((typeof modelValue == "object") && modelValue != null) {
          if (typeof dtoValue == "object") {
            // When both side are objects, check for === equality with the input,
            // since `convertValueToModel` mutates its input - it shouldn't be
            // returning new objects/arrays.
            // Dates won't enter this case because typeof(new Date()) == "object", but the date DTO value is a string.
            expect(mapped).toBe(dtoValue);
          }
          expect(mapped).toMatchObject(modelValue);
        } else {
          expect(mapped).toBe(modelValue);
        }
      });
    });
  })
});
