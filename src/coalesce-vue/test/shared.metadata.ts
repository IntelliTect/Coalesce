import { ObjectType, BasicCollectionProperty, getEnumMeta, ObjectProperty, ModelType, ModelCollectionNavigationProperty, ClassType } from "../src/metadata";

const metaBase = {
  name: "model",
  displayName: "Model",
  type: "object"
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
  ...metaBase,
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
  ...metaBase,
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

export const Student = Types.Student = <ObjectType>{
  ...metaBase,
  get displayProp() {
    return this.props.name;
  },
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
  ...metaBase,
  get displayProp() {
    return this.props.student;
  },
  props: {
    student: {
      ...value("student"),
      type: "object",
      typeDef: Student
    }
  }
};