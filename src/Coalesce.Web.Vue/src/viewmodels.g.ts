import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ServiceViewModel, DeepPartial, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface CaseViewModel extends $models.Case {
  
  /** The Primary key for the Case object */
  caseKey: number | null;
  title: string | null;
  description: string | null;
  openedAt: Date | null;
  assignedToId: number | null;
  assignedTo: PersonViewModel | null;
  reportedById: number | null;
  reportedBy: PersonViewModel | null;
  imageName: string | null;
  imageSize: number | null;
  imageHash: string | null;
  attachmentName: string | null;
  severity: string | null;
  status: $models.Statuses | null;
  caseProducts: CaseProductViewModel[] | null;
  devTeamAssignedId: number | null;
  devTeamAssigned: $models.DevTeam | null;
  duration: any | null;
}
export class CaseViewModel extends ViewModel<$models.Case, $apiClients.CaseApiClient, number> implements $models.Case  {
  
  
  public addToCaseProducts() {
    return this.$addChild('caseProducts') as CaseProductViewModel
  }
  
  get products(): ReadonlyArray<ProductViewModel> {
    return (this.caseProducts || []).map($ => $.product!).filter($ => $)
  }
  
  public get uploadAttachment() {
    const uploadAttachment = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadAttachment,
      (c, file: File | null) => c.uploadAttachment(this.$primaryKey, file),
      () => ({file: null as File | null, }),
      (c, args) => c.uploadAttachment(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'uploadAttachment', {value: uploadAttachment});
    return uploadAttachment
  }
  
  constructor(initialData?: DeepPartial<$models.Case> | null) {
    super($metadata.Case, new $apiClients.CaseApiClient(), initialData)
  }
}
defineProps(CaseViewModel, $metadata.Case)

export class CaseListViewModel extends ListViewModel<$models.Case, $apiClients.CaseApiClient, CaseViewModel> {
  
  public get getSomeCases() {
    const getSomeCases = this.$apiClient.$makeCaller(
      this.$metadata.methods.getSomeCases,
      (c) => c.getSomeCases(),
      () => ({}),
      (c, args) => c.getSomeCases())
    
    Object.defineProperty(this, 'getSomeCases', {value: getSomeCases});
    return getSomeCases
  }
  
  public get getAllOpenCasesCount() {
    const getAllOpenCasesCount = this.$apiClient.$makeCaller(
      this.$metadata.methods.getAllOpenCasesCount,
      (c) => c.getAllOpenCasesCount(),
      () => ({}),
      (c, args) => c.getAllOpenCasesCount())
    
    Object.defineProperty(this, 'getAllOpenCasesCount', {value: getAllOpenCasesCount});
    return getAllOpenCasesCount
  }
  
  public get randomizeDatesAndStatus() {
    const randomizeDatesAndStatus = this.$apiClient.$makeCaller(
      this.$metadata.methods.randomizeDatesAndStatus,
      (c) => c.randomizeDatesAndStatus(),
      () => ({}),
      (c, args) => c.randomizeDatesAndStatus())
    
    Object.defineProperty(this, 'randomizeDatesAndStatus', {value: randomizeDatesAndStatus});
    return randomizeDatesAndStatus
  }
  
  /** Returns a list of summary information about Cases */
  public get getCaseSummary() {
    const getCaseSummary = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCaseSummary,
      (c) => c.getCaseSummary(),
      () => ({}),
      (c, args) => c.getCaseSummary())
    
    Object.defineProperty(this, 'getCaseSummary', {value: getCaseSummary});
    return getCaseSummary
  }
  
  constructor() {
    super($metadata.Case, new $apiClients.CaseApiClient())
  }
}


export interface CaseDtoViewModel extends $models.CaseDto {
  caseId: number | null;
  title: string | null;
  assignedToName: string | null;
}
export class CaseDtoViewModel extends ViewModel<$models.CaseDto, $apiClients.CaseDtoApiClient, number> implements $models.CaseDto  {
  
  public get asyncMethodOnIClassDto() {
    const asyncMethodOnIClassDto = this.$apiClient.$makeCaller(
      this.$metadata.methods.asyncMethodOnIClassDto,
      (c, input: string | null) => c.asyncMethodOnIClassDto(this.$primaryKey, input),
      () => ({input: null as string | null, }),
      (c, args) => c.asyncMethodOnIClassDto(this.$primaryKey, args.input))
    
    Object.defineProperty(this, 'asyncMethodOnIClassDto', {value: asyncMethodOnIClassDto});
    return asyncMethodOnIClassDto
  }
  
  constructor(initialData?: DeepPartial<$models.CaseDto> | null) {
    super($metadata.CaseDto, new $apiClients.CaseDtoApiClient(), initialData)
  }
}
defineProps(CaseDtoViewModel, $metadata.CaseDto)

export class CaseDtoListViewModel extends ListViewModel<$models.CaseDto, $apiClients.CaseDtoApiClient, CaseDtoViewModel> {
  
  constructor() {
    super($metadata.CaseDto, new $apiClients.CaseDtoApiClient())
  }
}


export interface CaseProductViewModel extends $models.CaseProduct {
  caseProductId: number | null;
  caseId: number | null;
  case: CaseViewModel | null;
  productId: number | null;
  product: ProductViewModel | null;
}
export class CaseProductViewModel extends ViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient, number> implements $models.CaseProduct  {
  
  constructor(initialData?: DeepPartial<$models.CaseProduct> | null) {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient(), initialData)
  }
}
defineProps(CaseProductViewModel, $metadata.CaseProduct)

export class CaseProductListViewModel extends ListViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient, CaseProductViewModel> {
  
  constructor() {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient())
  }
}


export interface CompanyViewModel extends $models.Company {
  companyId: number | null;
  name: string | null;
  address1: string | null;
  address2: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  isDeleted: boolean | null;
  employees: PersonViewModel[] | null;
  altName: string | null;
}
export class CompanyViewModel extends ViewModel<$models.Company, $apiClients.CompanyApiClient, number> implements $models.Company  {
  
  
  public addToEmployees() {
    return this.$addChild('employees') as PersonViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.Company> | null) {
    super($metadata.Company, new $apiClients.CompanyApiClient(), initialData)
  }
}
defineProps(CompanyViewModel, $metadata.Company)

export class CompanyListViewModel extends ListViewModel<$models.Company, $apiClients.CompanyApiClient, CompanyViewModel> {
  
  public get getCertainItems() {
    const getCertainItems = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCertainItems,
      (c, isDeleted: boolean | null) => c.getCertainItems(isDeleted),
      () => ({isDeleted: null as boolean | null, }),
      (c, args) => c.getCertainItems(args.isDeleted))
    
    Object.defineProperty(this, 'getCertainItems', {value: getCertainItems});
    return getCertainItems
  }
  
  constructor() {
    super($metadata.Company, new $apiClients.CompanyApiClient())
  }
}


export interface PersonViewModel extends $models.Person {
  
  /** ID for the person object. */
  personId: number | null;
  
  /** Title of the person, Mr. Mrs, etc. */
  title: $models.Titles | null;
  
  /** First name of the person. */
  firstName: string | null;
  
  /** Last name of the person */
  lastName: string | null;
  
  /** Email address of the person */
  email: string | null;
  
  /** Genetic Gender of the person. */
  gender: $models.Genders | null;
  
  /** List of cases assigned to the person */
  casesAssigned: CaseViewModel[] | null;
  
  /** List of cases reported by the person. */
  casesReported: CaseViewModel[] | null;
  birthDate: Date | null;
  lastBath: Date | null;
  nextUpgrade: Date | null;
  personStats: $models.PersonStats | null;
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null;
  
  /** Company ID this person is employed by */
  companyId: number | null;
  
  /** Company loaded from the Company ID */
  company: CompanyViewModel | null;
  arbitraryCollectionOfStrings: string[] | null;
}
export class PersonViewModel extends ViewModel<$models.Person, $apiClients.PersonApiClient, number> implements $models.Person  {
  
  
  public addToCasesAssigned() {
    return this.$addChild('casesAssigned') as CaseViewModel
  }
  
  
  public addToCasesReported() {
    return this.$addChild('casesReported') as CaseViewModel
  }
  
  /** Sets the FirstName to the given text. */
  public get rename() {
    const rename = this.$apiClient.$makeCaller(
      this.$metadata.methods.rename,
      (c, name: string | null) => c.rename(this.$primaryKey, name),
      () => ({name: null as string | null, }),
      (c, args) => c.rename(this.$primaryKey, args.name))
    
    Object.defineProperty(this, 'rename', {value: rename});
    return rename
  }
  
  /** Removes spaces from the name and puts in dashes */
  public get changeSpacesToDashesInName() {
    const changeSpacesToDashesInName = this.$apiClient.$makeCaller(
      this.$metadata.methods.changeSpacesToDashesInName,
      (c) => c.changeSpacesToDashesInName(this.$primaryKey, ),
      () => ({}),
      (c, args) => c.changeSpacesToDashesInName(this.$primaryKey, ))
    
    Object.defineProperty(this, 'changeSpacesToDashesInName', {value: changeSpacesToDashesInName});
    return changeSpacesToDashesInName
  }
  
  public get getBirthdate() {
    const getBirthdate = this.$apiClient.$makeCaller(
      this.$metadata.methods.getBirthdate,
      (c) => c.getBirthdate(this.$primaryKey, ),
      () => ({}),
      (c, args) => c.getBirthdate(this.$primaryKey, ))
    
    Object.defineProperty(this, 'getBirthdate', {value: getBirthdate});
    return getBirthdate
  }
  
  public get fullNameAndAge() {
    const fullNameAndAge = this.$apiClient.$makeCaller(
      this.$metadata.methods.fullNameAndAge,
      (c) => c.fullNameAndAge(this.$primaryKey, ),
      () => ({}),
      (c, args) => c.fullNameAndAge(this.$primaryKey, ))
    
    Object.defineProperty(this, 'fullNameAndAge', {value: fullNameAndAge});
    return fullNameAndAge
  }
  
  public get obfuscateEmail() {
    const obfuscateEmail = this.$apiClient.$makeCaller(
      this.$metadata.methods.obfuscateEmail,
      (c) => c.obfuscateEmail(this.$primaryKey, ),
      () => ({}),
      (c, args) => c.obfuscateEmail(this.$primaryKey, ))
    
    Object.defineProperty(this, 'obfuscateEmail', {value: obfuscateEmail});
    return obfuscateEmail
  }
  
  public get changeFirstName() {
    const changeFirstName = this.$apiClient.$makeCaller(
      this.$metadata.methods.changeFirstName,
      (c, firstName: string | null, title: $models.Titles | null) => c.changeFirstName(this.$primaryKey, firstName, title),
      () => ({firstName: null as string | null, title: null as $models.Titles | null, }),
      (c, args) => c.changeFirstName(this.$primaryKey, args.firstName, args.title))
    
    Object.defineProperty(this, 'changeFirstName', {value: changeFirstName});
    return changeFirstName
  }
  
  constructor(initialData?: DeepPartial<$models.Person> | null) {
    super($metadata.Person, new $apiClients.PersonApiClient(), initialData)
  }
}
defineProps(PersonViewModel, $metadata.Person)

export class PersonListViewModel extends ListViewModel<$models.Person, $apiClients.PersonApiClient, PersonViewModel> {
  
  /** 
    Adds two numbers.
    
    This comment also includes multiple lines so I can test multi-line xmldoc comments.
  */
  public get add() {
    const add = this.$apiClient.$makeCaller(
      this.$metadata.methods.add,
      (c, numberOne: number | null, numberTwo: number | null) => c.add(numberOne, numberTwo),
      () => ({numberOne: null as number | null, numberTwo: null as number | null, }),
      (c, args) => c.add(args.numberOne, args.numberTwo))
    
    Object.defineProperty(this, 'add', {value: add});
    return add
  }
  
  /** Returns the user name */
  public get getUser() {
    const getUser = this.$apiClient.$makeCaller(
      this.$metadata.methods.getUser,
      (c) => c.getUser(),
      () => ({}),
      (c, args) => c.getUser())
    
    Object.defineProperty(this, 'getUser', {value: getUser});
    return getUser
  }
  
  public get personCount() {
    const personCount = this.$apiClient.$makeCaller(
      this.$metadata.methods.personCount,
      (c, lastNameStartsWith: string | null) => c.personCount(lastNameStartsWith),
      () => ({lastNameStartsWith: null as string | null, }),
      (c, args) => c.personCount(args.lastNameStartsWith))
    
    Object.defineProperty(this, 'personCount', {value: personCount});
    return personCount
  }
  
  public get removePersonById() {
    const removePersonById = this.$apiClient.$makeCaller(
      this.$metadata.methods.removePersonById,
      (c, id: number | null) => c.removePersonById(id),
      () => ({id: null as number | null, }),
      (c, args) => c.removePersonById(args.id))
    
    Object.defineProperty(this, 'removePersonById', {value: removePersonById});
    return removePersonById
  }
  
  /** Returns the user name */
  public get getUserPublic() {
    const getUserPublic = this.$apiClient.$makeCaller(
      this.$metadata.methods.getUserPublic,
      (c) => c.getUserPublic(),
      () => ({}),
      (c, args) => c.getUserPublic())
    
    Object.defineProperty(this, 'getUserPublic', {value: getUserPublic});
    return getUserPublic
  }
  
  /** Gets all the first names starting with the characters. */
  public get namesStartingWith() {
    const namesStartingWith = this.$apiClient.$makeCaller(
      this.$metadata.methods.namesStartingWith,
      (c, characters: string | null) => c.namesStartingWith(characters),
      () => ({characters: null as string | null, }),
      (c, args) => c.namesStartingWith(args.characters))
    
    Object.defineProperty(this, 'namesStartingWith', {value: namesStartingWith});
    return namesStartingWith
  }
  
  /** Gets all the first names starting with the characters. */
  public get methodWithEntityParameter() {
    const methodWithEntityParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithEntityParameter,
      (c, person: $models.Person | null) => c.methodWithEntityParameter(person),
      () => ({person: null as $models.Person | null, }),
      (c, args) => c.methodWithEntityParameter(args.person))
    
    Object.defineProperty(this, 'methodWithEntityParameter', {value: methodWithEntityParameter});
    return methodWithEntityParameter
  }
  
  /** Gets people matching the criteria, paginated by parameter 'page'. */
  public get searchPeople() {
    const searchPeople = this.$apiClient.$makeCaller(
      this.$metadata.methods.searchPeople,
      (c, criteria: $models.PersonCriteria | null, page: number | null) => c.searchPeople(criteria, page),
      () => ({criteria: null as $models.PersonCriteria | null, page: null as number | null, }),
      (c, args) => c.searchPeople(args.criteria, args.page))
    
    Object.defineProperty(this, 'searchPeople', {value: searchPeople});
    return searchPeople
  }
  
  constructor() {
    super($metadata.Person, new $apiClients.PersonApiClient())
  }
}


export interface ProductViewModel extends $models.Product {
  productId: number | null;
  name: string | null;
  details: $models.ProductDetails | null;
  uniqueId: string | null;
}
export class ProductViewModel extends ViewModel<$models.Product, $apiClients.ProductApiClient, number> implements $models.Product  {
  
  constructor(initialData?: DeepPartial<$models.Product> | null) {
    super($metadata.Product, new $apiClients.ProductApiClient(), initialData)
  }
}
defineProps(ProductViewModel, $metadata.Product)

export class ProductListViewModel extends ListViewModel<$models.Product, $apiClients.ProductApiClient, ProductViewModel> {
  
  constructor() {
    super($metadata.Product, new $apiClients.ProductApiClient())
  }
}


export interface ZipCodeViewModel extends $models.ZipCode {
  zip: string | null;
  state: string | null;
}
export class ZipCodeViewModel extends ViewModel<$models.ZipCode, $apiClients.ZipCodeApiClient, string> implements $models.ZipCode  {
  
  constructor(initialData?: DeepPartial<$models.ZipCode> | null) {
    super($metadata.ZipCode, new $apiClients.ZipCodeApiClient(), initialData)
  }
}
defineProps(ZipCodeViewModel, $metadata.ZipCode)

export class ZipCodeListViewModel extends ListViewModel<$models.ZipCode, $apiClients.ZipCodeApiClient, ZipCodeViewModel> {
  
  constructor() {
    super($metadata.ZipCode, new $apiClients.ZipCodeApiClient())
  }
}


export class WeatherServiceViewModel extends ServiceViewModel<typeof $metadata.WeatherService, $apiClients.WeatherServiceApiClient> {
  
  public get getWeather() {
    const getWeather = this.$apiClient.$makeCaller(
      this.$metadata.methods.getWeather,
      (c, location: $models.Location | null, dateTime: Date | null, conditions: $models.SkyConditions | null) => c.getWeather(location, dateTime, conditions),
      () => ({location: null as $models.Location | null, dateTime: null as Date | null, conditions: null as $models.SkyConditions | null, }),
      (c, args) => c.getWeather(args.location, args.dateTime, args.conditions))
    
    Object.defineProperty(this, 'getWeather', {value: getWeather});
    return getWeather
  }
  
  constructor() {
    super($metadata.WeatherService, new $apiClients.WeatherServiceApiClient())
  }
}


const viewModelTypeLookup = ViewModel.typeLookup = {
  Case: CaseViewModel,
  CaseDto: CaseDtoViewModel,
  CaseProduct: CaseProductViewModel,
  Company: CompanyViewModel,
  Person: PersonViewModel,
  Product: ProductViewModel,
  ZipCode: ZipCodeViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  Case: CaseListViewModel,
  CaseDto: CaseDtoListViewModel,
  CaseProduct: CaseProductListViewModel,
  Company: CompanyListViewModel,
  Person: PersonListViewModel,
  Product: ProductListViewModel,
  ZipCode: ZipCodeListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  WeatherService: WeatherServiceViewModel,
}

