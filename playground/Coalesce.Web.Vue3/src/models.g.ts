import * as metadata from './metadata.g'
import { Model, DataSource, convertToModel, mapToModel } from 'coalesce-vue/lib/model'

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


export enum SkyConditions {
  Cloudy = 0,
  PartyCloudy = 1,
  Sunny = 2,
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
  
  /** Mutates the input object and its descendents into a valid AuditLog implementation. */
  static convert(data?: Partial<AuditLog>): AuditLog {
    return convertToModel(data || {}, metadata.AuditLog) 
  }
  
  /** Maps the input object and its descendents to a new, valid AuditLog implementation. */
  static map(data?: Partial<AuditLog>): AuditLog {
    return mapToModel(data || {}, metadata.AuditLog) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid AuditLogProperty implementation. */
  static convert(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return convertToModel(data || {}, metadata.AuditLogProperty) 
  }
  
  /** Maps the input object and its descendents to a new, valid AuditLogProperty implementation. */
  static map(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return mapToModel(data || {}, metadata.AuditLogProperty) 
  }
  
  /** Instantiate a new AuditLogProperty, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLogProperty> | {[k: string]: any}) {
    Object.assign(this, AuditLogProperty.map(data || {}));
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
  caseProducts: CaseProduct[] | null
  devTeamAssignedId: number | null
  devTeamAssigned: DevTeam | null
  duration: unknown | null
}
export class Case {
  
  /** Mutates the input object and its descendents into a valid Case implementation. */
  static convert(data?: Partial<Case>): Case {
    return convertToModel(data || {}, metadata.Case) 
  }
  
  /** Maps the input object and its descendents to a new, valid Case implementation. */
  static map(data?: Partial<Case>): Case {
    return mapToModel(data || {}, metadata.Case) 
  }
  
  /** Instantiate a new Case, optionally basing it on the given data. */
  constructor(data?: Partial<Case> | {[k: string]: any}) {
    Object.assign(this, Case.map(data || {}));
  }
}
export namespace Case {
  export namespace DataSources {
    
    export class AllOpenCases implements DataSource<typeof metadata.Case.dataSources.allOpenCases> {
      readonly $metadata = metadata.Case.dataSources.allOpenCases
      minDate: Date | null = null
      
      constructor(params?: Omit<Partial<AllOpenCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
      }
    }
  }
}


export interface CaseDto extends Model<typeof metadata.CaseDto> {
  caseId: number | null
  title: string | null
  assignedToName: string | null
}
export class CaseDto {
  
  /** Mutates the input object and its descendents into a valid CaseDto implementation. */
  static convert(data?: Partial<CaseDto>): CaseDto {
    return convertToModel(data || {}, metadata.CaseDto) 
  }
  
  /** Maps the input object and its descendents to a new, valid CaseDto implementation. */
  static map(data?: Partial<CaseDto>): CaseDto {
    return mapToModel(data || {}, metadata.CaseDto) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid CaseDtoStandalone implementation. */
  static convert(data?: Partial<CaseDtoStandalone>): CaseDtoStandalone {
    return convertToModel(data || {}, metadata.CaseDtoStandalone) 
  }
  
  /** Maps the input object and its descendents to a new, valid CaseDtoStandalone implementation. */
  static map(data?: Partial<CaseDtoStandalone>): CaseDtoStandalone {
    return mapToModel(data || {}, metadata.CaseDtoStandalone) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid CaseProduct implementation. */
  static convert(data?: Partial<CaseProduct>): CaseProduct {
    return convertToModel(data || {}, metadata.CaseProduct) 
  }
  
  /** Maps the input object and its descendents to a new, valid CaseProduct implementation. */
  static map(data?: Partial<CaseProduct>): CaseProduct {
    return mapToModel(data || {}, metadata.CaseProduct) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid Company implementation. */
  static convert(data?: Partial<Company>): Company {
    return convertToModel(data || {}, metadata.Company) 
  }
  
  /** Maps the input object and its descendents to a new, valid Company implementation. */
  static map(data?: Partial<Company>): Company {
    return mapToModel(data || {}, metadata.Company) 
  }
  
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


export interface Log extends Model<typeof metadata.Log> {
  logId: number | null
  level: string | null
  message: string | null
}
export class Log {
  
  /** Mutates the input object and its descendents into a valid Log implementation. */
  static convert(data?: Partial<Log>): Log {
    return convertToModel(data || {}, metadata.Log) 
  }
  
  /** Maps the input object and its descendents to a new, valid Log implementation. */
  static map(data?: Partial<Log>): Log {
    return mapToModel(data || {}, metadata.Log) 
  }
  
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
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null
  
  /** Company ID this person is employed by */
  companyId: number | null
  
  /** Company loaded from the Company ID */
  company: Company | null
  arbitraryCollectionOfStrings: string[] | null
}
export class Person {
  
  /** Mutates the input object and its descendents into a valid Person implementation. */
  static convert(data?: Partial<Person>): Person {
    return convertToModel(data || {}, metadata.Person) 
  }
  
  /** Maps the input object and its descendents to a new, valid Person implementation. */
  static map(data?: Partial<Person>): Person {
    return mapToModel(data || {}, metadata.Person) 
  }
  
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
    }
    
    export class WithoutCases implements DataSource<typeof metadata.Person.dataSources.withoutCases> {
      readonly $metadata = metadata.Person.dataSources.withoutCases
    }
  }
}


export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
  details: ProductDetails | null
  uniqueId: string | null
  unknown: unknown | null
}
export class Product {
  
  /** Mutates the input object and its descendents into a valid Product implementation. */
  static convert(data?: Partial<Product>): Product {
    return convertToModel(data || {}, metadata.Product) 
  }
  
  /** Maps the input object and its descendents to a new, valid Product implementation. */
  static map(data?: Partial<Product>): Product {
    return mapToModel(data || {}, metadata.Product) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid ZipCode implementation. */
  static convert(data?: Partial<ZipCode>): ZipCode {
    return convertToModel(data || {}, metadata.ZipCode) 
  }
  
  /** Maps the input object and its descendents to a new, valid ZipCode implementation. */
  static map(data?: Partial<ZipCode>): ZipCode {
    return mapToModel(data || {}, metadata.ZipCode) 
  }
  
  /** Instantiate a new ZipCode, optionally basing it on the given data. */
  constructor(data?: Partial<ZipCode> | {[k: string]: any}) {
    Object.assign(this, ZipCode.map(data || {}));
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
  
  /** Mutates the input object and its descendents into a valid CaseSummary implementation. */
  static convert(data?: Partial<CaseSummary>): CaseSummary {
    return convertToModel(data || {}, metadata.CaseSummary) 
  }
  
  /** Maps the input object and its descendents to a new, valid CaseSummary implementation. */
  static map(data?: Partial<CaseSummary>): CaseSummary {
    return mapToModel(data || {}, metadata.CaseSummary) 
  }
  
  /** Instantiate a new CaseSummary, optionally basing it on the given data. */
  constructor(data?: Partial<CaseSummary> | {[k: string]: any}) {
    Object.assign(this, CaseSummary.map(data || {}));
  }
}


export interface DevTeam extends Model<typeof metadata.DevTeam> {
  devTeamId: number | null
  name: string | null
}
export class DevTeam {
  
  /** Mutates the input object and its descendents into a valid DevTeam implementation. */
  static convert(data?: Partial<DevTeam>): DevTeam {
    return convertToModel(data || {}, metadata.DevTeam) 
  }
  
  /** Maps the input object and its descendents to a new, valid DevTeam implementation. */
  static map(data?: Partial<DevTeam>): DevTeam {
    return mapToModel(data || {}, metadata.DevTeam) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid Location implementation. */
  static convert(data?: Partial<Location>): Location {
    return convertToModel(data || {}, metadata.Location) 
  }
  
  /** Maps the input object and its descendents to a new, valid Location implementation. */
  static map(data?: Partial<Location>): Location {
    return mapToModel(data || {}, metadata.Location) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid PersonCriteria implementation. */
  static convert(data?: Partial<PersonCriteria>): PersonCriteria {
    return convertToModel(data || {}, metadata.PersonCriteria) 
  }
  
  /** Maps the input object and its descendents to a new, valid PersonCriteria implementation. */
  static map(data?: Partial<PersonCriteria>): PersonCriteria {
    return mapToModel(data || {}, metadata.PersonCriteria) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid PersonLocation implementation. */
  static convert(data?: Partial<PersonLocation>): PersonLocation {
    return convertToModel(data || {}, metadata.PersonLocation) 
  }
  
  /** Maps the input object and its descendents to a new, valid PersonLocation implementation. */
  static map(data?: Partial<PersonLocation>): PersonLocation {
    return mapToModel(data || {}, metadata.PersonLocation) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid PersonStats implementation. */
  static convert(data?: Partial<PersonStats>): PersonStats {
    return convertToModel(data || {}, metadata.PersonStats) 
  }
  
  /** Maps the input object and its descendents to a new, valid PersonStats implementation. */
  static map(data?: Partial<PersonStats>): PersonStats {
    return mapToModel(data || {}, metadata.PersonStats) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid ProductDetails implementation. */
  static convert(data?: Partial<ProductDetails>): ProductDetails {
    return convertToModel(data || {}, metadata.ProductDetails) 
  }
  
  /** Maps the input object and its descendents to a new, valid ProductDetails implementation. */
  static map(data?: Partial<ProductDetails>): ProductDetails {
    return mapToModel(data || {}, metadata.ProductDetails) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid StandaloneReadCreate implementation. */
  static convert(data?: Partial<StandaloneReadCreate>): StandaloneReadCreate {
    return convertToModel(data || {}, metadata.StandaloneReadCreate) 
  }
  
  /** Maps the input object and its descendents to a new, valid StandaloneReadCreate implementation. */
  static map(data?: Partial<StandaloneReadCreate>): StandaloneReadCreate {
    return mapToModel(data || {}, metadata.StandaloneReadCreate) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid StandaloneReadonly implementation. */
  static convert(data?: Partial<StandaloneReadonly>): StandaloneReadonly {
    return convertToModel(data || {}, metadata.StandaloneReadonly) 
  }
  
  /** Maps the input object and its descendents to a new, valid StandaloneReadonly implementation. */
  static map(data?: Partial<StandaloneReadonly>): StandaloneReadonly {
    return mapToModel(data || {}, metadata.StandaloneReadonly) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid StandaloneReadWrite implementation. */
  static convert(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return convertToModel(data || {}, metadata.StandaloneReadWrite) 
  }
  
  /** Maps the input object and its descendents to a new, valid StandaloneReadWrite implementation. */
  static map(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return mapToModel(data || {}, metadata.StandaloneReadWrite) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid StreetAddress implementation. */
  static convert(data?: Partial<StreetAddress>): StreetAddress {
    return convertToModel(data || {}, metadata.StreetAddress) 
  }
  
  /** Maps the input object and its descendents to a new, valid StreetAddress implementation. */
  static map(data?: Partial<StreetAddress>): StreetAddress {
    return mapToModel(data || {}, metadata.StreetAddress) 
  }
  
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
  
  /** Mutates the input object and its descendents into a valid WeatherData implementation. */
  static convert(data?: Partial<WeatherData>): WeatherData {
    return convertToModel(data || {}, metadata.WeatherData) 
  }
  
  /** Maps the input object and its descendents to a new, valid WeatherData implementation. */
  static map(data?: Partial<WeatherData>): WeatherData {
    return mapToModel(data || {}, metadata.WeatherData) 
  }
  
  /** Instantiate a new WeatherData, optionally basing it on the given data. */
  constructor(data?: Partial<WeatherData> | {[k: string]: any}) {
    Object.assign(this, WeatherData.map(data || {}));
  }
}


