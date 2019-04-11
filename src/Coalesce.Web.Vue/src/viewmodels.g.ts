import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface PersonViewModel extends $models.Person {}
export class PersonViewModel extends ViewModel<$models.Person, $apiClients.PersonApiClient> {
  
  /** Sets the FirstName to the given text. */
  public rename = this.$apiClient.$makeCaller(
    "item", 
    (c, name: string | null) => c.rename(this.$primaryKey, name),
    () => ({name: null as string | null, }),
    (c, args) => c.rename(this.$primaryKey, , args.name))
  
  /** Removes spaces from the name and puts in dashes */
  public changeSpacesToDashesInName = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.changeSpacesToDashesInName(this.$primaryKey, ),
    () => ({}),
    (c, args) => c.changeSpacesToDashesInName(this.$primaryKey, ))
  
  public fullNameAndAge = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.fullNameAndAge(this.$primaryKey, ),
    () => ({}),
    (c, args) => c.fullNameAndAge(this.$primaryKey, ))
  
  public obfuscateEmail = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.obfuscateEmail(this.$primaryKey, ),
    () => ({}),
    (c, args) => c.obfuscateEmail(this.$primaryKey, ))
  
  public changeFirstName = this.$apiClient.$makeCaller(
    "item", 
    (c, firstName: string | null, title: $models.Titles | null) => c.changeFirstName(this.$primaryKey, firstName, title),
    () => ({firstName: null as string | null, title: null as $models.Titles | null, }),
    (c, args) => c.changeFirstName(this.$primaryKey, , args.firstName, , args.title))
  
  constructor(initialData?: $models.Person) {
    super($metadata.Person, new $apiClients.PersonApiClient(), initialData)
  }
}
defineProps(PersonViewModel, $metadata.Person)

export class PersonListViewModel extends ListViewModel<$models.Person, $apiClients.PersonApiClient> {
  
  /** Adds two numbers. */
  public add = this.$apiClient.$makeCaller(
    "item", 
    (c, numberOne: number | null, numberTwo: number | null) => c.add(numberOne, numberTwo),
    () => ({numberOne: null as number | null, numberTwo: null as number | null, }),
    (c, args) => c.add(, args.numberOne, , args.numberTwo))
  
  /** Returns the user name */
  public getUser = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.getUser(),
    () => ({}),
    (c, args) => c.getUser())
  
  public personCount = this.$apiClient.$makeCaller(
    "item", 
    (c, lastNameStartsWith: string | null) => c.personCount(lastNameStartsWith),
    () => ({lastNameStartsWith: null as string | null, }),
    (c, args) => c.personCount(, args.lastNameStartsWith))
  
  public removePersonById = this.$apiClient.$makeCaller(
    "item", 
    (c, id: number | null) => c.removePersonById(id),
    () => ({id: null as number | null, }),
    (c, args) => c.removePersonById(, args.id))
  
  /** Returns the user name */
  public getUserPublic = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.getUserPublic(),
    () => ({}),
    (c, args) => c.getUserPublic())
  
  /** Gets all the first names starting with the characters. */
  public namesStartingWith = this.$apiClient.$makeCaller(
    "item", 
    (c, characters: string | null) => c.namesStartingWith(characters),
    () => ({characters: null as string | null, }),
    (c, args) => c.namesStartingWith(, args.characters))
  
  /** Gets people matching the criteria, paginated by parameter 'page'. */
  public searchPeople = this.$apiClient.$makeCaller(
    "list", 
    (c, criteria: $models.PersonCriteria | null, page: number | null) => c.searchPeople(criteria, page),
    () => ({criteria: null as $models.PersonCriteria | null, page: null as number | null, }),
    (c, args) => c.searchPeople(, args.criteria, , args.page))
  
  constructor() {
    super($metadata.Person, new $apiClients.PersonApiClient())
  }
}


export interface CaseViewModel extends $models.Case {}
export class CaseViewModel extends ViewModel<$models.Case, $apiClients.CaseApiClient> {
  
  constructor(initialData?: $models.Case) {
    super($metadata.Case, new $apiClients.CaseApiClient(), initialData)
  }
}
defineProps(CaseViewModel, $metadata.Case)

export class CaseListViewModel extends ListViewModel<$models.Case, $apiClients.CaseApiClient> {
  
  public getSomeCases = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.getSomeCases(),
    () => ({}),
    (c, args) => c.getSomeCases())
  
  public getAllOpenCasesCount = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.getAllOpenCasesCount(),
    () => ({}),
    (c, args) => c.getAllOpenCasesCount())
  
  public randomizeDatesAndStatus = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.randomizeDatesAndStatus(),
    () => ({}),
    (c, args) => c.randomizeDatesAndStatus())
  
  /** Returns a list of summary information about Cases */
  public getCaseSummary = this.$apiClient.$makeCaller(
    "item", 
    (c) => c.getCaseSummary(),
    () => ({}),
    (c, args) => c.getCaseSummary())
  
  constructor() {
    super($metadata.Case, new $apiClients.CaseApiClient())
  }
}


export interface CompanyViewModel extends $models.Company {}
export class CompanyViewModel extends ViewModel<$models.Company, $apiClients.CompanyApiClient> {
  
  constructor(initialData?: $models.Company) {
    super($metadata.Company, new $apiClients.CompanyApiClient(), initialData)
  }
}
defineProps(CompanyViewModel, $metadata.Company)

export class CompanyListViewModel extends ListViewModel<$models.Company, $apiClients.CompanyApiClient> {
  
  public getCertainItems = this.$apiClient.$makeCaller(
    "item", 
    (c, isDeleted: boolean | null) => c.getCertainItems(isDeleted),
    () => ({isDeleted: null as boolean | null, }),
    (c, args) => c.getCertainItems(, args.isDeleted))
  
  constructor() {
    super($metadata.Company, new $apiClients.CompanyApiClient())
  }
}


export interface ProductViewModel extends $models.Product {}
export class ProductViewModel extends ViewModel<$models.Product, $apiClients.ProductApiClient> {
  
  constructor(initialData?: $models.Product) {
    super($metadata.Product, new $apiClients.ProductApiClient(), initialData)
  }
}
defineProps(ProductViewModel, $metadata.Product)

export class ProductListViewModel extends ListViewModel<$models.Product, $apiClients.ProductApiClient> {
  
  constructor() {
    super($metadata.Product, new $apiClients.ProductApiClient())
  }
}


export interface CaseProductViewModel extends $models.CaseProduct {}
export class CaseProductViewModel extends ViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient> {
  
  constructor(initialData?: $models.CaseProduct) {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient(), initialData)
  }
}
defineProps(CaseProductViewModel, $metadata.CaseProduct)

export class CaseProductListViewModel extends ListViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient> {
  
  constructor() {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient())
  }
}


