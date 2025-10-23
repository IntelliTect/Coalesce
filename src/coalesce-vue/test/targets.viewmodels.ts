/* eslint-disable @typescript-eslint/no-unsafe-declaration-merging */

// Ensure that ViewModel.typeLookup is created by the generated code as expected,
// since the viewmodels in this class (which are deprecated and usages need to be changed
// to use `test-targets`) get added to the generated ViewModel.typeLookup.
import "../../test-targets/viewmodels.g";

import * as metadata from "./targets.metadata";
import * as models from "./targets.models";
import * as apiClients from "./targets.apiclients";
import {
  ViewModel,
  ListViewModel,
  type DeepPartial,
  defineProps,
} from "../src/viewmodel";

export interface StudentViewModel extends models.Student {
  studentId: number | null;
  name: string | null;
  isEnrolled: boolean | null;
  birthDate: Date | null;
  courses: CourseViewModel[] | null;
  currentCourseId: number | null;
  currentCourse: CourseViewModel | null;
  grade: models.Grade | null;
  advisor: AdvisorViewModel | null;
  studentAdvisorId: number | null;
}
export class StudentViewModel extends ViewModel<
  models.Student,
  apiClients.StudentApiClient,
  number
> {
  constructor(initialData?: DeepPartial<models.Student> | null) {
    super(metadata.Student, new apiClients.StudentApiClient(), initialData);
  }
}
defineProps(StudentViewModel, metadata.Student);

export class StudentListViewModel extends ListViewModel<
  models.Student,
  apiClients.StudentApiClient,
  StudentViewModel
> {
  constructor() {
    super(metadata.Student, new apiClients.StudentApiClient());
  }
}

export interface CourseViewModel extends models.Course {
  courseId: number | null;
  studentId: number | null;
  name: string | null;
}
export class CourseViewModel extends ViewModel<
  models.Course,
  apiClients.CourseApiClient,
  number
> {
  constructor(initialData?: DeepPartial<models.Course> | null) {
    super(metadata.Course, new apiClients.CourseApiClient(), initialData);
  }
}
defineProps(CourseViewModel, metadata.Course);

export interface AdvisorViewModel extends models.Advisor {
  advisorId: number | null;
  name: string | null;
  students: StudentViewModel[];
  studentsNonNavigation: StudentViewModel[];
  studentWrapperObject: models.DisplaysStudent | null;
}
export class AdvisorViewModel extends ViewModel<
  models.Advisor,
  apiClients.AdvisorApiClient,
  number
> {
  constructor(initialData?: DeepPartial<models.Advisor> | null) {
    super(metadata.Advisor, new apiClients.AdvisorApiClient(), initialData);
  }
}
defineProps(AdvisorViewModel, metadata.Advisor);

// Add these to the lookup created by "../../test-targets/viewmodels.g";
Object.assign(ViewModel.typeLookup!, {
  Student: StudentViewModel,
  Course: CourseViewModel,
  Advisor: AdvisorViewModel,
});
