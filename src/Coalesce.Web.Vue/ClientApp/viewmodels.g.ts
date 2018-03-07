import * as metadata from './metadata.g'
import * as models from './models.g'
import * as apiClients from './api-clients.g'
import { ViewModel, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface PersonViewModel extends models.Person {}
export class PersonViewModel extends ViewModel<models.Person, apiClients.PersonApiClient> {
  constructor(initialData?: models.Person) {
    super(metadata.Person, new apiClients.PersonApiClient(metadata.Person), initialData)
  }
}
defineProps(PersonViewModel, metadata.Person)

export interface CaseViewModel extends models.Case {}
export class CaseViewModel extends ViewModel<models.Case, apiClients.CaseApiClient> {
  constructor(initialData?: models.Case) {
    super(metadata.Case, new apiClients.CaseApiClient(metadata.Case), initialData)
  }
}
defineProps(CaseViewModel, metadata.Case)

export interface CompanyViewModel extends models.Company {}
export class CompanyViewModel extends ViewModel<models.Company, apiClients.CompanyApiClient> {
  constructor(initialData?: models.Company) {
    super(metadata.Company, new apiClients.CompanyApiClient(metadata.Company), initialData)
  }
}
defineProps(CompanyViewModel, metadata.Company)

export interface ProductViewModel extends models.Product {}
export class ProductViewModel extends ViewModel<models.Product, apiClients.ProductApiClient> {
  constructor(initialData?: models.Product) {
    super(metadata.Product, new apiClients.ProductApiClient(metadata.Product), initialData)
  }
}
defineProps(ProductViewModel, metadata.Product)

export interface CaseProductViewModel extends models.CaseProduct {}
export class CaseProductViewModel extends ViewModel<models.CaseProduct, apiClients.CaseProductApiClient> {
  constructor(initialData?: models.CaseProduct) {
    super(metadata.CaseProduct, new apiClients.CaseProductApiClient(metadata.CaseProduct), initialData)
  }
}
defineProps(CaseProductViewModel, metadata.CaseProduct)

