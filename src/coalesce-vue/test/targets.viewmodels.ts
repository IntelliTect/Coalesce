import * as metadata from "./targets.metadata"
import * as models from "./targets.models"
import * as apiClients from "./targets.apiclients"
import { ViewModel, ListViewModel, defineProps } from '@/viewmodel'

export interface StudentViewModel extends models.Student {}
export class StudentViewModel extends ViewModel<models.Student, apiClients.StudentApiClient> {
  constructor(initialData?: models.Student) {
    super(metadata.Student, new apiClients.StudentApiClient(), initialData)
  }
}
defineProps(StudentViewModel, metadata.Student)