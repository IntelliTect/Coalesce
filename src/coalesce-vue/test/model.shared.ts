
import * as $metadata from "./shared.metadata";
import { ObjectValue, Value } from "../src/metadata";

const studentProps = $metadata.Student.props;

export const studentValue = <ObjectValue>{
  name: "student",
  displayName: "Student",
  role: "value",
  type: "object",
  typeDef: $metadata.Student
};

/** Conversions which map the same in either direction between model and DTOs */
export interface MappingData {
    meta: Value;
    model?: any;
    dto: any;
    error?: string;
}

export const twoWayConversions = <MappingData[]>[
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

  // Collection
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

  // Object
  // valued-props off of the root object
  {
    meta: studentValue,
    model: {
      $metadata: $metadata.Student,
      studentId: 1,
      name: "Steve",
      birthDate: new Date("1990-01-02T03:04:05.000-08:00")
    },
    dto: { 
      name: "Steve", 
      studentId: 1, 
      birthDate: "1990-01-02T03:04:05.000-08:00" 
    }
  },

  // null props off of the root object
  {
    meta: studentValue,
    model: {
      $metadata: $metadata.Student,
      studentId: null,
      name: null,
      birthDate: null,
      isEnrolled: null,
      grade: null,
      courses: null,
    },
    dto: { 
      name: null, 
      studentId: null, 
      birthDate: null,
      isEnrolled: null,
      grade: null,
    }
  },
  // null root objects
  { meta: studentValue, model: null, dto: null },

  // Model (object covers most of the cases here...
  // just need to test the few special branches at the start for model values)
  { meta: studentProps.advisor, model: null, dto: null }
];
