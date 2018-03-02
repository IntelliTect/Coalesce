import * as metadata from './metadata.g'
import * as models from './models.g'
import { ViewModel, defineProps } from './coalesce/core/viewmodel'

export interface PersonViewModel extends models.Person {}
export class PersonViewModel extends ViewModel<models.Person> {
  constructor(initialData?: models.Person) {
    super(metadata.Person, initialData)
  }
}
defineProps(PersonViewModel, metadata.Person)

export interface CaseViewModel extends models.Case {}
export class CaseViewModel extends ViewModel<models.Case> {
  constructor(initialData?: models.Case) {
    super(metadata.Case, initialData)
  }
}
defineProps(CaseViewModel, metadata.Case)

export interface CompanyViewModel extends models.Company {}
export class CompanyViewModel extends ViewModel<models.Company> {
  constructor(initialData?: models.Company) {
    super(metadata.Company, initialData)
  }
}
defineProps(CompanyViewModel, metadata.Company)

export interface ProductViewModel extends models.Product {}
export class ProductViewModel extends ViewModel<models.Product> {
  constructor(initialData?: models.Product) {
    super(metadata.Product, initialData)
  }
}
defineProps(ProductViewModel, metadata.Product)

export interface CaseProductViewModel extends models.CaseProduct {}
export class CaseProductViewModel extends ViewModel<models.CaseProduct> {
  constructor(initialData?: models.CaseProduct) {
    super(metadata.CaseProduct, initialData)
  }
}
defineProps(CaseProductViewModel, metadata.CaseProduct)

