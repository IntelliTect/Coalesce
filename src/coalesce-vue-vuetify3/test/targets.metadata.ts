import { BehaviorFlags } from "coalesce-vue";
import {
  ObjectType,
  getEnumMeta,
  ModelType,
  Domain,
  ForeignKeyProperty,
  PrimaryKeyProperty,
  ModelReferenceNavigationProperty,
  solidify,
} from "coalesce-vue";

const metaBase = (name: string = "model") => {
  return {
    name: name,
    displayName: name.substr(0, 1).toUpperCase() + name.substr(1),
  };
};

const value = (name: string = "prop") => {
  return {
    name: name,
    displayName: name.substr(0, 1).toUpperCase() + name.substr(1),
    role: "value",
  };
};

const domain: Domain = { enums: {}, types: {}, services: {} };

export const Course = (domain.types.Course = {
  ...metaBase("Course"),
  type: "model",
  behaviorFlags: 7 as BehaviorFlags,
  get keyProp() {
    return this.props.courseId;
  },
  get displayProp() {
    return this.props.name;
  },
  controllerRoute: "Courses",
  dataSources: {},
  methods: {},
  props: {
    courseId: {
      name: "courseId",
      displayName: "CourseId",
      role: "primaryKey",
      type: "number",
    },
    name: {
      name: "name",
      displayName: "Name",
      role: "value",
      type: "string",
    },
    studentId: {
      name: "studentId",
      displayName: "StudentId",
      type: "number",
      role: "foreignKey",
      get navigationProp() {
        return domain.types.Course.props
          .student as ModelReferenceNavigationProperty;
      },
      get principalType() {
        return domain.types.Student as ModelType;
      },
      get principalKey() {
        return (domain.types.Student as ModelType)
          .keyProp as PrimaryKeyProperty;
      },
    },
    student: {
      name: "student",
      displayName: "Student",
      type: "model",
      role: "referenceNavigation",
      dontSerialize: true,
      get foreignKey() {
        return domain.types.Course.props.studentId as ForeignKeyProperty;
      },
      get principalKey() {
        return (domain.types.Student as ModelType)
          .keyProp as PrimaryKeyProperty;
      },
      get typeDef() {
        return domain.types.Student as ModelType;
      },
    },
  },
});

export const Advisor = (domain.types.Advisor = {
  ...metaBase("Advisor"),
  type: "model",
  behaviorFlags: 7 as BehaviorFlags,
  get keyProp() {
    return this.props.advisorId;
  },
  get displayProp() {
    return this.props.name;
  },
  controllerRoute: "Advisors",
  dataSources: {},
  methods: {},
  props: {
    advisorId: {
      name: "advisorId",
      displayName: "AdvisorId",
      role: "primaryKey",
      type: "number",
    },
    name: {
      name: "name",
      displayName: "Name",
      role: "value",
      type: "string",
      rules: {
        required: (val) => !!val || "Name is required.",
      },
    },
    students: {
      name: "students",
      displayName: "Students",
      role: "collectionNavigation",
      type: "collection",
      dontSerialize: true,
      get foreignKey() {
        return (domain.types.Student as ModelType).props
          .studentAdvisorId as ForeignKeyProperty;
      },
      itemType: {
        name: "$collectionValue",
        role: "value",
        displayName: "",
        type: "model",
        get typeDef() {
          return domain.types.Student as ModelType;
        },
      },
    },
    studentsNonNavigation: {
      // A collection of models that doesn't function as a navigation property.
      name: "studentsNonNavigation",
      displayName: "Students NonNav",
      role: "value",
      type: "collection",
      dontSerialize: true,
      itemType: {
        name: "$collectionValue",
        role: "value",
        displayName: "",
        type: "model",
        get typeDef() {
          return domain.types.Student as ModelType;
        },
      },
    },
    studentWrapperObject: {
      name: "studentWrapperObject",
      displayName: "Student Wrapper Object",
      role: "value",
      type: "object",
      get typeDef() {
        return domain.types.DisplaysStudent as ObjectType;
      },
    },
  },
});

export const Student = (domain.types.Student = {
  ...metaBase("Student"),
  type: "model",
  behaviorFlags: 7 as BehaviorFlags,
  get displayProp() {
    return this.props.name;
  },
  get keyProp() {
    return this.props.studentId;
  },
  controllerRoute: "Students",
  methods: {
    personCount: {
      name: "personCount",
      displayName: "Person Count",
      transportType: "item",
      httpMethod: "GET",
      params: {
        lastNameStartsWith: {
          name: "lastNameStartsWith",
          displayName: "Last Name Starts With",
          type: "string",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "number",
      },
    },
    fullNameAndAge: {
      name: "fullNameAndAge",
      displayName: "Full Name And Age",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          role: "value",
          type: "number",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "string",
      },
    },
    getWithObjParam: {
      name: "getWithObjParam",
      displayName: "getWithObjParam",
      transportType: "item",
      httpMethod: "GET",
      params: {
        advisor: {
          name: "advisor",
          displayName: "Advisor",
          type: "model",
          role: "value",
          typeDef: Advisor,
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        role: "value",
        typeDef: Advisor,
      },
    },
    getFile: {
      name: "getFile",
      displayName: "getFile",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "id",
          type: "number",
          role: "value",
          get source() {
            return (domain.types.Student as ModelType).props.studentId;
          },
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "string",
          role: "value",
          get source() {
            return (domain.types.Student as ModelType).props.name;
          },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "file",
      },
    },
    manyParams: {
      name: "manyParams",
      displayName: "ManyParams",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "id",
          type: "number",
          role: "value",
          get source() {
            return (domain.types.Student as ModelType).props.studentId;
          },
        },
        string: {
          name: "string",
          displayName: "String",
          type: "string",
          role: "value",
        },
        startDate: {
          name: "startDate",
          displayName: "Start Date",
          type: "date",
          dateKind: "datetime",
          role: "value",
        },
        num: {
          name: "num",
          displayName: "Num",
          type: "number",
          role: "value",
        },
        model: {
          name: "model",
          displayName: "Model",
          type: "model",
          get typeDef() {
            return domain.types.Course as ModelType;
          },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "string",
      },
    },
  },
  props: {
    studentId: {
      name: "studentId",
      displayName: "StudentId",
      role: "primaryKey",
      type: "number",
    },
    name: {
      name: "name",
      displayName: "Name",
      role: "value",
      type: "string",
    },
    isEnrolled: {
      name: "isEnrolled",
      displayName: "Is Enrolled",
      role: "value",
      type: "boolean",
    },
    birthDate: {
      name: "birthDate",
      displayName: "BirthDate",
      dateKind: "datetime",
      role: "value",
      type: "date",
    },
    dateNoOffset: {
      name: "dateNoOffset",
      displayName: "Date No Offset",
      dateKind: "datetime",
      noOffset: true,
      role: "value",
      type: "date",
    },
    password: {
      name: "password",
      displayName: "Password",
      type: "string",
      subtype: "password",
      role: "value",
    },
    email: {
      name: "email",
      displayName: "Email",
      type: "string",
      subtype: "email",
      role: "value",
    },
    phone: {
      name: "phone",
      displayName: "Phone",
      type: "string",
      subtype: "tel",
      role: "value",
    },
    color: {
      name: "color",
      displayName: "Color",
      type: "string",
      subtype: "color",
      role: "value",
    },
    notes: {
      name: "notes",
      displayName: "Notes",
      type: "string",
      subtype: "multiline",
      role: "value",
    },
    courses: {
      name: "courses",
      displayName: "Courses",
      role: "collectionNavigation",
      type: "collection",
      dontSerialize: true,
      get foreignKey() {
        return (domain.types.Course as ModelType).props
          .studentId as ForeignKeyProperty;
      },
      itemType: {
        name: "$collectionValue",
        role: "value",
        displayName: "",
        type: "model",
        typeDef: Course,
      },
    },
    currentCourse: {
      name: "currentCourse",
      displayName: "Current Course",
      type: "model",
      role: "referenceNavigation",
      dontSerialize: true,
      description: "The course that the student is currently attending.",
      get foreignKey() {
        return domain.types.Student.props.currentCourseId as ForeignKeyProperty;
      },
      get principalKey() {
        return Course.keyProp as PrimaryKeyProperty;
      },
      typeDef: Course,
    },
    currentCourseId: {
      name: "currentCourseId",
      displayName: "Current Course ID",
      type: "number",
      role: "foreignKey",
      get navigationProp() {
        return domain.types.Student.props
          .currentCourse as ModelReferenceNavigationProperty;
      },
      get principalType() {
        return domain.types.Course as ModelType;
      },
      get principalKey() {
        return Course.keyProp as PrimaryKeyProperty;
      },
      rules: {
        required: (val) => !!val || "Current Course is required.",
      },
    },
    grade: {
      name: "grade",
      displayName: "Grade",
      role: "value",
      type: "enum",
      typeDef: {
        name: "grades",
        displayName: "Grades",
        type: "enum",
        ...getEnumMeta([
          { value: 9, strValue: "Freshman", displayName: "Freshman" },
          { value: 10, strValue: "Sophomore", displayName: "Sophomore" },
          { value: 11, strValue: "Junior", displayName: "Junior" },
          { value: 12, strValue: "Senior", displayName: "Senior" },
        ]),
      },
    },
    advisor: {
      name: "advisor",
      displayName: "Advisor",
      type: "model",
      role: "referenceNavigation",
      dontSerialize: true,
      get foreignKey() {
        return domain.types.Student.props
          .studentAdvisorId as ForeignKeyProperty;
      },
      get principalKey() {
        return Advisor.keyProp as PrimaryKeyProperty;
      },
      typeDef: Advisor,
    },
    // This is intentionally named weirdly as to not match the name of the PK of Advisor.
    // This helps eek out bugs where the wrong props names are used for setting/getting props.
    studentAdvisorId: {
      name: "studentAdvisorId",
      displayName: "StudentAdvisorId",
      type: "number",
      role: "foreignKey",
      get navigationProp() {
        return domain.types.Student.props
          .advisor as ModelReferenceNavigationProperty;
      },
      get principalType() {
        return domain.types.Advisor as ModelType;
      },
      get principalKey() {
        return Advisor.keyProp as PrimaryKeyProperty;
      },
    },
    unknownObj: {
      name: "unknownObj",
      displayName: "Unknown Obj",
      type: "unknown",
      role: "value",
    },
  },
  dataSources: {
    search: {
      type: "dataSource",
      name: "Search",
      displayName: "Search",
      props: {
        nameStart: {
          name: "nameStart",
          displayName: "Name Start",
          type: "string",
          role: "value",
        },
      },
    },
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource",
      displayName: "Default Source",
      isDefault: true,
      props: {},
    },
  },
} as const);

export const DisplaysStudent = (domain.types.DisplaysStudent = <ObjectType>{
  ...metaBase("DisplaysStudent"),
  type: "object",
  get displayProp() {
    return this.props.student;
  },
  props: {
    name: {
      ...value("name"),
      type: "string",
    },
    student: {
      ...value("student"),
      type: "model",
      dontSerialize: true,
      get typeDef() {
        return Student;
      },
    },
  },
});

export const DisplaysStudentSerializesChild = <ObjectType>{
  ...DisplaysStudent,
  props: {
    ...DisplaysStudent.props,
    student: {
      ...DisplaysStudent.props.student,
      dontSerialize: false,
    },
  },
};

interface AppDomain extends Domain {
  enums: {};
  types: {
    Student: typeof Student;
    Advisor: typeof Advisor;
    Course: typeof Course;
    DisplaysStudent: typeof DisplaysStudent;
  };
  services: {};
}

solidify(domain);

export default domain as AppDomain;
