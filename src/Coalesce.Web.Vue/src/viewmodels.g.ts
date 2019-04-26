import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ViewModelMap, ViewModelTypeLookup } from 'coalesce-vue/lib/viewmodel'
import { ModelType } from 'coalesce-vue';

export class PersonViewModel extends ViewModel<$models.Person, $apiClients.PersonApiClient> implements $models.Person  {
  
  /** ID for the person object. */
  personId: number | null = null;
  
  /** Title of the person, Mr. Mrs, etc. */
  title: $models.Titles | null = null;
  
  /** First name of the person. */
  firstName: string | null = null;
  
  /** Last name of the person */
  lastName: string | null = null;
  
  /** Email address of the person */
  email: string | null = null;
  
  /** Genetic Gender of the person. */
  gender: $models.Genders | null = null;
  
  /** List of cases assigned to the person */
  casesAssigned: CaseViewModel[] | null = null;
  
  /** List of cases reported by the person. */
  casesReported: CaseViewModel[] | null = null;
  birthDate: Date | null = null;
  lastBath: Date | null = null;
  nextUpgrade: Date | null = null;
  personStats: $models.PersonStats | null = null;
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null = null;
  
  /** Company ID this person is employed by */
  companyId: number | null = null;
  
  /** Company loaded from the Company ID */
  company: CompanyViewModel | null = null;
  arbitraryCollectionOfStrings: string[] | null = null;
  
  
  public addToCasesAssigned(): CaseViewModel {
    return this.$addChild('casesAssigned', () => new CaseViewModel())
  }
  
  
  public addToCasesReported(): CaseViewModel {
    return this.$addChild('casesReported', () => new CaseViewModel())
  }
  
  /** Sets the FirstName to the given text. */
  public rename = this.$apiClient.$makeCaller(
    "item", 
    (c, name: string | null) => c.rename(this.$primaryKey, name),
    () => ({name: null as string | null, }),
    (c, args) => c.rename(this.$primaryKey, args.name))
  
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
    (c, args) => c.changeFirstName(this.$primaryKey, args.firstName, args.title))
  
  constructor(initialData?: $models.Person) {
    super($metadata.Person, new $apiClients.PersonApiClient(), initialData)
  }
}

export class PersonListViewModel extends ListViewModel<$models.Person, $apiClients.PersonApiClient> {
  
  /** Adds two numbers. */
  public add = this.$apiClient.$makeCaller(
    "item", 
    (c, numberOne: number | null, numberTwo: number | null) => c.add(numberOne, numberTwo),
    () => ({numberOne: null as number | null, numberTwo: null as number | null, }),
    (c, args) => c.add(args.numberOne, args.numberTwo))
  
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
    (c, args) => c.personCount(args.lastNameStartsWith))
  
  public removePersonById = this.$apiClient.$makeCaller(
    "item", 
    (c, id: number | null) => c.removePersonById(id),
    () => ({id: null as number | null, }),
    (c, args) => c.removePersonById(args.id))
  
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
    (c, args) => c.namesStartingWith(args.characters))
  
  /** Gets people matching the criteria, paginated by parameter 'page'. */
  public searchPeople = this.$apiClient.$makeCaller(
    "list", 
    (c, criteria: $models.PersonCriteria | null, page: number | null) => c.searchPeople(criteria, page),
    () => ({criteria: null as $models.PersonCriteria | null, page: null as number | null, }),
    (c, args) => c.searchPeople(args.criteria, args.page))
  
  constructor() {
    super($metadata.Person, new $apiClients.PersonApiClient())
  }
}


export class CaseViewModel extends ViewModel<$models.Case, $apiClients.CaseApiClient> implements $models.Case  {
  
  /** The Primary key for the Case object */
  caseKey: number | null = null;
  title: string | null = null;
  description: string | null = null;
  openedAt: Date | null = null;
  assignedToId: number | null = null;
  assignedTo: PersonViewModel | null = null;
  reportedById: number | null = null;
  reportedBy: PersonViewModel | null = null;
  imageName: string | null = null;
  imageSize: number | null = null;
  imageHash: string | null = null;
  attachmentName: string | null = null;
  severity: string | null = null;
  status: $models.Statuses | null = null;
  caseProducts: CaseProductViewModel[] | null = null;
  devTeamAssignedId: number | null = null;
  devTeamAssigned: $models.DevTeam | null = null;
  duration: any | null = null;
  
  
  public addToCaseProducts(): CaseProductViewModel {
    return this.$addChild('caseProducts', () => new CaseProductViewModel())
  }
  
  constructor(initialData?: $models.Case) {
    super($metadata.Case, new $apiClients.CaseApiClient(), initialData)
  }
}

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


export class CompanyViewModel extends ViewModel<$models.Company, $apiClients.CompanyApiClient> implements $models.Company  {
  companyId: number | null = null;
  name: string | null = null;
  address1: string | null = null;
  address2: string | null = null;
  city: string | null = null;
  state: string | null = null;
  zipCode: string | null = null;
  isDeleted: boolean | null = null;
  employees: PersonViewModel[] | null = null;
  altName: string | null = null;
  
  
  public addToEmployees(): PersonViewModel {
    return this.$addChild('employees', () => new PersonViewModel())
  }
  
  constructor(initialData?: $models.Company) {
    super($metadata.Company, new $apiClients.CompanyApiClient(), initialData)
  }
}

export class CompanyListViewModel extends ListViewModel<$models.Company, $apiClients.CompanyApiClient> {
  
  public getCertainItems = this.$apiClient.$makeCaller(
    "item", 
    (c, isDeleted: boolean | null) => c.getCertainItems(isDeleted),
    () => ({isDeleted: null as boolean | null, }),
    (c, args) => c.getCertainItems(args.isDeleted))
  
  constructor() {
    super($metadata.Company, new $apiClients.CompanyApiClient())
  }
}


export class ProductViewModel extends ViewModel<$models.Product, $apiClients.ProductApiClient> implements $models.Product  {
  productId: number | null = null;
  name: string | null = null;
  details: $models.ProductDetails | null = null;
  uniqueId: string | null = null;
  
  constructor(initialData?: $models.Product) {
    super($metadata.Product, new $apiClients.ProductApiClient(), initialData)
  }
}

export class ProductListViewModel extends ListViewModel<$models.Product, $apiClients.ProductApiClient> {
  
  constructor() {
    super($metadata.Product, new $apiClients.ProductApiClient())
  }
}


export class CaseProductViewModel extends ViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient> implements $models.CaseProduct  {
  caseProductId: number | null = null;
  caseId: number | null = null;
  case: CaseViewModel | null = null;
  productId: number | null = null;
  product: ProductViewModel | null = null;
  
  constructor(initialData?: $models.CaseProduct) {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient(), initialData)
  }
}

export class CaseProductListViewModel extends ListViewModel<$models.CaseProduct, $apiClients.CaseProductApiClient> {
  
  constructor() {
    super($metadata.CaseProduct, new $apiClients.CaseProductApiClient())
  }
}


export class CaseDtoViewModel extends ViewModel<$models.CaseDto, $apiClients.CaseDtoApiClient> implements $models.CaseDto  {
  caseId: number | null = null;
  title: string | null = null;
  assignedToName: string | null = null;
  
  public asyncMethodOnIClassDto = this.$apiClient.$makeCaller(
    "item", 
    (c, input: string | null) => c.asyncMethodOnIClassDto(this.$primaryKey, input),
    () => ({input: null as string | null, }),
    (c, args) => c.asyncMethodOnIClassDto(this.$primaryKey, args.input))
  
  constructor(initialData?: $models.CaseDto) {
    super($metadata.CaseDto, new $apiClients.CaseDtoApiClient(), initialData)
  }
}

export class CaseDtoListViewModel extends ListViewModel<$models.CaseDto, $apiClients.CaseDtoApiClient> {
  
  constructor() {
    super($metadata.CaseDto, new $apiClients.CaseDtoApiClient())
  }
}

const viewModelTypeLookup =  {
  Person: {
    viewModel: PersonViewModel,
    listViewModel: PersonListViewModel
  },
  Case: {
    viewModel: CaseViewModel,
    listViewModel: CaseListViewModel
  },
  Company: {
    viewModel: CompanyViewModel,
    listViewModel: CompanyListViewModel
  },
  Product: {
    viewModel: ProductViewModel,
    listViewModel: ProductListViewModel
  },
  CaseProduct: {
    viewModel: CaseProductViewModel,
    listViewModel: CaseProductListViewModel
  },
  CaseDto: {
    viewModel: CaseDtoViewModel,
    listViewModel: CaseDtoListViewModel
  },
} as ViewModelTypeLookup

export default viewModelTypeLookup;

export function factory(meta: ModelType) {
  return new viewModelTypeLookup[meta.name].viewModel();
}

export const viewModelMap = new ViewModelMap(model => {
    var ctor = (viewModelTypeLookup as ViewModelTypeLookup)[model.$metadata.name].viewModel;
    return new ctor(model);
})
