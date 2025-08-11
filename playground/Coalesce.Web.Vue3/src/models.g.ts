import * as metadata from './metadata.g'
import { convertToModel, mapToModel, reactiveDataSource } from 'coalesce-vue/lib/model'
import type { Model, DataSource } from 'coalesce-vue/lib/model'
import type { ClassType } from 'coalesce-vue/lib/metadata'

export enum AuditEntryState {
  EntityAdded = 0,
  EntityDeleted = 1,
  EntityModified = 2,
}


export enum Genders {
  NonSpecified = 0,
  Male = 1,
  Female = 2,
}


export enum Statuses {
  Open = 0,
  InProgress = 1,
  Resolved = 2,
  
  /** The case was closed without being solved. */
  ClosedNoSolution = 3,
  
  /** Case closed. */
  Cancelled = 4,
}


export enum Titles {
  Mr = 0,
  Ms = 1,
  Mrs = 2,
  Miss = 4,
}


export interface AbstractClass<TMeta extends ClassType = typeof metadata.AbstractClass | typeof metadata.AbstractClassImpl> extends Model<TMeta> {
  id: number | null
  abstractClassString: string | null
  abstractModelPeople: AbstractClassPerson[] | null
}
export class AbstractClass {
  
  /** Mutates the input object and its descendants into a valid AbstractClass implementation. */
  static convert(data?: Partial<AbstractClass>): AbstractClass {
    return convertToModel<AbstractClass>(data || {}, metadata.AbstractClass) 
  }
  
  /** Maps the input object and its descendants to a new, valid AbstractClass implementation. */
  static map(data?: Partial<AbstractClass>): AbstractClass {
    return mapToModel<AbstractClass>(data || {}, metadata.AbstractClass) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AbstractClass || x?.$metadata.baseTypes?.includes(metadata.AbstractClass); }
  
  /** Instantiate a new AbstractClass, optionally basing it on the given data. */
  constructor(data?: Partial<AbstractClass> | {[k: string]: any}) {
    Object.assign(this, AbstractClass.map(data || {}));
  }
}


export interface AbstractClassImpl extends Model<typeof metadata.AbstractClassImpl> {
  implString: string | null
  id: number | null
  abstractClassString: string | null
  abstractModelPeople: AbstractClassPerson[] | null
}
export class AbstractClassImpl {
  
  /** Mutates the input object and its descendants into a valid AbstractClassImpl implementation. */
  static convert(data?: Partial<AbstractClassImpl>): AbstractClassImpl {
    return convertToModel<AbstractClassImpl>(data || {}, metadata.AbstractClassImpl) 
  }
  
  /** Maps the input object and its descendants to a new, valid AbstractClassImpl implementation. */
  static map(data?: Partial<AbstractClassImpl>): AbstractClassImpl {
    return mapToModel<AbstractClassImpl>(data || {}, metadata.AbstractClassImpl) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AbstractClassImpl; }
  
  /** Instantiate a new AbstractClassImpl, optionally basing it on the given data. */
  constructor(data?: Partial<AbstractClassImpl> | {[k: string]: any}) {
    Object.assign(this, AbstractClassImpl.map(data || {}));
  }
}


export interface AbstractClassPerson extends Model<typeof metadata.AbstractClassPerson> {
  id: number | null
  personId: number | null
  person: Person | null
  abstractClassId: number | null
  abstractClass: AbstractClass | null
}
export class AbstractClassPerson {
  
  /** Mutates the input object and its descendants into a valid AbstractClassPerson implementation. */
  static convert(data?: Partial<AbstractClassPerson>): AbstractClassPerson {
    return convertToModel<AbstractClassPerson>(data || {}, metadata.AbstractClassPerson) 
  }
  
  /** Maps the input object and its descendants to a new, valid AbstractClassPerson implementation. */
  static map(data?: Partial<AbstractClassPerson>): AbstractClassPerson {
    return mapToModel<AbstractClassPerson>(data || {}, metadata.AbstractClassPerson) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AbstractClassPerson; }
  
  /** Instantiate a new AbstractClassPerson, optionally basing it on the given data. */
  constructor(data?: Partial<AbstractClassPerson> | {[k: string]: any}) {
    Object.assign(this, AbstractClassPerson.map(data || {}));
  }
}


export interface AuditLog extends Model<typeof metadata.AuditLog> {
  message: string | null
  userId: number | null
  user: Person | null
  id: number | null
  type: string | null
  keyValue: string | null
  description: string | null
  state: AuditEntryState | null
  date: Date | null
  properties: AuditLogProperty[] | null
  clientIp: string | null
  referrer: string | null
  endpoint: string | null
}
export class AuditLog {
  
  /** Mutates the input object and its descendants into a valid AuditLog implementation. */
  static convert(data?: Partial<AuditLog>): AuditLog {
    return convertToModel<AuditLog>(data || {}, metadata.AuditLog) 
  }
  
  /** Maps the input object and its descendants to a new, valid AuditLog implementation. */
  static map(data?: Partial<AuditLog>): AuditLog {
    return mapToModel<AuditLog>(data || {}, metadata.AuditLog) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLog; }
  
  /** Instantiate a new AuditLog, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLog> | {[k: string]: any}) {
    Object.assign(this, AuditLog.map(data || {}));
  }
}


export interface AuditLogProperty extends Model<typeof metadata.AuditLogProperty> {
  id: number | null
  parentId: number | null
  propertyName: string | null
  oldValue: string | null
  oldValueDescription: string | null
  newValue: string | null
  newValueDescription: string | null
}
export class AuditLogProperty {
  
  /** Mutates the input object and its descendants into a valid AuditLogProperty implementation. */
  static convert(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return convertToModel<AuditLogProperty>(data || {}, metadata.AuditLogProperty) 
  }
  
  /** Maps the input object and its descendants to a new, valid AuditLogProperty implementation. */
  static map(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return mapToModel<AuditLogProperty>(data || {}, metadata.AuditLogProperty) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLogProperty; }
  
  /** Instantiate a new AuditLogProperty, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLogProperty> | {[k: string]: any}) {
    Object.assign(this, AuditLogProperty.map(data || {}));
  }
}


export interface BaseClass<TMeta extends ClassType = typeof metadata.BaseClass | typeof metadata.BaseClassDerived> extends Model<TMeta> {
  id: number | null
  baseClassString: string | null
}
export class BaseClass {
  
  /** Mutates the input object and its descendants into a valid BaseClass implementation. */
  static convert(data?: Partial<BaseClass>): BaseClass {
    return convertToModel<BaseClass>(data || {}, metadata.BaseClass) 
  }
  
  /** Maps the input object and its descendants to a new, valid BaseClass implementation. */
  static map(data?: Partial<BaseClass>): BaseClass {
    return mapToModel<BaseClass>(data || {}, metadata.BaseClass) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.BaseClass || x?.$metadata.baseTypes?.includes(metadata.BaseClass); }
  
  /** Instantiate a new BaseClass, optionally basing it on the given data. */
  constructor(data?: Partial<BaseClass> | {[k: string]: any}) {
    Object.assign(this, BaseClass.map(data || {}));
  }
}


export interface BaseClassDerived extends Model<typeof metadata.BaseClassDerived> {
  derivedClassString: string | null
  id: number | null
  baseClassString: string | null
}
export class BaseClassDerived {
  
  /** Mutates the input object and its descendants into a valid BaseClassDerived implementation. */
  static convert(data?: Partial<BaseClassDerived>): BaseClassDerived {
    return convertToModel<BaseClassDerived>(data || {}, metadata.BaseClassDerived) 
  }
  
  /** Maps the input object and its descendants to a new, valid BaseClassDerived implementation. */
  static map(data?: Partial<BaseClassDerived>): BaseClassDerived {
    return mapToModel<BaseClassDerived>(data || {}, metadata.BaseClassDerived) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.BaseClassDerived; }
  
  /** Instantiate a new BaseClassDerived, optionally basing it on the given data. */
  constructor(data?: Partial<BaseClassDerived> | {[k: string]: any}) {
    Object.assign(this, BaseClassDerived.map(data || {}));
  }
}


export interface Case extends Model<typeof metadata.Case> {
  
  /** The Primary key for the Case object */
  caseKey: number | null
  title: string | null
  
  /** User-provided description of the issue */
  description: string | null
  
  /** Date and time when the case was opened */
  openedAt: Date | null
  assignedToId: number | null
  assignedTo: Person | null
  reportedById: number | null
  
  /** Person who originally reported the case */
  reportedBy: Person | null
  attachmentSize: number | null
  attachmentName: string | null
  attachmentType: string | null
  attachmentHash: string | null
  severity: string | null
  status: Statuses | null
  numbers: number[] | null
  strings: string[] | null
  states: Statuses[] | null
  caseProducts: CaseProduct[] | null
  devTeamAssignedId: number | null
  devTeamAssigned: DevTeam | null
  duration: unknown | null
}
export class Case {
  
  static magicNumber = 42
  static magicString = "42"
  static magicEnum = Statuses.ClosedNoSolution
  
  /** Mutates the input object and its descendants into a valid Case implementation. */
  static convert(data?: Partial<Case>): Case {
    return convertToModel<Case>(data || {}, metadata.Case) 
  }
  
  /** Maps the input object and its descendants to a new, valid Case implementation. */
  static map(data?: Partial<Case>): Case {
    return mapToModel<Case>(data || {}, metadata.Case) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Case; }
  
  /** Instantiate a new Case, optionally basing it on the given data. */
  constructor(data?: Partial<Case> | {[k: string]: any}) {
    Object.assign(this, Case.map(data || {}));
  }
}
export namespace Case {
  export namespace DataSources {
    
    export class AllOpenCases implements DataSource<typeof metadata.Case.dataSources.allOpenCases> {
      readonly $metadata = metadata.Case.dataSources.allOpenCases
      
      /** Only include cases opened on or after this date */
      minDate: Date | null = null
      
      constructor(params?: Omit<Partial<AllOpenCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
    }
    
    export class MissingManyToManyFarSide implements DataSource<typeof metadata.Case.dataSources.missingManyToManyFarSide> {
      readonly $metadata = metadata.Case.dataSources.missingManyToManyFarSide
    }
  }
}


export interface CaseDto extends Model<typeof metadata.CaseDto> {
  caseId: number | null
  title: string | null
  assignedToName: string | null
}
export class CaseDto {
  
  /** Mutates the input object and its descendants into a valid CaseDto implementation. */
  static convert(data?: Partial<CaseDto>): CaseDto {
    return convertToModel<CaseDto>(data || {}, metadata.CaseDto) 
  }
  
  /** Maps the input object and its descendants to a new, valid CaseDto implementation. */
  static map(data?: Partial<CaseDto>): CaseDto {
    return mapToModel<CaseDto>(data || {}, metadata.CaseDto) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseDto; }
  
  /** Instantiate a new CaseDto, optionally basing it on the given data. */
  constructor(data?: Partial<CaseDto> | {[k: string]: any}) {
    Object.assign(this, CaseDto.map(data || {}));
  }
}
export namespace CaseDto {
  export namespace DataSources {
    
    export class CaseDtoSource implements DataSource<typeof metadata.CaseDto.dataSources.caseDtoSource> {
      readonly $metadata = metadata.CaseDto.dataSources.caseDtoSource
    }
  }
}


export interface CaseDtoStandalone extends Model<typeof metadata.CaseDtoStandalone> {
  caseId: number | null
  title: string | null
}
export class CaseDtoStandalone {
  
  /** Mutates the input object and its descendants into a valid CaseDtoStandalone implementation. */
  static convert(data?: Partial<CaseDtoStandalone>): CaseDtoStandalone {
    return convertToModel<CaseDtoStandalone>(data || {}, metadata.CaseDtoStandalone) 
  }
  
  /** Maps the input object and its descendants to a new, valid CaseDtoStandalone implementation. */
  static map(data?: Partial<CaseDtoStandalone>): CaseDtoStandalone {
    return mapToModel<CaseDtoStandalone>(data || {}, metadata.CaseDtoStandalone) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseDtoStandalone; }
  
  /** Instantiate a new CaseDtoStandalone, optionally basing it on the given data. */
  constructor(data?: Partial<CaseDtoStandalone> | {[k: string]: any}) {
    Object.assign(this, CaseDtoStandalone.map(data || {}));
  }
}


export interface CaseProduct extends Model<typeof metadata.CaseProduct> {
  caseProductId: number | null
  caseId: number | null
  case: Case | null
  productId: number | null
  product: Product | null
}
export class CaseProduct {
  
  /** Mutates the input object and its descendants into a valid CaseProduct implementation. */
  static convert(data?: Partial<CaseProduct>): CaseProduct {
    return convertToModel<CaseProduct>(data || {}, metadata.CaseProduct) 
  }
  
  /** Maps the input object and its descendants to a new, valid CaseProduct implementation. */
  static map(data?: Partial<CaseProduct>): CaseProduct {
    return mapToModel<CaseProduct>(data || {}, metadata.CaseProduct) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseProduct; }
  
  /** Instantiate a new CaseProduct, optionally basing it on the given data. */
  constructor(data?: Partial<CaseProduct> | {[k: string]: any}) {
    Object.assign(this, CaseProduct.map(data || {}));
  }
}


export interface Company extends Model<typeof metadata.Company> {
  id: number | null
  name: string | null
  address1: string | null
  address2: string | null
  city: string | null
  state: string | null
  zipCode: string | null
  phone: string | null
  websiteUrl: string | null
  logoUrl: string | null
  isDeleted: boolean | null
  employees: Person[] | null
  altName: string | null
}
export class Company {
  
  /** Mutates the input object and its descendants into a valid Company implementation. */
  static convert(data?: Partial<Company>): Company {
    return convertToModel<Company>(data || {}, metadata.Company) 
  }
  
  /** Maps the input object and its descendants to a new, valid Company implementation. */
  static map(data?: Partial<Company>): Company {
    return mapToModel<Company>(data || {}, metadata.Company) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Company; }
  
  /** Instantiate a new Company, optionally basing it on the given data. */
  constructor(data?: Partial<Company> | {[k: string]: any}) {
    Object.assign(this, Company.map(data || {}));
  }
}
export namespace Company {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.Company.dataSources.defaultSource> {
      readonly $metadata = metadata.Company.dataSources.defaultSource
    }
  }
}


export interface DateOnlyPk extends Model<typeof metadata.DateOnlyPk> {
  dateOnlyPkId: Date | null
  name: string | null
}
export class DateOnlyPk {
  
  /** Mutates the input object and its descendants into a valid DateOnlyPk implementation. */
  static convert(data?: Partial<DateOnlyPk>): DateOnlyPk {
    return convertToModel<DateOnlyPk>(data || {}, metadata.DateOnlyPk) 
  }
  
  /** Maps the input object and its descendants to a new, valid DateOnlyPk implementation. */
  static map(data?: Partial<DateOnlyPk>): DateOnlyPk {
    return mapToModel<DateOnlyPk>(data || {}, metadata.DateOnlyPk) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.DateOnlyPk; }
  
  /** Instantiate a new DateOnlyPk, optionally basing it on the given data. */
  constructor(data?: Partial<DateOnlyPk> | {[k: string]: any}) {
    Object.assign(this, DateOnlyPk.map(data || {}));
  }
}


export interface Log extends Model<typeof metadata.Log> {
  logId: number | null
  level: string | null
  message: string | null
}
export class Log {
  
  /** Mutates the input object and its descendants into a valid Log implementation. */
  static convert(data?: Partial<Log>): Log {
    return convertToModel<Log>(data || {}, metadata.Log) 
  }
  
  /** Maps the input object and its descendants to a new, valid Log implementation. */
  static map(data?: Partial<Log>): Log {
    return mapToModel<Log>(data || {}, metadata.Log) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Log; }
  
  /** Instantiate a new Log, optionally basing it on the given data. */
  constructor(data?: Partial<Log> | {[k: string]: any}) {
    Object.assign(this, Log.map(data || {}));
  }
}


export interface Person extends Model<typeof metadata.Person> {
  
  /** ID for the person object. */
  personId: number | null
  
  /** Title of the person, Mr. Mrs, etc. */
  title: Titles | null
  
  /** First name of the person. */
  firstName: string | null
  
  /** Last name of the person */
  lastName: string | null
  
  /** Email address of the person */
  email: string | null
  
  /** Genetic Gender of the person. */
  gender: Genders | null
  height: number | null
  
  /** List of cases assigned to the person */
  casesAssigned: Case[] | null
  
  /** List of cases reported by the person. */
  casesReported: Case[] | null
  birthDate: Date | null
  lastBath: Date | null
  nextUpgrade: Date | null
  personStats: PersonStats | null
  profilePic: string | null
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null
  
  /** Company ID this person is employed by */
  companyId: number | null
  
  /** Company loaded from the Company ID */
  company: Company | null
  arbitraryCollectionOfStrings: string[] | null
}
export class Person {
  
  /** Mutates the input object and its descendants into a valid Person implementation. */
  static convert(data?: Partial<Person>): Person {
    return convertToModel<Person>(data || {}, metadata.Person) 
  }
  
  /** Maps the input object and its descendants to a new, valid Person implementation. */
  static map(data?: Partial<Person>): Person {
    return mapToModel<Person>(data || {}, metadata.Person) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Person; }
  
  /** Instantiate a new Person, optionally basing it on the given data. */
  constructor(data?: Partial<Person> | {[k: string]: any}) {
    Object.assign(this, Person.map(data || {}));
  }
}
export namespace Person {
  export namespace DataSources {
    
    /** People whose last name starts with B or c */
    export class BOrCPeople implements DataSource<typeof metadata.Person.dataSources.bOrCPeople> {
      readonly $metadata = metadata.Person.dataSources.bOrCPeople
    }
    
    export class NamesStartingWithAWithCases implements DataSource<typeof metadata.Person.dataSources.namesStartingWithAWithCases> {
      readonly $metadata = metadata.Person.dataSources.namesStartingWithAWithCases
      allowedStatuses: Statuses[] | null = null
      
      constructor(params?: Omit<Partial<NamesStartingWithAWithCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
    }
    
    export class WithoutCases implements DataSource<typeof metadata.Person.dataSources.withoutCases> {
      readonly $metadata = metadata.Person.dataSources.withoutCases
      personCriteria: PersonCriteria | null = null
      
      constructor(params?: Omit<Partial<WithoutCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
    }
  }
}


export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
  details: ProductDetails | null
  uniqueId: string | null
  milestoneId: Date | null
  
  /** Product milestone date */
  milestone: DateOnlyPk | null
  unknown: unknown | null
}
export class Product {
  
  /** Mutates the input object and its descendants into a valid Product implementation. */
  static convert(data?: Partial<Product>): Product {
    return convertToModel<Product>(data || {}, metadata.Product) 
  }
  
  /** Maps the input object and its descendants to a new, valid Product implementation. */
  static map(data?: Partial<Product>): Product {
    return mapToModel<Product>(data || {}, metadata.Product) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Product; }
  
  /** Instantiate a new Product, optionally basing it on the given data. */
  constructor(data?: Partial<Product> | {[k: string]: any}) {
    Object.assign(this, Product.map(data || {}));
  }
}


export interface ZipCode extends Model<typeof metadata.ZipCode> {
  zip: string | null
  state: string | null
}
export class ZipCode {
  
  /** Mutates the input object and its descendants into a valid ZipCode implementation. */
  static convert(data?: Partial<ZipCode>): ZipCode {
    return convertToModel<ZipCode>(data || {}, metadata.ZipCode) 
  }
  
  /** Maps the input object and its descendants to a new, valid ZipCode implementation. */
  static map(data?: Partial<ZipCode>): ZipCode {
    return mapToModel<ZipCode>(data || {}, metadata.ZipCode) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ZipCode; }
  
  /** Instantiate a new ZipCode, optionally basing it on the given data. */
  constructor(data?: Partial<ZipCode> | {[k: string]: any}) {
    Object.assign(this, ZipCode.map(data || {}));
  }
}


export interface CaseStandalone extends Model<typeof metadata.CaseStandalone> {
  id: number | null
  assignedTo: Person | null
}
export class CaseStandalone {
  
  /** Mutates the input object and its descendants into a valid CaseStandalone implementation. */
  static convert(data?: Partial<CaseStandalone>): CaseStandalone {
    return convertToModel<CaseStandalone>(data || {}, metadata.CaseStandalone) 
  }
  
  /** Maps the input object and its descendants to a new, valid CaseStandalone implementation. */
  static map(data?: Partial<CaseStandalone>): CaseStandalone {
    return mapToModel<CaseStandalone>(data || {}, metadata.CaseStandalone) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseStandalone; }
  
  /** Instantiate a new CaseStandalone, optionally basing it on the given data. */
  constructor(data?: Partial<CaseStandalone> | {[k: string]: any}) {
    Object.assign(this, CaseStandalone.map(data || {}));
  }
}
export namespace CaseStandalone {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.CaseStandalone.dataSources.defaultSource> {
      readonly $metadata = metadata.CaseStandalone.dataSources.defaultSource
    }
  }
}


export interface CaseSummary extends Model<typeof metadata.CaseSummary> {
  caseSummaryId: number | null
  openCases: number | null
  caseCount: number | null
  closeCases: number | null
  description: string | null
  testDict: unknown[] | null
}
export class CaseSummary {
  
  /** Mutates the input object and its descendants into a valid CaseSummary implementation. */
  static convert(data?: Partial<CaseSummary>): CaseSummary {
    return convertToModel<CaseSummary>(data || {}, metadata.CaseSummary) 
  }
  
  /** Maps the input object and its descendants to a new, valid CaseSummary implementation. */
  static map(data?: Partial<CaseSummary>): CaseSummary {
    return mapToModel<CaseSummary>(data || {}, metadata.CaseSummary) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseSummary; }
  
  /** Instantiate a new CaseSummary, optionally basing it on the given data. */
  constructor(data?: Partial<CaseSummary> | {[k: string]: any}) {
    Object.assign(this, CaseSummary.map(data || {}));
  }
}


export interface ChatResponse extends Model<typeof metadata.ChatResponse> {
  response: string | null
  history: string | null
}
export class ChatResponse {
  
  /** Mutates the input object and its descendants into a valid ChatResponse implementation. */
  static convert(data?: Partial<ChatResponse>): ChatResponse {
    return convertToModel<ChatResponse>(data || {}, metadata.ChatResponse) 
  }
  
  /** Maps the input object and its descendants to a new, valid ChatResponse implementation. */
  static map(data?: Partial<ChatResponse>): ChatResponse {
    return mapToModel<ChatResponse>(data || {}, metadata.ChatResponse) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ChatResponse; }
  
  /** Instantiate a new ChatResponse, optionally basing it on the given data. */
  constructor(data?: Partial<ChatResponse> | {[k: string]: any}) {
    Object.assign(this, ChatResponse.map(data || {}));
  }
}


export interface DevTeam extends Model<typeof metadata.DevTeam> {
  devTeamId: number | null
  name: string | null
}
export class DevTeam {
  
  /** Mutates the input object and its descendants into a valid DevTeam implementation. */
  static convert(data?: Partial<DevTeam>): DevTeam {
    return convertToModel<DevTeam>(data || {}, metadata.DevTeam) 
  }
  
  /** Maps the input object and its descendants to a new, valid DevTeam implementation. */
  static map(data?: Partial<DevTeam>): DevTeam {
    return mapToModel<DevTeam>(data || {}, metadata.DevTeam) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.DevTeam; }
  
  /** Instantiate a new DevTeam, optionally basing it on the given data. */
  constructor(data?: Partial<DevTeam> | {[k: string]: any}) {
    Object.assign(this, DevTeam.map(data || {}));
  }
}


export interface Location extends Model<typeof metadata.Location> {
  city: string | null
  state: string | null
  zip: string | null
}
export class Location {
  
  /** Mutates the input object and its descendants into a valid Location implementation. */
  static convert(data?: Partial<Location>): Location {
    return convertToModel<Location>(data || {}, metadata.Location) 
  }
  
  /** Maps the input object and its descendants to a new, valid Location implementation. */
  static map(data?: Partial<Location>): Location {
    return mapToModel<Location>(data || {}, metadata.Location) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Location; }
  
  /** Instantiate a new Location, optionally basing it on the given data. */
  constructor(data?: Partial<Location> | {[k: string]: any}) {
    Object.assign(this, Location.map(data || {}));
  }
}


export interface PersonCriteria extends Model<typeof metadata.PersonCriteria> {
  name: string | null
  birthdayMonth: number | null
  emailDomain: string | null
}
export class PersonCriteria {
  
  /** Mutates the input object and its descendants into a valid PersonCriteria implementation. */
  static convert(data?: Partial<PersonCriteria>): PersonCriteria {
    return convertToModel<PersonCriteria>(data || {}, metadata.PersonCriteria) 
  }
  
  /** Maps the input object and its descendants to a new, valid PersonCriteria implementation. */
  static map(data?: Partial<PersonCriteria>): PersonCriteria {
    return mapToModel<PersonCriteria>(data || {}, metadata.PersonCriteria) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.PersonCriteria; }
  
  /** Instantiate a new PersonCriteria, optionally basing it on the given data. */
  constructor(data?: Partial<PersonCriteria> | {[k: string]: any}) {
    Object.assign(this, PersonCriteria.map(data || {}));
  }
}


export interface PersonLocation extends Model<typeof metadata.PersonLocation> {
  latitude: number | null
  longitude: number | null
}
export class PersonLocation {
  
  /** Mutates the input object and its descendants into a valid PersonLocation implementation. */
  static convert(data?: Partial<PersonLocation>): PersonLocation {
    return convertToModel<PersonLocation>(data || {}, metadata.PersonLocation) 
  }
  
  /** Maps the input object and its descendants to a new, valid PersonLocation implementation. */
  static map(data?: Partial<PersonLocation>): PersonLocation {
    return mapToModel<PersonLocation>(data || {}, metadata.PersonLocation) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.PersonLocation; }
  
  /** Instantiate a new PersonLocation, optionally basing it on the given data. */
  constructor(data?: Partial<PersonLocation> | {[k: string]: any}) {
    Object.assign(this, PersonLocation.map(data || {}));
  }
}


export interface PersonStats extends Model<typeof metadata.PersonStats> {
  height: number | null
  weight: number | null
  name: string | null
  nullableValueTypeCollection: Date[] | null
  valueTypeCollection: Date[] | null
  personLocation: PersonLocation | null
}
export class PersonStats {
  
  /** Mutates the input object and its descendants into a valid PersonStats implementation. */
  static convert(data?: Partial<PersonStats>): PersonStats {
    return convertToModel<PersonStats>(data || {}, metadata.PersonStats) 
  }
  
  /** Maps the input object and its descendants to a new, valid PersonStats implementation. */
  static map(data?: Partial<PersonStats>): PersonStats {
    return mapToModel<PersonStats>(data || {}, metadata.PersonStats) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.PersonStats; }
  
  /** Instantiate a new PersonStats, optionally basing it on the given data. */
  constructor(data?: Partial<PersonStats> | {[k: string]: any}) {
    Object.assign(this, PersonStats.map(data || {}));
  }
}


export interface ProductDetails extends Model<typeof metadata.ProductDetails> {
  manufacturingAddress: StreetAddress | null
  companyHqAddress: StreetAddress | null
}
export class ProductDetails {
  
  /** Mutates the input object and its descendants into a valid ProductDetails implementation. */
  static convert(data?: Partial<ProductDetails>): ProductDetails {
    return convertToModel<ProductDetails>(data || {}, metadata.ProductDetails) 
  }
  
  /** Maps the input object and its descendants to a new, valid ProductDetails implementation. */
  static map(data?: Partial<ProductDetails>): ProductDetails {
    return mapToModel<ProductDetails>(data || {}, metadata.ProductDetails) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ProductDetails; }
  
  /** Instantiate a new ProductDetails, optionally basing it on the given data. */
  constructor(data?: Partial<ProductDetails> | {[k: string]: any}) {
    Object.assign(this, ProductDetails.map(data || {}));
  }
}


export interface StandaloneReadCreate extends Model<typeof metadata.StandaloneReadCreate> {
  id: number | null
  name: string | null
  date: Date | null
}
export class StandaloneReadCreate {
  
  /** Mutates the input object and its descendants into a valid StandaloneReadCreate implementation. */
  static convert(data?: Partial<StandaloneReadCreate>): StandaloneReadCreate {
    return convertToModel<StandaloneReadCreate>(data || {}, metadata.StandaloneReadCreate) 
  }
  
  /** Maps the input object and its descendants to a new, valid StandaloneReadCreate implementation. */
  static map(data?: Partial<StandaloneReadCreate>): StandaloneReadCreate {
    return mapToModel<StandaloneReadCreate>(data || {}, metadata.StandaloneReadCreate) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.StandaloneReadCreate; }
  
  /** Instantiate a new StandaloneReadCreate, optionally basing it on the given data. */
  constructor(data?: Partial<StandaloneReadCreate> | {[k: string]: any}) {
    Object.assign(this, StandaloneReadCreate.map(data || {}));
  }
}
export namespace StandaloneReadCreate {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.StandaloneReadCreate.dataSources.defaultSource> {
      readonly $metadata = metadata.StandaloneReadCreate.dataSources.defaultSource
    }
  }
}


export interface StandaloneReadonly extends Model<typeof metadata.StandaloneReadonly> {
  id: number | null
  name: string | null
  description: string | null
}
export class StandaloneReadonly {
  
  /** Mutates the input object and its descendants into a valid StandaloneReadonly implementation. */
  static convert(data?: Partial<StandaloneReadonly>): StandaloneReadonly {
    return convertToModel<StandaloneReadonly>(data || {}, metadata.StandaloneReadonly) 
  }
  
  /** Maps the input object and its descendants to a new, valid StandaloneReadonly implementation. */
  static map(data?: Partial<StandaloneReadonly>): StandaloneReadonly {
    return mapToModel<StandaloneReadonly>(data || {}, metadata.StandaloneReadonly) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.StandaloneReadonly; }
  
  /** Instantiate a new StandaloneReadonly, optionally basing it on the given data. */
  constructor(data?: Partial<StandaloneReadonly> | {[k: string]: any}) {
    Object.assign(this, StandaloneReadonly.map(data || {}));
  }
}
export namespace StandaloneReadonly {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.StandaloneReadonly.dataSources.defaultSource> {
      readonly $metadata = metadata.StandaloneReadonly.dataSources.defaultSource
    }
  }
}


export interface StandaloneReadWrite extends Model<typeof metadata.StandaloneReadWrite> {
  id: number | null
  name: string | null
  date: Date | null
}
export class StandaloneReadWrite {
  
  /** Mutates the input object and its descendants into a valid StandaloneReadWrite implementation. */
  static convert(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return convertToModel<StandaloneReadWrite>(data || {}, metadata.StandaloneReadWrite) 
  }
  
  /** Maps the input object and its descendants to a new, valid StandaloneReadWrite implementation. */
  static map(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return mapToModel<StandaloneReadWrite>(data || {}, metadata.StandaloneReadWrite) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.StandaloneReadWrite; }
  
  /** Instantiate a new StandaloneReadWrite, optionally basing it on the given data. */
  constructor(data?: Partial<StandaloneReadWrite> | {[k: string]: any}) {
    Object.assign(this, StandaloneReadWrite.map(data || {}));
  }
}
export namespace StandaloneReadWrite {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.StandaloneReadWrite.dataSources.defaultSource> {
      readonly $metadata = metadata.StandaloneReadWrite.dataSources.defaultSource
    }
  }
}


export interface StreetAddress extends Model<typeof metadata.StreetAddress> {
  address: string | null
  city: string | null
  state: string | null
  postalCode: string | null
}
export class StreetAddress {
  
  /** Mutates the input object and its descendants into a valid StreetAddress implementation. */
  static convert(data?: Partial<StreetAddress>): StreetAddress {
    return convertToModel<StreetAddress>(data || {}, metadata.StreetAddress) 
  }
  
  /** Maps the input object and its descendants to a new, valid StreetAddress implementation. */
  static map(data?: Partial<StreetAddress>): StreetAddress {
    return mapToModel<StreetAddress>(data || {}, metadata.StreetAddress) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.StreetAddress; }
  
  /** Instantiate a new StreetAddress, optionally basing it on the given data. */
  constructor(data?: Partial<StreetAddress> | {[k: string]: any}) {
    Object.assign(this, StreetAddress.map(data || {}));
  }
}


export interface WeatherData extends Model<typeof metadata.WeatherData> {
  tempFahrenheit: number | null
  humidity: number | null
  location: Location | null
}
export class WeatherData {
  
  /** Mutates the input object and its descendants into a valid WeatherData implementation. */
  static convert(data?: Partial<WeatherData>): WeatherData {
    return convertToModel<WeatherData>(data || {}, metadata.WeatherData) 
  }
  
  /** Maps the input object and its descendants to a new, valid WeatherData implementation. */
  static map(data?: Partial<WeatherData>): WeatherData {
    return mapToModel<WeatherData>(data || {}, metadata.WeatherData) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.WeatherData; }
  
  /** Instantiate a new WeatherData, optionally basing it on the given data. */
  constructor(data?: Partial<WeatherData> | {[k: string]: any}) {
    Object.assign(this, WeatherData.map(data || {}));
  }
}


declare module "coalesce-vue/lib/model" {
  interface EnumTypeLookup {
    AuditEntryState: AuditEntryState
    Genders: Genders
    Statuses: Statuses
    Titles: Titles
  }
  interface ModelTypeLookup {
    AbstractClass: AbstractClass
    AbstractClassImpl: AbstractClassImpl
    AbstractClassPerson: AbstractClassPerson
    AuditLog: AuditLog
    AuditLogProperty: AuditLogProperty
    BaseClass: BaseClass
    BaseClassDerived: BaseClassDerived
    Case: Case
    CaseDto: CaseDto
    CaseDtoStandalone: CaseDtoStandalone
    CaseProduct: CaseProduct
    CaseStandalone: CaseStandalone
    CaseSummary: CaseSummary
    ChatResponse: ChatResponse
    Company: Company
    DateOnlyPk: DateOnlyPk
    DevTeam: DevTeam
    Location: Location
    Log: Log
    Person: Person
    PersonCriteria: PersonCriteria
    PersonLocation: PersonLocation
    PersonStats: PersonStats
    Product: Product
    ProductDetails: ProductDetails
    StandaloneReadCreate: StandaloneReadCreate
    StandaloneReadonly: StandaloneReadonly
    StandaloneReadWrite: StandaloneReadWrite
    StreetAddress: StreetAddress
    WeatherData: WeatherData
    ZipCode: ZipCode
  }
}
