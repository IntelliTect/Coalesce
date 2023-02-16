import * as metadata from "./targets.metadata";
import { Model, DataSource, convertToModel, mapToModel } from "coalesce-vue";

export enum Grade {
  Freshman = 9,
  Sophomore = 10,
  Junior = 11,
  Senior = 12,
}

export interface Student extends Model<typeof metadata.Student> {
  studentId: number | null;
  name: string | null;
  isEnrolled: boolean | null;
  birthDate: Date | null;
  courses: Course[] | null;
  currentCourseId: number | null;
  currentCourse: Course | null;
  grade: Grade | null;
  advisor: Advisor | null;
  studentAdvisorId: number | null;
  password: string | null;
  email: string | null;
  phone: string | null;
  color: string | null;
  notes: string | null;
}
export class Student {
  /** Mutates the input object and its descendents into a valid Student implementation. */
  static convert(data?: Partial<Student>): Student {
    return convertToModel(data || {}, metadata.Student);
  }

  /** Maps the input object and its descendents to a new, valid Student implementation. */
  static map(data?: Partial<Student>): Student {
    return mapToModel(data || {}, metadata.Student);
  }

  /** Instantiate a new Student, optionally basing it on the given data. */
  constructor(data?: Partial<Student> | { [k: string]: any }) {
    Object.assign(this, Student.map(data || {}));
  }
}

export namespace Student {
  export namespace DataSources {
    export class Search
      implements DataSource<typeof metadata.Student.dataSources.search>
    {
      readonly $metadata = metadata.Student.dataSources.search;
      nameStart: string | null = null;
    }

    export class DefaultSource
      implements DataSource<typeof metadata.Student.dataSources.defaultSource>
    {
      readonly $metadata = metadata.Student.dataSources.defaultSource;
    }
  }
}

export interface Advisor extends Model<typeof metadata.Advisor> {
  advisorId: number | null;
  name: string | null;
  students: Student[];
  studentsNonNavigation: Student[];
  studentWrapperObject: DisplaysStudent | null;
}
export class Advisor {
  /** Mutates the input object and its descendents into a valid Advisor implementation. */
  static convert(data?: Partial<Advisor>): Advisor {
    return convertToModel(data || {}, metadata.Advisor);
  }

  /** Maps the input object and its descendents to a new, valid Advisor implementation. */
  static map(data?: Partial<Advisor>): Advisor {
    return mapToModel(data || {}, metadata.Advisor);
  }

  /** Instantiate a new Advisor, optionally basing it on the given data. */
  constructor(data?: Partial<Advisor> | { [k: string]: any }) {
    Object.assign(this, Advisor.map(data || {}));
  }
}

export interface Course extends Model<typeof metadata.Course> {
  courseId: number | null;
  studentId: number | null;
  student: Student | null;
  name: string | null;
}
export class Course {
  /** Mutates the input object and its descendents into a valid Course implementation. */
  static convert(data?: Partial<Course>): Course {
    return convertToModel(data || {}, metadata.Course);
  }

  /** Maps the input object and its descendents to a new, valid Course implementation. */
  static map(data?: Partial<Course>): Course {
    return mapToModel(data || {}, metadata.Course);
  }

  /** Instantiate a new Course, optionally basing it on the given data. */
  constructor(data?: Partial<Course> | { [k: string]: any }) {
    Object.assign(this, Course.map(data || {}));
  }
}

export interface DisplaysStudent
  extends Model<typeof metadata.DisplaysStudent> {
  name: string | null;
  student: Student | null;
}
