import { ObjectType, BasicCollectionProperty, getEnumMeta, ObjectProperty, ModelType, ModelCollectionNavigationProperty, ClassType } from "../src/metadata";

const metaBase = (name: string = "model") => { 
  return {
    name: name,
    displayName: name.substr(0, 1).toUpperCase() + name.substr(1)
  };
};

const value = (name: string = "prop") => {
  return {
    name: name,
    displayName: name.substr(0, 1).toUpperCase() + name.substr(1),
    role: "value"
  };
};

const Types: { [key: string]: ClassType } = {};
export default Types;

export const Course = Types.Course = <ModelType>{
  ...metaBase("course"),
  type: "model",
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
      ...value("courseId"),
      type: "number"
    },
    name: {
      ...value("name"),
      type: "string"
    }
  }
};

export const Advisor = Types.Advisor = <ModelType>{
  ...metaBase("advisor"),
  type: "model",
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
      ...value("advisorId"),
      type: "number"
    },
    name: {
      ...value("name"),
      type: "string"
    }
  }
};

export const Student = Types.Student = <ModelType>{
  ...metaBase("student"),
  type: "model",
  get displayProp() {
    return this.props.name;
  },
  get keyProp() {
    return this.props.studentId;
  },
  controllerRoute: "Students",
  dataSources: {},
  methods: {},
  props: {
    studentId: {
      ...value("studentId"),
      type: "number"
    },
    name: {
      ...value("name"),
      type: "string"
    },
    isEnrolled: {
      ...value("isEnrolled"),
      type: "boolean"
    },
    birthDate: {
      ...value("birthDate"),
      type: "date"
    },
    courses: <ModelCollectionNavigationProperty>{
      ...value("courses"),
      role: "collectionNavigation",
      type: "collection",
      dontSerialize: true,
      itemType: {
        ...value("$collectionValue"),
        type: "model",
        typeDef: Course,
      }
    },
    grade: {
      ...value("grade"),
      type: "enum",
      typeDef: {
        name: "grades",
        displayName: "Grades",
        type: "enum",
        ...getEnumMeta([
          { value: 9, strValue: "Freshman", displayName: "Freshman" },
          { value: 10, strValue: "Sophomore", displayName: "Sophomore" },
          { value: 11, strValue: "Junior", displayName: "Junior" },
          { value: 12, strValue: "Senior", displayName: "Senior" }
        ])
      }
    },
    advisor: {
      name: "advisor",
      displayName: "Advisor",
      type: "model",
      role: "referenceNavigation",
      dontSerialize: true,
      get foreignKey() { return Types.Student.props.advisorId },
      get principalKey() { return Advisor.keyProp },
      typeDef: Advisor
    },
    advisorId: {
      name: "advisorId",
      displayName: "AdvisorId",
      type: "number",
      role: "foreignKey",
      get navigationProp() { return Types.Student.props.advisor },
      get principalType() { return Types.Advisor },
      get principalKey() { return Advisor.keyProp }
    }
  }
};

export const DisplaysStudent = <ObjectType>{
  ...metaBase("displaysStudent"),
  type: "object",
  get displayProp() {
    return this.props.student;
  },
  props: {
    name: {
      ...value("name"),
      type: "string"
    },
    student: {
      ...value("student"),
      type: "model",
      dontSerialize: true,
      typeDef: Student
    }
  }
};