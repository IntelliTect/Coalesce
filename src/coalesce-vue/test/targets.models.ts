import * as metadata from './targets.metadata'
import { Model, DataSource, convertToModel, mapToModel } from '@/model'

export enum Grade {
  Freshman = 9,
  Sophomore = 10,
  Junior = 11,
  Senior = 12,
}

export interface Student extends Model<typeof metadata.Student> {
  studentId: number | null
  name: string | null
  isEnrolled: boolean | null
  birthDate: Date | null
  courses: Course[] | null
  grade: Grade | null
  advisor: Advisor | null
  advisorId: number | null
}

export interface Advisor extends Model<typeof metadata.Advisor> {
  advisorId: number | null
  name: string | null
}

export interface Course extends Model<typeof metadata.Course> {
  courseId: number | null
  name: string | null
}