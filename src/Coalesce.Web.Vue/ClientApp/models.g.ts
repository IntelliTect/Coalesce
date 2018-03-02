import * as metadata from './metadata.g'
import { Model, convertToModel } from './coalesce/core/model'

export enum Titles {
  Mr = 0,
  Ms = 1,
  Mrs = 2,
  Miss = 4,
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
  ClosedNoSolution = 3,
  Cancelled = 4,
}

export namespace Person {
  /** Mutates the input object and its descendents into a valid Person implementation. */
  export function from(data?: Partial<Person>): Person { return convertToModel(data || {}, metadata.Person) }
}
export interface Person extends Model<typeof metadata.Person> {
  personId: number | null
  title: Titles | null
  firstName: string | null
  lastName: string | null
  email: string | null
  gender: Genders | null
  casesAssigned: Case[] | null
  casesReported: Case[] | null
  birthDate: Date | null
  lastBath: Date | null
  nextUpgrade: Date | null
  personStats: PersonStats | null
  name: string | null
  companyId: number | null
  company: Company | null
}

export namespace Case {
  /** Mutates the input object and its descendents into a valid Case implementation. */
  export function from(data?: Partial<Case>): Case { return convertToModel(data || {}, metadata.Case) }
}
export interface Case extends Model<typeof metadata.Case> {
  caseKey: number | null
  title: string | null
  description: string | null
  openedAt: Date | null
  assignedToId: number | null
  assignedTo: Person | null
  reportedById: number | null
  reportedBy: Person | null
  attachment: string | null
  severity: string | null
  status: Statuses | null
  caseProducts: CaseProduct[] | null
  devTeamAssignedId: number | null
  devTeamAssigned: DevTeam | null
  duration: any | null
}

export namespace Company {
  /** Mutates the input object and its descendents into a valid Company implementation. */
  export function from(data?: Partial<Company>): Company { return convertToModel(data || {}, metadata.Company) }
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

export namespace Product {
  /** Mutates the input object and its descendents into a valid Product implementation. */
  export function from(data?: Partial<Product>): Product { return convertToModel(data || {}, metadata.Product) }
}
export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
  details: ProductDetails | null
}

export namespace CaseProduct {
  /** Mutates the input object and its descendents into a valid CaseProduct implementation. */
  export function from(data?: Partial<CaseProduct>): CaseProduct { return convertToModel(data || {}, metadata.CaseProduct) }
}
export interface CaseProduct extends Model<typeof metadata.CaseProduct> {
  caseProductId: number | null
  caseId: number | null
  case: Case | null
  productId: number | null
  product: Product | null
}

export namespace CaseDto {
  /** Mutates the input object and its descendents into a valid CaseDto implementation. */
  export function from(data?: Partial<CaseDto>): CaseDto { return convertToModel(data || {}, metadata.CaseDto) }
}
export interface CaseDto extends Model<typeof metadata.CaseDto> {
  caseId: number | null
  title: string | null
  assignedToName: string | null
}

export namespace PersonCriteria {
  /** Mutates the input object and its descendents into a valid PersonCriteria implementation. */
  export function from(data?: Partial<PersonCriteria>): PersonCriteria { return convertToModel(data || {}, metadata.PersonCriteria) }
}
export interface PersonCriteria extends Model<typeof metadata.PersonCriteria> {
  name: string | null
  birthdayMonth: number | null
  emailDomain: string | null
}

export namespace PersonStats {
  /** Mutates the input object and its descendents into a valid PersonStats implementation. */
  export function from(data?: Partial<PersonStats>): PersonStats { return convertToModel(data || {}, metadata.PersonStats) }
}
export interface PersonStats extends Model<typeof metadata.PersonStats> {
  height: number | null
  weight: number | null
  name: string | null
}

export namespace CaseSummary {
  /** Mutates the input object and its descendents into a valid CaseSummary implementation. */
  export function from(data?: Partial<CaseSummary>): CaseSummary { return convertToModel(data || {}, metadata.CaseSummary) }
}
export interface CaseSummary extends Model<typeof metadata.CaseSummary> {
  caseSummaryId: number | null
  openCases: number | null
  caseCount: number | null
  closeCases: number | null
  description: string | null
}

export namespace DevTeam {
  /** Mutates the input object and its descendents into a valid DevTeam implementation. */
  export function from(data?: Partial<DevTeam>): DevTeam { return convertToModel(data || {}, metadata.DevTeam) }
}
export interface DevTeam extends Model<typeof metadata.DevTeam> {
  devTeamId: number | null
  name: string | null
}

export namespace ProductDetails {
  /** Mutates the input object and its descendents into a valid ProductDetails implementation. */
  export function from(data?: Partial<ProductDetails>): ProductDetails { return convertToModel(data || {}, metadata.ProductDetails) }
}
export interface ProductDetails extends Model<typeof metadata.ProductDetails> {
  manufacturingAddress: StreetAddress | null
  companyHqAddress: StreetAddress | null
}

export namespace StreetAddress {
  /** Mutates the input object and its descendents into a valid StreetAddress implementation. */
  export function from(data?: Partial<StreetAddress>): StreetAddress { return convertToModel(data || {}, metadata.StreetAddress) }
}
export interface StreetAddress extends Model<typeof metadata.StreetAddress> {
  address: string | null
  city: string | null
  state: string | null
  postalCode: string | null
}

export namespace WeatherData {
  /** Mutates the input object and its descendents into a valid WeatherData implementation. */
  export function from(data?: Partial<WeatherData>): WeatherData { return convertToModel(data || {}, metadata.WeatherData) }
}
export interface WeatherData extends Model<typeof metadata.WeatherData> {
  tempFahrenheit: number | null
  humidity: number | null
  location: Location | null
}

export namespace Location {
  /** Mutates the input object and its descendents into a valid Location implementation. */
  export function from(data?: Partial<Location>): Location { return convertToModel(data || {}, metadata.Location) }
}
export interface Location extends Model<typeof metadata.Location> {
  city: string | null
  state: string | null
  zip: string | null
}

