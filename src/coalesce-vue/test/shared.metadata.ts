import { ObjectType, BasicCollectionProperty, getEnumMeta, ObjectProperty, ModelType, ModelCollectionNavigationProperty } from "@/metadata";

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

export const Course = <ModelType>{
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

export const Student = <ObjectType>{
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
    }
  }
};

// Have to set this up like this because its self-referential,
// and jest does weird things to file-scoped vars.
Student.props.friend = <ObjectProperty>{
  ...value("friend"),
  type: "object",
  typeDef: Student
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