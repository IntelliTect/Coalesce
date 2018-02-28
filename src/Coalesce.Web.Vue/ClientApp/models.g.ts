import * as metadata from './metadata.g'
import * as moment from 'moment'
import { Model } from './coalesce/core/model'

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

export interface Person extends Model<typeof metadata.Person> {
  personId: number | null
  title: Titles | null
  firstName: string | null
  lastName: string | null
  email: string | null
  gender: Genders | null
  casesAssigned: Case[] | null
  casesReported: Case[] | null
  birthDate: moment.Moment | null
  lastBath: moment.Moment | null
  nextUpgrade: moment.Moment | null
  personStats: PersonStats | null
  name: string | null
  companyId: number | null
  company: Company | null
}

export interface Case extends Model<typeof metadata.Case> {
  caseKey: number | null
  title: string | null
  description: string | null
  openedAt: moment.Moment | null
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

export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
}

export interface CaseProduct extends Model<typeof metadata.CaseProduct> {
  caseProductId: number | null
  caseId: number | null
  case: Case | null
  productId: number | null
  product: Product | null
}

export interface CaseDto extends Model<typeof metadata.CaseDto> {
  caseId: number | null
  title: string | null
  assignedToName: string | null
}

export interface PersonCriteria extends Model<typeof metadata.PersonCriteria> {
  name: string | null
  birthdayMonth: number | null
  emailDomain: string | null
}

export interface PersonStats extends Model<typeof metadata.PersonStats> {
  height: number | null
  weight: number | null
  name: string | null
}

export interface CaseSummary extends Model<typeof metadata.CaseSummary> {
  caseSummaryId: number | null
  openCases: number | null
  caseCount: number | null
  closeCases: number | null
  description: string | null
}

export interface DevTeam extends Model<typeof metadata.DevTeam> {
  devTeamId: number | null
  name: string | null
}

export interface WeatherData extends Model<typeof metadata.WeatherData> {
  tempFahrenheit: number | null
  humidity: number | null
  location: Location | null
}

export interface Location extends Model<typeof metadata.Location> {
  city: string | null
  state: string | null
  zip: string | null
}

