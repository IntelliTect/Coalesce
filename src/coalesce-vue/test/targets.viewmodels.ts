import * as metadata from "./targets.metadata"
import * as models from "./targets.models"
import * as apiClients from "./targets.apiclients"
import { ViewModel, ListViewModel } from '../src/viewmodel'

export class StudentViewModel extends ViewModel<models.Student, apiClients.StudentApiClient> {
  studentId: number | null = null;
  name: string | null = null;
  isEnrolled: boolean | null = null;
  birthDate: Date | null = null;
  courses: CourseViewModel[] | null = null;
  grade: models.Grade | null = null;
  advisor: AdvisorViewModel | null = null;
  advisorId: number | null = null;

  constructor(initialData?: models.Student) {
    super(metadata.Student, new apiClients.StudentApiClient(), initialData)
  }
}


export class CourseViewModel extends ViewModel<models.Course, apiClients.CourseApiClient> {
  courseId: number | null = null;
  name: string | null = null;

  constructor(initialData?: models.Course) {
    super(metadata.Course, new apiClients.CourseApiClient(), initialData)
  }
}


export class AdvisorViewModel extends ViewModel<models.Advisor, apiClients.AdvisorApiClient> {
  advisorId: number | null = null;
  name: string | null = null;

  constructor(initialData?: models.Advisor) {
    super(metadata.Advisor, new apiClients.AdvisorApiClient(), initialData)
  }
}