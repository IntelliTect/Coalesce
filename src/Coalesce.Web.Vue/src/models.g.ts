import * as metadata from './metadata.g'
import { Model, DataSource, convertToModel, mapToModel } from 'coalesce-vue/lib/model'

export enum Genders {
  NonSpecified = 0,
  Male = 1,
  Female = 2,
}


export enum Statuses {
  Open = 0,
  InProgress = 1,
  Resolved = 2,
  ClosedNoSolution = 3,
  Cancelled = 4,
}


export enum Titles {
  Mr = 0,
  Ms = 1,
  Mrs = 2,
  Miss = 4,
}


export interface Case extends Model<typeof metadata.Case> {
  
  /** The Primary key for the Case object */
  caseKey: number | null
  title: string | null
  description: string | null
  openedAt: Date | null
  assignedToId: number | null
  assignedTo: Person | null
  reportedById: number | null
  reportedBy: Person | null
  imageName: string | null
  imageSize: number | null
  imageHash: string | null
  attachmentName: string | null
  severity: string | null
  status: Statuses | null
  caseProducts: CaseProduct[] | null
  devTeamAssignedId: number | null
  devTeamAssigned: DevTeam | null
  duration: any | null
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
  companyId: number | null
  name: string | null
  address1: string | null
  address2: string | null
  city: string | null
  state: string | null
  zipCode: string | null
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


export interface CaseSummary extends Model<typeof metadata.CaseSummary> {
  caseSummaryId: number | null
  openCases: number | null
  caseCount: number | null
  closeCases: number | null
  description: string | null
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


export interface PersonStats extends Model<typeof metadata.PersonStats> {
  height: number | null
  weight: number | null
  name: string | null
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


