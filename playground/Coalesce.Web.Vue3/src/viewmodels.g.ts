import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ViewModelCollection, ServiceViewModel, type DeepPartial, defineProps, createAbstractProxyViewModelType } from 'coalesce-vue/lib/viewmodel'

export type AbstractClassViewModel = AbstractClassImplViewModel
export const AbstractClassViewModel = createAbstractProxyViewModelType<$models.AbstractClass, AbstractClassViewModel>($metadata.AbstractClass, $apiClients.AbstractClassApiClient)

export class AbstractClassListViewModel extends ListViewModel<$models.AbstractClass, $apiClients.AbstractClassApiClient, AbstractClassViewModel> {
  
  public get getCount() {
    const getCount = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCount,
      (c) => c.getCount(),
      () => ({}),
      (c, args) => c.getCount())
    
    Object.defineProperty(this, 'getCount', {value: getCount});
    return getCount
  }
  
  public get echoAbstractModel() {
    const echoAbstractModel = this.$apiClient.$makeCaller(
      this.$metadata.methods.echoAbstractModel,
      (c, model: $models.AbstractClass | null) => c.echoAbstractModel(model),
      () => ({model: null as $models.AbstractClass | null, }),
      (c, args) => c.echoAbstractModel(args.model))
    
    Object.defineProperty(this, 'echoAbstractModel', {value: echoAbstractModel});
    return echoAbstractModel
  }
  
  constructor() {
    super($metadata.AbstractClass, new $apiClients.AbstractClassApiClient())
  }
}


export interface AbstractClassImplViewModel extends $models.AbstractClassImpl {
  implString: string | null;
  id: number | null;
  abstractClassString: string | null;
  get abstractModelPeople(): ViewModelCollection<AbstractClassPersonViewModel, $models.AbstractClassPerson>;
  set abstractModelPeople(value: (AbstractClassPersonViewModel | $models.AbstractClassPerson)[] | null);
}
export class AbstractClassImplViewModel extends ViewModel<$models.AbstractClassImpl, $apiClients.AbstractClassImplApiClient, number> implements $models.AbstractClassImpl  {
  
  
  public addToAbstractModelPeople(initialData?: DeepPartial<$models.AbstractClassPerson> | null) {
    return this.$addChild('abstractModelPeople', initialData) as AbstractClassPersonViewModel
  }
  
  get people(): ReadonlyArray<PersonViewModel> {
    return (this.abstractModelPeople || []).map($ => $.person!).filter($ => $)
  }
  
  public get getId() {
    const getId = this.$apiClient.$makeCaller(
      this.$metadata.methods.getId,
      (c) => c.getId(this.$primaryKey),
      () => ({}),
      (c, args) => c.getId(this.$primaryKey))
    
    Object.defineProperty(this, 'getId', {value: getId});
    return getId
  }
  
  constructor(initialData?: DeepPartial<$models.AbstractClassImpl> | null) {
    super($metadata.AbstractClassImpl, new $apiClients.AbstractClassImplApiClient(), initialData)
  }
}
defineProps(AbstractClassImplViewModel, $metadata.AbstractClassImpl)

export class AbstractClassImplListViewModel extends ListViewModel<$models.AbstractClassImpl, $apiClients.AbstractClassImplApiClient, AbstractClassImplViewModel> {
  
  public get getCount() {
    const getCount = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCount,
      (c) => c.getCount(),
      () => ({}),
      (c, args) => c.getCount())
    
    Object.defineProperty(this, 'getCount', {value: getCount});
    return getCount
  }
  
  public get echoAbstractModel() {
    const echoAbstractModel = this.$apiClient.$makeCaller(
      this.$metadata.methods.echoAbstractModel,
      (c, model: $models.AbstractClass | null) => c.echoAbstractModel(model),
      () => ({model: null as $models.AbstractClass | null, }),
      (c, args) => c.echoAbstractModel(args.model))
    
    Object.defineProperty(this, 'echoAbstractModel', {value: echoAbstractModel});
    return echoAbstractModel
  }
  
  constructor() {
    super($metadata.AbstractClassImpl, new $apiClients.AbstractClassImplApiClient())
  }
}


export interface AbstractClassPersonViewModel extends $models.AbstractClassPerson {
  id: number | null;
  personId: number | null;
  get person(): PersonViewModel | null;
  set person(value: PersonViewModel | $models.Person | null);
  abstractClassId: number | null;
  get abstractClass(): AbstractClassViewModel | null;
  set abstractClass(value: AbstractClassViewModel | $models.AbstractClass | null);
}
export class AbstractClassPersonViewModel extends ViewModel<$models.AbstractClassPerson, $apiClients.AbstractClassPersonApiClient, number> implements $models.AbstractClassPerson  {
  
  constructor(initialData?: DeepPartial<$models.AbstractClassPerson> | null) {
    super($metadata.AbstractClassPerson, new $apiClients.AbstractClassPersonApiClient(), initialData)
  }
}
defineProps(AbstractClassPersonViewModel, $metadata.AbstractClassPerson)

export class AbstractClassPersonListViewModel extends ListViewModel<$models.AbstractClassPerson, $apiClients.AbstractClassPersonApiClient, AbstractClassPersonViewModel> {
  
  constructor() {
    super($metadata.AbstractClassPerson, new $apiClients.AbstractClassPersonApiClient())
  }
}


export interface AuditLogViewModel extends $models.AuditLog {
  message: string | null;
  userId: number | null;
  get user(): PersonViewModel | null;
  set user(value: PersonViewModel | $models.Person | null);
  id: number | null;
  type: string | null;
  keyValue: string | null;
  description: string | null;
  state: $models.AuditEntryState | null;
  date: Date | null;
  get properties(): ViewModelCollection<AuditLogPropertyViewModel, $models.AuditLogProperty>;
  set properties(value: (AuditLogPropertyViewModel | $models.AuditLogProperty)[] | null);
  clientIp: string | null;
  referrer: string | null;
  endpoint: string | null;
}
export class AuditLogViewModel extends ViewModel<$models.AuditLog, $apiClients.AuditLogApiClient, number> implements $models.AuditLog  {
  
  
  public addToProperties(initialData?: DeepPartial<$models.AuditLogProperty> | null) {
    return this.$addChild('properties', initialData) as AuditLogPropertyViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.AuditLog> | null) {
    super($metadata.AuditLog, new $apiClients.AuditLogApiClient(), initialData)
  }
}
defineProps(AuditLogViewModel, $metadata.AuditLog)

export class AuditLogListViewModel extends ListViewModel<$models.AuditLog, $apiClients.AuditLogApiClient, AuditLogViewModel> {
  
  constructor() {
    super($metadata.AuditLog, new $apiClients.AuditLogApiClient())
  }
}


export interface AuditLogPropertyViewModel extends $models.AuditLogProperty {
  id: number | null;
  parentId: number | null;
  propertyName: string | null;
  oldValue: string | null;
  oldValueDescription: string | null;
  newValue: string | null;
  newValueDescription: string | null;
}
export class AuditLogPropertyViewModel extends ViewModel<$models.AuditLogProperty, $apiClients.AuditLogPropertyApiClient, number> implements $models.AuditLogProperty  {
  
  constructor(initialData?: DeepPartial<$models.AuditLogProperty> | null) {
    super($metadata.AuditLogProperty, new $apiClients.AuditLogPropertyApiClient(), initialData)
  }
}
defineProps(AuditLogPropertyViewModel, $metadata.AuditLogProperty)

export class AuditLogPropertyListViewModel extends ListViewModel<$models.AuditLogProperty, $apiClients.AuditLogPropertyApiClient, AuditLogPropertyViewModel> {
  
  constructor() {
    super($metadata.AuditLogProperty, new $apiClients.AuditLogPropertyApiClient())
  }
}


export interface BaseClassViewModel extends $models.BaseClass {
  id: number | null;
  baseClassString: string | null;
}
export class BaseClassViewModel extends ViewModel<$models.BaseClass, $apiClients.BaseClassApiClient, number> implements $models.BaseClass  {
  
  constructor(initialData?: DeepPartial<$models.BaseClass> | null) {
    super($metadata.BaseClass, new $apiClients.BaseClassApiClient(), initialData)
  }
}
defineProps(BaseClassViewModel, $metadata.BaseClass)

export class BaseClassListViewModel extends ListViewModel<$models.BaseClass, $apiClients.BaseClassApiClient, BaseClassViewModel> {
  
  constructor() {
    super($metadata.BaseClass, new $apiClients.BaseClassApiClient())
  }
}


export interface BaseClassDerivedViewModel extends $models.BaseClassDerived {
  derivedClassString: string | null;
  id: number | null;
  baseClassString: string | null;
}
export class BaseClassDerivedViewModel extends ViewModel<$models.BaseClassDerived, $apiClients.BaseClassDerivedApiClient, number> implements $models.BaseClassDerived  {
  
  constructor(initialData?: DeepPartial<$models.BaseClassDerived> | null) {
    super($metadata.BaseClassDerived, new $apiClients.BaseClassDerivedApiClient(), initialData)
  }
}
defineProps(BaseClassDerivedViewModel, $metadata.BaseClassDerived)

export class BaseClassDerivedListViewModel extends ListViewModel<$models.BaseClassDerived, $apiClients.BaseClassDerivedApiClient, BaseClassDerivedViewModel> {
  
  constructor() {
    super($metadata.BaseClassDerived, new $apiClients.BaseClassDerivedApiClient())
  }
}


export interface CaseViewModel extends $models.Case {
  
  /** The Primary key for the Case object */
  caseKey: number | null;
  title: string | null;
  
  /** User-provided description of the issue */
  description: string | null;
  
  /** Date and time when the case was opened */
  openedAt: Date | null;
  assignedToId: number | null;
  get assignedTo(): PersonViewModel | null;
  set assignedTo(value: PersonViewModel | $models.Person | null);
  reportedById: number | null;
  
  /** Person who originally reported the case */
  get reportedBy(): PersonViewModel | null;
  set reportedBy(value: PersonViewModel | $models.Person | null);
  attachmentSize: number | null;
  attachmentName: string | null;
  attachmentType: string | null;
  attachmentHash: string | null;
  severity: string | null;
  status: $models.Statuses | null;
  numbers: number[] | null;
  strings: string[] | null;
  states: $models.Statuses[] | null;
  get caseProducts(): ViewModelCollection<CaseProductViewModel, $models.CaseProduct>;
  set caseProducts(value: (CaseProductViewModel | $models.CaseProduct)[] | null);
  devTeamAssignedId: number | null;
  devTeamAssigned: $models.DevTeam | null;
  duration: unknown | null;
}
export class CaseViewModel extends ViewModel<$models.Case, $apiClients.CaseApiClient, number> implements $models.Case  {
  static DataSources = $models.Case.DataSources;
  
  static magicNumber = 42
  static magicString = "42"
  static magicEnum = $models.Statuses.ClosedNoSolution
  
  
  public addToCaseProducts(initialData?: DeepPartial<$models.CaseProduct> | null) {
    return this.$addChild('caseProducts', initialData) as CaseProductViewModel
  }
  
  get products(): ReadonlyArray<ProductViewModel> {
    return (this.caseProducts || []).map($ => $.product!).filter($ => $)
  }
  
  public get uploadImage() {
    const uploadImage = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadImage,
      (c, file: File | null) => c.uploadImage(this.$primaryKey, file),
      () => ({file: null as File | null, }),
      (c, args) => c.uploadImage(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'uploadImage', {value: uploadImage});
    return uploadImage
  }
  
  public get downloadImage() {
    const downloadImage = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadImage,
      (c) => c.downloadImage(this.$primaryKey, this.attachmentHash),
      () => ({}),
      (c, args) => c.downloadImage(this.$primaryKey, this.attachmentHash))
    
    Object.defineProperty(this, 'downloadImage', {value: downloadImage});
    return downloadImage
  }
  
  public get uploadAndDownload() {
    const uploadAndDownload = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadAndDownload,
      (c, file: File | null) => c.uploadAndDownload(this.$primaryKey, file),
      () => ({file: null as File | null, }),
      (c, args) => c.uploadAndDownload(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'uploadAndDownload', {value: uploadAndDownload});
    return uploadAndDownload
  }
  
  public get uploadImages() {
    const uploadImages = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadImages,
      (c, files: File[] | null) => c.uploadImages(this.$primaryKey, files),
      () => ({files: null as File[] | null, }),
      (c, args) => c.uploadImages(this.$primaryKey, args.files))
    
    Object.defineProperty(this, 'uploadImages', {value: uploadImages});
    return uploadImages
  }
  
  public get uploadByteArray() {
    const uploadByteArray = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadByteArray,
      (c, file: string | Uint8Array | null) => c.uploadByteArray(this.$primaryKey, file),
      () => ({file: null as string | Uint8Array | null, }),
      (c, args) => c.uploadByteArray(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'uploadByteArray', {value: uploadByteArray});
    return uploadByteArray
  }
  
  constructor(initialData?: DeepPartial<$models.Case> | null) {
    super($metadata.Case, new $apiClients.CaseApiClient(), initialData)
  }
}
defineProps(CaseViewModel, $metadata.Case)

export class CaseListViewModel extends ListViewModel<$models.Case, $apiClients.CaseApiClient, CaseViewModel> {
  static DataSources = $models.Case.DataSources;
  
  static magicNumber = 42
  static magicString = "42"
  static magicEnum = $models.Statuses.ClosedNoSolution
  
  public get getCaseTitles() {
    const getCaseTitles = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCaseTitles,
      (c, search: string | null) => c.getCaseTitles(search),
      () => ({search: null as string | null, }),
      (c, args) => c.getCaseTitles(args.search))
    
    Object.defineProperty(this, 'getCaseTitles', {value: getCaseTitles});
    return getCaseTitles
  }
  
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
  static DataSources = $models.CaseDto.DataSources;
  
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
    this.$saveMode = "whole"
  }
}
defineProps(CaseDtoViewModel, $metadata.CaseDto)

export class CaseDtoListViewModel extends ListViewModel<$models.CaseDto, $apiClients.CaseDtoApiClient, CaseDtoViewModel> {
  static DataSources = $models.CaseDto.DataSources;
  
  constructor() {
    super($metadata.CaseDto, new $apiClients.CaseDtoApiClient())
  }
}


export interface CaseDtoStandaloneViewModel extends $models.CaseDtoStandalone {
  caseId: number | null;
  title: string | null;
}
export class CaseDtoStandaloneViewModel extends ViewModel<$models.CaseDtoStandalone, $apiClients.CaseDtoStandaloneApiClient, number> implements $models.CaseDtoStandalone  {
  
  constructor(initialData?: DeepPartial<$models.CaseDtoStandalone> | null) {
    super($metadata.CaseDtoStandalone, new $apiClients.CaseDtoStandaloneApiClient(), initialData)
    this.$saveMode = "whole"
  }
}
defineProps(CaseDtoStandaloneViewModel, $metadata.CaseDtoStandalone)

export class CaseDtoStandaloneListViewModel extends ListViewModel<$models.CaseDtoStandalone, $apiClients.CaseDtoStandaloneApiClient, CaseDtoStandaloneViewModel> {
  
  constructor() {
    super($metadata.CaseDtoStandalone, new $apiClients.CaseDtoStandaloneApiClient())
  }
}


export interface CaseProductViewModel extends $models.CaseProduct {
  caseProductId: number | null;
  caseId: number | null;
  get case(): CaseViewModel | null;
  set case(value: CaseViewModel | $models.Case | null);
  productId: number | null;
  get product(): ProductViewModel | null;
  set product(value: ProductViewModel | $models.Product | null);
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


export interface CaseStandaloneViewModel extends $models.CaseStandalone {
  id: number | null;
  get assignedTo(): PersonViewModel | null;
  set assignedTo(value: PersonViewModel | $models.Person | null);
}
export class CaseStandaloneViewModel extends ViewModel<$models.CaseStandalone, $apiClients.CaseStandaloneApiClient, number> implements $models.CaseStandalone  {
  static DataSources = $models.CaseStandalone.DataSources;
  
  constructor(initialData?: DeepPartial<$models.CaseStandalone> | null) {
    super($metadata.CaseStandalone, new $apiClients.CaseStandaloneApiClient(), initialData)
  }
}
defineProps(CaseStandaloneViewModel, $metadata.CaseStandalone)

export class CaseStandaloneListViewModel extends ListViewModel<$models.CaseStandalone, $apiClients.CaseStandaloneApiClient, CaseStandaloneViewModel> {
  static DataSources = $models.CaseStandalone.DataSources;
  
  constructor() {
    super($metadata.CaseStandalone, new $apiClients.CaseStandaloneApiClient())
  }
}


export interface CompanyViewModel extends $models.Company {
  id: number | null;
  name: string | null;
  address1: string | null;
  address2: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  phone: string | null;
  websiteUrl: string | null;
  logoUrl: string | null;
  isDeleted: boolean | null;
  get employees(): ViewModelCollection<PersonViewModel, $models.Person>;
  set employees(value: (PersonViewModel | $models.Person)[] | null);
  altName: string | null;
}
export class CompanyViewModel extends ViewModel<$models.Company, $apiClients.CompanyApiClient, number> implements $models.Company  {
  static DataSources = $models.Company.DataSources;
  
  
  public addToEmployees(initialData?: DeepPartial<$models.Person> | null) {
    return this.$addChild('employees', initialData) as PersonViewModel
  }
  
  public get conflictingParameterNames() {
    const conflictingParameterNames = this.$apiClient.$makeCaller(
      this.$metadata.methods.conflictingParameterNames,
      (c, companyParam: $models.Company | null, name: string | null) => c.conflictingParameterNames(this.$primaryKey, companyParam, name),
      () => ({companyParam: null as $models.Company | null, name: null as string | null, }),
      (c, args) => c.conflictingParameterNames(this.$primaryKey, args.companyParam, args.name))
    
    Object.defineProperty(this, 'conflictingParameterNames', {value: conflictingParameterNames});
    return conflictingParameterNames
  }
  
  constructor(initialData?: DeepPartial<$models.Company> | null) {
    super($metadata.Company, new $apiClients.CompanyApiClient(), initialData)
  }
}
defineProps(CompanyViewModel, $metadata.Company)

export class CompanyListViewModel extends ListViewModel<$models.Company, $apiClients.CompanyApiClient, CompanyViewModel> {
  static DataSources = $models.Company.DataSources;
  
  public get getCertainItems() {
    const getCertainItems = this.$apiClient.$makeCaller(
      this.$metadata.methods.getCertainItems,
      (c, isDeleted?: boolean | null) => c.getCertainItems(isDeleted),
      () => ({isDeleted: null as boolean | null, }),
      (c, args) => c.getCertainItems(args.isDeleted))
    
    Object.defineProperty(this, 'getCertainItems', {value: getCertainItems});
    return getCertainItems
  }
  
  constructor() {
    super($metadata.Company, new $apiClients.CompanyApiClient())
  }
}


export interface LogViewModel extends $models.Log {
  logId: number | null;
  level: string | null;
  message: string | null;
}
export class LogViewModel extends ViewModel<$models.Log, $apiClients.LogApiClient, number> implements $models.Log  {
  
  constructor(initialData?: DeepPartial<$models.Log> | null) {
    super($metadata.Log, new $apiClients.LogApiClient(), initialData)
  }
}
defineProps(LogViewModel, $metadata.Log)

export class LogListViewModel extends ListViewModel<$models.Log, $apiClients.LogApiClient, LogViewModel> {
  
  constructor() {
    super($metadata.Log, new $apiClients.LogApiClient())
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
  height: number | null;
  
  /** List of cases assigned to the person */
  get casesAssigned(): ViewModelCollection<CaseViewModel, $models.Case>;
  set casesAssigned(value: (CaseViewModel | $models.Case)[] | null);
  
  /** List of cases reported by the person. */
  get casesReported(): ViewModelCollection<CaseViewModel, $models.Case>;
  set casesReported(value: (CaseViewModel | $models.Case)[] | null);
  birthDate: Date | null;
  lastBath: Date | null;
  nextUpgrade: Date | null;
  personStats: $models.PersonStats | null;
  profilePic: string | null;
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null;
  
  /** Company ID this person is employed by */
  companyId: number | null;
  
  /** Company loaded from the Company ID */
  get company(): CompanyViewModel | null;
  set company(value: CompanyViewModel | $models.Company | null);
  arbitraryCollectionOfStrings: string[] | null;
}
export class PersonViewModel extends ViewModel<$models.Person, $apiClients.PersonApiClient, number> implements $models.Person  {
  static DataSources = $models.Person.DataSources;
  
  
  public addToCasesAssigned(initialData?: DeepPartial<$models.Case> | null) {
    return this.$addChild('casesAssigned', initialData) as CaseViewModel
  }
  
  
  public addToCasesReported(initialData?: DeepPartial<$models.Case> | null) {
    return this.$addChild('casesReported', initialData) as CaseViewModel
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
  
  public get uploadPicture() {
    const uploadPicture = this.$apiClient.$makeCaller(
      this.$metadata.methods.uploadPicture,
      (c, file: File | null) => c.uploadPicture(this.$primaryKey, file),
      () => ({file: null as File | null, }),
      (c, args) => c.uploadPicture(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'uploadPicture', {value: uploadPicture});
    return uploadPicture
  }
  
  /** Removes spaces from the name and puts in dashes */
  public get changeSpacesToDashesInName() {
    const changeSpacesToDashesInName = this.$apiClient.$makeCaller(
      this.$metadata.methods.changeSpacesToDashesInName,
      (c) => c.changeSpacesToDashesInName(this.$primaryKey),
      () => ({}),
      (c, args) => c.changeSpacesToDashesInName(this.$primaryKey))
    
    Object.defineProperty(this, 'changeSpacesToDashesInName', {value: changeSpacesToDashesInName});
    return changeSpacesToDashesInName
  }
  
  public get getBirthdate() {
    const getBirthdate = this.$apiClient.$makeCaller(
      this.$metadata.methods.getBirthdate,
      (c) => c.getBirthdate(this.$primaryKey),
      () => ({}),
      (c, args) => c.getBirthdate(this.$primaryKey))
    
    Object.defineProperty(this, 'getBirthdate', {value: getBirthdate});
    return getBirthdate
  }
  
  /** Returns the user name */
  public get setBirthDate() {
    const setBirthDate = this.$apiClient.$makeCaller(
      this.$metadata.methods.setBirthDate,
      (c, date?: Date | null, time?: Date | null) => c.setBirthDate(this.$primaryKey, date, time),
      () => ({date: null as Date | null, time: null as Date | null, }),
      (c, args) => c.setBirthDate(this.$primaryKey, args.date, args.time))
    
    Object.defineProperty(this, 'setBirthDate', {value: setBirthDate});
    return setBirthDate
  }
  
  public get fullNameAndAge() {
    const fullNameAndAge = this.$apiClient.$makeCaller(
      this.$metadata.methods.fullNameAndAge,
      (c) => c.fullNameAndAge(this.$primaryKey),
      () => ({}),
      (c, args) => c.fullNameAndAge(this.$primaryKey))
    
    Object.defineProperty(this, 'fullNameAndAge', {value: fullNameAndAge});
    return fullNameAndAge
  }
  
  public get obfuscateEmail() {
    const obfuscateEmail = this.$apiClient.$makeCaller(
      this.$metadata.methods.obfuscateEmail,
      (c) => c.obfuscateEmail(this.$primaryKey),
      () => ({}),
      (c, args) => c.obfuscateEmail(this.$primaryKey))
    
    Object.defineProperty(this, 'obfuscateEmail', {value: obfuscateEmail});
    return obfuscateEmail
  }
  
  public get changeFirstName() {
    const changeFirstName = this.$apiClient.$makeCaller(
      this.$metadata.methods.changeFirstName,
      (c, firstName: string | null, title?: $models.Titles | null) => c.changeFirstName(this.$primaryKey, firstName, title),
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
  static DataSources = $models.Person.DataSources;
  
  /** 
    Adds two numbers.
    
    This comment also includes multiple lines so I can test multi-line xmldoc comments.
  */
  public get add() {
    const add = this.$apiClient.$makeCaller(
      this.$metadata.methods.add,
      (c, numberOne?: number | null, numberTwo?: number | null) => c.add(numberOne, numberTwo),
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
  
  /** Returns a count of all people in the database whose last name starts with the specified value. */
  public get personCount() {
    const personCount = this.$apiClient.$makeCaller(
      this.$metadata.methods.personCount,
      (c, lastNameStartsWith?: string | null) => c.personCount(lastNameStartsWith),
      () => ({lastNameStartsWith: null as string | null, }),
      (c, args) => c.personCount(args.lastNameStartsWith))
    
    Object.defineProperty(this, 'personCount', {value: personCount});
    return personCount
  }
  
  public get removePersonById() {
    const removePersonById = this.$apiClient.$makeCaller(
      this.$metadata.methods.removePersonById,
      (c, id?: number | null) => c.removePersonById(id),
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
  
  public get methodWithStringArrayParameter() {
    const methodWithStringArrayParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithStringArrayParameter,
      (c, strings: string[] | null) => c.methodWithStringArrayParameter(strings),
      () => ({strings: null as string[] | null, }),
      (c, args) => c.methodWithStringArrayParameter(args.strings))
    
    Object.defineProperty(this, 'methodWithStringArrayParameter', {value: methodWithStringArrayParameter});
    return methodWithStringArrayParameter
  }
  
  public get methodWithEntityParameter() {
    const methodWithEntityParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithEntityParameter,
      (c, person: $models.Person | null, people: $models.Person[] | null) => c.methodWithEntityParameter(person, people),
      () => ({person: null as $models.Person | null, people: null as $models.Person[] | null, }),
      (c, args) => c.methodWithEntityParameter(args.person, args.people))
    
    Object.defineProperty(this, 'methodWithEntityParameter', {value: methodWithEntityParameter});
    return methodWithEntityParameter
  }
  
  public get methodWithOptionalEntityParameter() {
    const methodWithOptionalEntityParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOptionalEntityParameter,
      (c, person?: $models.Person | null) => c.methodWithOptionalEntityParameter(person),
      () => ({person: null as $models.Person | null, }),
      (c, args) => c.methodWithOptionalEntityParameter(args.person))
    
    Object.defineProperty(this, 'methodWithOptionalEntityParameter', {value: methodWithOptionalEntityParameter});
    return methodWithOptionalEntityParameter
  }
  
  public get methodWithExplicitlyInjectedDataSource() {
    const methodWithExplicitlyInjectedDataSource = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithExplicitlyInjectedDataSource,
      (c) => c.methodWithExplicitlyInjectedDataSource(),
      () => ({}),
      (c, args) => c.methodWithExplicitlyInjectedDataSource())
    
    Object.defineProperty(this, 'methodWithExplicitlyInjectedDataSource', {value: methodWithExplicitlyInjectedDataSource});
    return methodWithExplicitlyInjectedDataSource
  }
  
  /** Gets people matching the criteria, paginated by parameter 'page'. */
  public get searchPeople() {
    const searchPeople = this.$apiClient.$makeCaller(
      this.$metadata.methods.searchPeople,
      (c, criteria: $models.PersonCriteria | null, page?: number | null) => c.searchPeople(criteria, page),
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
  unknown: unknown | null;
}
export class ProductViewModel extends ViewModel<$models.Product, $apiClients.ProductApiClient, number> implements $models.Product  {
  static DataSources = $models.Product.DataSources;
  
  constructor(initialData?: DeepPartial<$models.Product> | null) {
    super($metadata.Product, new $apiClients.ProductApiClient(), initialData)
  }
}
defineProps(ProductViewModel, $metadata.Product)

export class ProductListViewModel extends ListViewModel<$models.Product, $apiClients.ProductApiClient, ProductViewModel> {
  static DataSources = $models.Product.DataSources;
  
  constructor() {
    super($metadata.Product, new $apiClients.ProductApiClient())
  }
}


export interface StandaloneReadCreateViewModel extends $models.StandaloneReadCreate {
  id: number | null;
  name: string | null;
  date: Date | null;
}
export class StandaloneReadCreateViewModel extends ViewModel<$models.StandaloneReadCreate, $apiClients.StandaloneReadCreateApiClient, number> implements $models.StandaloneReadCreate  {
  static DataSources = $models.StandaloneReadCreate.DataSources;
  
  constructor(initialData?: DeepPartial<$models.StandaloneReadCreate> | null) {
    super($metadata.StandaloneReadCreate, new $apiClients.StandaloneReadCreateApiClient(), initialData)
  }
}
defineProps(StandaloneReadCreateViewModel, $metadata.StandaloneReadCreate)

export class StandaloneReadCreateListViewModel extends ListViewModel<$models.StandaloneReadCreate, $apiClients.StandaloneReadCreateApiClient, StandaloneReadCreateViewModel> {
  static DataSources = $models.StandaloneReadCreate.DataSources;
  
  constructor() {
    super($metadata.StandaloneReadCreate, new $apiClients.StandaloneReadCreateApiClient())
  }
}


export interface StandaloneReadonlyViewModel extends $models.StandaloneReadonly {
  id: number | null;
  name: string | null;
  description: string | null;
}
export class StandaloneReadonlyViewModel extends ViewModel<$models.StandaloneReadonly, $apiClients.StandaloneReadonlyApiClient, number> implements $models.StandaloneReadonly  {
  static DataSources = $models.StandaloneReadonly.DataSources;
  
  constructor(initialData?: DeepPartial<$models.StandaloneReadonly> | null) {
    super($metadata.StandaloneReadonly, new $apiClients.StandaloneReadonlyApiClient(), initialData)
  }
}
defineProps(StandaloneReadonlyViewModel, $metadata.StandaloneReadonly)

export class StandaloneReadonlyListViewModel extends ListViewModel<$models.StandaloneReadonly, $apiClients.StandaloneReadonlyApiClient, StandaloneReadonlyViewModel> {
  static DataSources = $models.StandaloneReadonly.DataSources;
  
  constructor() {
    super($metadata.StandaloneReadonly, new $apiClients.StandaloneReadonlyApiClient())
  }
}


export interface StandaloneReadWriteViewModel extends $models.StandaloneReadWrite {
  id: number | null;
  name: string | null;
  date: Date | null;
}
export class StandaloneReadWriteViewModel extends ViewModel<$models.StandaloneReadWrite, $apiClients.StandaloneReadWriteApiClient, number> implements $models.StandaloneReadWrite  {
  static DataSources = $models.StandaloneReadWrite.DataSources;
  
  constructor(initialData?: DeepPartial<$models.StandaloneReadWrite> | null) {
    super($metadata.StandaloneReadWrite, new $apiClients.StandaloneReadWriteApiClient(), initialData)
  }
}
defineProps(StandaloneReadWriteViewModel, $metadata.StandaloneReadWrite)

export class StandaloneReadWriteListViewModel extends ListViewModel<$models.StandaloneReadWrite, $apiClients.StandaloneReadWriteApiClient, StandaloneReadWriteViewModel> {
  static DataSources = $models.StandaloneReadWrite.DataSources;
  
  constructor() {
    super($metadata.StandaloneReadWrite, new $apiClients.StandaloneReadWriteApiClient())
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


export class AIAgentServiceViewModel extends ServiceViewModel<typeof $metadata.AIAgentService, $apiClients.AIAgentServiceApiClient> {
  
  /** A chat agent that orchestrates other agents */
  public get orchestratedAgent() {
    const orchestratedAgent = this.$apiClient.$makeCaller(
      this.$metadata.methods.orchestratedAgent,
      (c, history: string | null, prompt: string | null) => c.orchestratedAgent(history, prompt),
      () => ({history: null as string | null, prompt: null as string | null, }),
      (c, args) => c.orchestratedAgent(args.history, args.prompt))
    
    Object.defineProperty(this, 'orchestratedAgent', {value: orchestratedAgent});
    return orchestratedAgent
  }
  
  /** A chat agent that delegates to other chat completion services. */
  public get metaCompletionAgent() {
    const metaCompletionAgent = this.$apiClient.$makeCaller(
      this.$metadata.methods.metaCompletionAgent,
      (c, history: string | null, prompt: string | null) => c.metaCompletionAgent(history, prompt),
      () => ({history: null as string | null, prompt: null as string | null, }),
      (c, args) => c.metaCompletionAgent(args.history, args.prompt))
    
    Object.defineProperty(this, 'metaCompletionAgent', {value: metaCompletionAgent});
    return metaCompletionAgent
  }
  
  /** A chat agent that directly uses all kernel plugin tools. */
  public get omniToolAgent() {
    const omniToolAgent = this.$apiClient.$makeCaller(
      this.$metadata.methods.omniToolAgent,
      (c, history: string | null, prompt: string | null) => c.omniToolAgent(history, prompt),
      () => ({history: null as string | null, prompt: null as string | null, }),
      (c, args) => c.omniToolAgent(args.history, args.prompt))
    
    Object.defineProperty(this, 'omniToolAgent', {value: omniToolAgent});
    return omniToolAgent
  }
  
  public get personAgent() {
    const personAgent = this.$apiClient.$makeCaller(
      this.$metadata.methods.personAgent,
      (c, prompt: string | null) => c.personAgent(prompt),
      () => ({prompt: null as string | null, }),
      (c, args) => c.personAgent(args.prompt))
    
    Object.defineProperty(this, 'personAgent', {value: personAgent});
    return personAgent
  }
  
  public get productAgent() {
    const productAgent = this.$apiClient.$makeCaller(
      this.$metadata.methods.productAgent,
      (c, prompt: string | null) => c.productAgent(prompt),
      () => ({prompt: null as string | null, }),
      (c, args) => c.productAgent(args.prompt))
    
    Object.defineProperty(this, 'productAgent', {value: productAgent});
    return productAgent
  }
  
  constructor() {
    super($metadata.AIAgentService, new $apiClients.AIAgentServiceApiClient())
  }
}


export class WeatherServiceViewModel extends ServiceViewModel<typeof $metadata.WeatherService, $apiClients.WeatherServiceApiClient> {
  
  public get getWeather() {
    const getWeather = this.$apiClient.$makeCaller(
      this.$metadata.methods.getWeather,
      (c, location: $models.Location | null, dateTime?: Date | null, conditions?: $models.SkyConditions | null) => c.getWeather(location, dateTime, conditions),
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
  AbstractClassImpl: AbstractClassImplViewModel,
  AbstractClassPerson: AbstractClassPersonViewModel,
  AuditLog: AuditLogViewModel,
  AuditLogProperty: AuditLogPropertyViewModel,
  BaseClass: BaseClassViewModel,
  BaseClassDerived: BaseClassDerivedViewModel,
  Case: CaseViewModel,
  CaseDto: CaseDtoViewModel,
  CaseDtoStandalone: CaseDtoStandaloneViewModel,
  CaseProduct: CaseProductViewModel,
  CaseStandalone: CaseStandaloneViewModel,
  Company: CompanyViewModel,
  Log: LogViewModel,
  Person: PersonViewModel,
  Product: ProductViewModel,
  StandaloneReadCreate: StandaloneReadCreateViewModel,
  StandaloneReadonly: StandaloneReadonlyViewModel,
  StandaloneReadWrite: StandaloneReadWriteViewModel,
  ZipCode: ZipCodeViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  AbstractClass: AbstractClassListViewModel,
  AbstractClassImpl: AbstractClassImplListViewModel,
  AbstractClassPerson: AbstractClassPersonListViewModel,
  AuditLog: AuditLogListViewModel,
  AuditLogProperty: AuditLogPropertyListViewModel,
  BaseClass: BaseClassListViewModel,
  BaseClassDerived: BaseClassDerivedListViewModel,
  Case: CaseListViewModel,
  CaseDto: CaseDtoListViewModel,
  CaseDtoStandalone: CaseDtoStandaloneListViewModel,
  CaseProduct: CaseProductListViewModel,
  CaseStandalone: CaseStandaloneListViewModel,
  Company: CompanyListViewModel,
  Log: LogListViewModel,
  Person: PersonListViewModel,
  Product: ProductListViewModel,
  StandaloneReadCreate: StandaloneReadCreateListViewModel,
  StandaloneReadonly: StandaloneReadonlyListViewModel,
  StandaloneReadWrite: StandaloneReadWriteListViewModel,
  ZipCode: ZipCodeListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  AIAgentService: AIAgentServiceViewModel,
  WeatherService: WeatherServiceViewModel,
}

