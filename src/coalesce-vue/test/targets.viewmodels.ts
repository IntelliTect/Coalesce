import * as metadata from "./targets.metadata"
import * as models from "./targets.models"
import * as apiClients from "./targets.apiclients"
import { ViewModel, ListViewModel, defineProps } from '../src/viewmodel'

export interface StudentViewModel extends models.Student {
  studentId: number | null;
  name: string | null;
  isEnrolled: boolean | null;
  birthDate: Date | null;
  courses: CourseViewModel[] | null;
  grade: models.Grade | null;
  advisor: AdvisorViewModel | null;
  studentAdvisorId: number | null;
}
export class StudentViewModel extends ViewModel<models.Student, apiClients.StudentApiClient, number> {
  constructor(initialData?: models.Student | {}) {
    super(metadata.Student, new apiClients.StudentApiClient(), initialData)
  }
}
defineProps(StudentViewModel, metadata.Student);


export interface CourseViewModel extends models.Course {
  courseId: number | null;
  name: string | null;
}
export class CourseViewModel extends ViewModel<models.Course, apiClients.CourseApiClient, number> {

  constructor(initialData?: models.Course | {}) {
    super(metadata.Course, new apiClients.CourseApiClient(), initialData)
  }
}
defineProps(CourseViewModel, metadata.Course);


export interface AdvisorViewModel extends models.Advisor {
  advisorId: number | null;
  name: string | null;
  students: StudentViewModel[]
}
export class AdvisorViewModel extends ViewModel<models.Advisor, apiClients.AdvisorApiClient, number> {
  constructor(initialData?: models.Advisor | {}) {
    super(metadata.Advisor, new apiClients.AdvisorApiClient(), initialData)
  }
}
defineProps(AdvisorViewModel, metadata.Advisor);

ViewModel.typeLookup = {
  Student: StudentViewModel,
  Course: CourseViewModel,
  Advisor: AdvisorViewModel,
}