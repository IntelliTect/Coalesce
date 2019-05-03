import { ObjectType, BasicCollectionProperty, getEnumMeta, ObjectProperty, ModelType, ModelCollectionNavigationProperty, ClassType, Domain, ForeignKeyProperty, PrimaryKeyProperty, ModelReferenceNavigationProperty } from "../src/metadata";

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

const domain: Domain = { enums: {}, types: {}, services: {} }

export const Course = domain.types.Course = {
  ...metaBase("Course"),
  type: "model",
  behaviorFlags: 7,
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
      name: 'courseId',
      displayName: 'CourseId',
      role: 'value',
      type: "number"
    },
    name: {
      name: 'name',
      displayName: 'Name',
      role: 'value',
      type: "string"
    }
  }
};

export const Advisor = domain.types.Advisor = {
  ...metaBase("Advisor"),
  type: "model",
  behaviorFlags: 7,
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
      name: 'advisorId',
      displayName: 'AdvisorId',
      role: 'primaryKey',
      type: "number"
    },
    name: {
      name: 'name',
      displayName: 'Name',
      role: 'value',
      type: "string"
    }
  }
};

export const Student = domain.types.Student = {
  ...metaBase("Student"),
  type: "model",
  behaviorFlags: 7,
  get displayProp() {
    return this.props.name;
  },
  get keyProp() {
    return this.props.studentId;
  },
  controllerRoute: "Students",
  dataSources: {},
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
  },
  props: {
    studentId: {
      name: 'studentId',
      displayName: 'StudentId',
      role: 'value',
      type: "number"
    },
    name: {
      name: 'name',
      displayName: 'Name',
      role: 'value',
      type: "string"
    },
    isEnrolled: {
      name: 'isEnrolled',
      displayName: 'IsEnrolled',
      role: 'value',
      type: "boolean"
    },
    birthDate: {
      name: 'birthDate',
      displayName: 'BirthDate',
      role: 'value',
      type: "date"
    },
    courses: {
      name: 'courses',
      displayName: "Courses",
      role: "collectionNavigation",
      type: "collection",
      dontSerialize: true,
      get foreignKey() { return (domain.types.Course as ModelType).props.courseId as ForeignKeyProperty },
      itemType: {
        name: '$collectionValue',
        role: 'value',
        displayName: "",
        type: "model",
        typeDef: Course,
      }
    },
    grade: {
      name: 'grade',
      displayName: 'Grade',
      role: 'value',
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
      get foreignKey() { return domain.types.Student.props.advisorId as ForeignKeyProperty },
      get principalKey() { return Advisor.keyProp as PrimaryKeyProperty },
      typeDef: Advisor
    },
    advisorId: {
      name: "advisorId",
      displayName: "AdvisorId",
      type: "number",
      role: "foreignKey",
      get navigationProp() { return domain.types.Student.props.advisor as ModelReferenceNavigationProperty },
      get principalType() { return domain.types.Advisor as ModelType },
      get principalKey() { return Advisor.keyProp as PrimaryKeyProperty }
    }
  }
};

export const DisplaysStudent = <ObjectType>{
  ...metaBase("DisplaysStudent"),
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

interface AppDomain extends Domain {
  enums: {
  }
  types: {
    Student: typeof Student,
    Advisor: typeof Advisor
    Course: typeof Course,
    DisplaysStudent: typeof DisplaysStudent,
  }
  services: {
  }
}

export default domain as AppDomain