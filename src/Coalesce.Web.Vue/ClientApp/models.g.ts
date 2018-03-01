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

export class Person { constructor () { return convertToModel({}, metadata.Person) } }
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

export class Case { constructor () { return convertToModel({}, metadata.Case) } }
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

export class Company { constructor () { return convertToModel({}, metadata.Company) } }
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

export class Product { constructor () { return convertToModel({}, metadata.Product) } }
export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
  details: ProductDetails | null
}

export class CaseProduct { constructor () { return convertToModel({}, metadata.CaseProduct) } }
export interface CaseProduct extends Model<typeof metadata.CaseProduct> {
  caseProductId: number | null
  caseId: number | null
  case: Case | null
  productId: number | null
  product: Product | null
}

export class CaseDto { constructor () { return convertToModel({}, metadata.CaseDto) } }
export interface CaseDto extends Model<typeof metadata.CaseDto> {
  caseId: number | null
  title: string | null
  assignedToName: string | null
}

export class PersonCriteria { constructor () { return convertToModel({}, metadata.PersonCriteria) } }
export interface PersonCriteria extends Model<typeof metadata.PersonCriteria> {
  name: string | null
  birthdayMonth: number | null
  emailDomain: string | null
}

export class PersonStats { constructor () { return convertToModel({}, metadata.PersonStats) } }
export interface PersonStats extends Model<typeof metadata.PersonStats> {
  height: number | null
  weight: number | null
  name: string | null
}

export class CaseSummary { constructor () { return convertToModel({}, metadata.CaseSummary) } }
export interface CaseSummary extends Model<typeof metadata.CaseSummary> {
  caseSummaryId: number | null
  openCases: number | null
  caseCount: number | null
  closeCases: number | null
  description: string | null
}

export class DevTeam { constructor () { return convertToModel({}, metadata.DevTeam) } }
export interface DevTeam extends Model<typeof metadata.DevTeam> {
  devTeamId: number | null
  name: string | null
}

export class ProductDetails { constructor () { return convertToModel({}, metadata.ProductDetails) } }
export interface ProductDetails extends Model<typeof metadata.ProductDetails> {
  manufacturingAddress: StreetAddress | null
  companyHqAddress: StreetAddress | null
}

export class StreetAddress { constructor () { return convertToModel({}, metadata.StreetAddress) } }
export interface StreetAddress extends Model<typeof metadata.StreetAddress> {
  address: string | null
  city: string | null
  state: string | null
  postalCode: string | null
}

export class WeatherData { constructor () { return convertToModel({}, metadata.WeatherData) } }
export interface WeatherData extends Model<typeof metadata.WeatherData> {
  tempFahrenheit: number | null
  humidity: number | null
  location: Location | null
}

export class Location { constructor () { return convertToModel({}, metadata.Location) } }
export interface Location extends Model<typeof metadata.Location> {
  city: string | null
  state: string | null
  zip: string | null
}

