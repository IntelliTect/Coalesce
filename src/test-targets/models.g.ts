import * as metadata from './metadata.g'
import { convertToModel, mapToModel, reactiveDataSource } from 'coalesce-vue/lib/model'
import type { Model, DataSource } from 'coalesce-vue/lib/model'

export enum EnumPkId {
  Value0 = 0,
  Value1 = 1,
  Value10 = 10,
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
  
  /** Closed with a solution. */
  Resolved = 2,
  
  /** Closed without any resolution. */
  ClosedNoSolution = 3,
  
  /** doc comment on enum member */
  Cancelled = 99,
}


export enum Titles {
  Mr = 0,
  Ms = 1,
  Mrs = 2,
  Miss = 4,
}


export interface AbstractImpl extends Model<typeof metadata.AbstractImpl> {
  implOnlyField: string | null
  id: number | null
  discriminatior: string | null
}
export class AbstractImpl {
  
  /** Mutates the input object and its descendents into a valid AbstractImpl implementation. */
  static convert(data?: Partial<AbstractImpl>): AbstractImpl {
    return convertToModel(data || {}, metadata.AbstractImpl) 
  }
  
  /** Maps the input object and its descendents to a new, valid AbstractImpl implementation. */
  static map(data?: Partial<AbstractImpl>): AbstractImpl {
    return mapToModel(data || {}, metadata.AbstractImpl) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AbstractImpl; }
  
  /** Instantiate a new AbstractImpl, optionally basing it on the given data. */
  constructor(data?: Partial<AbstractImpl> | {[k: string]: any}) {
    Object.assign(this, AbstractImpl.map(data || {}));
  }
}


export interface AbstractModel extends Model<typeof metadata.AbstractModel> {
  id: number | null
  discriminatior: string | null
}
export class AbstractModel {
  
  /** Mutates the input object and its descendents into a valid AbstractModel implementation. */
  static convert(data?: Partial<AbstractModel>): AbstractModel {
    return convertToModel(data || {}, metadata.AbstractModel) 
  }
  
  /** Maps the input object and its descendents to a new, valid AbstractModel implementation. */
  static map(data?: Partial<AbstractModel>): AbstractModel {
    return mapToModel(data || {}, metadata.AbstractModel) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AbstractModel; }
  
  /** Instantiate a new AbstractModel, optionally basing it on the given data. */
  constructor(data?: Partial<AbstractModel> | {[k: string]: any}) {
    Object.assign(this, AbstractModel.map(data || {}));
  }
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
  status: Statuses | null
  caseProducts: CaseProduct[] | null
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
      minDate: Date | null = null
      
      constructor(params?: Omit<Partial<AllOpenCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
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
  
  /** Mutates the input object and its descendents into a valid CaseProduct implementation. */
  static convert(data?: Partial<CaseProduct>): CaseProduct {
    return convertToModel(data || {}, metadata.CaseProduct) 
  }
  
  /** Maps the input object and its descendents to a new, valid CaseProduct implementation. */
  static map(data?: Partial<CaseProduct>): CaseProduct {
    return mapToModel(data || {}, metadata.CaseProduct) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.CaseProduct; }
  
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
  phone: string | null
  websiteUrl: string | null
  logoUrl: string | null
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Company; }
  
  /** Instantiate a new Company, optionally basing it on the given data. */
  constructor(data?: Partial<Company> | {[k: string]: any}) {
    Object.assign(this, Company.map(data || {}));
  }
}


export interface ComplexModel extends Model<typeof metadata.ComplexModel> {
  complexModelId: number | null
  tests: Test[] | null
  
  /** 
    Test case for foreign keys without a reference navigation prop.
    This configuration *will* be picked up by EF conventions.
  */
  childrenWithoutRefNavProp: ComplexModelDependent[] | null
  unmappedCollectionOfMappedModels: Test[] | null
  singleTestId: number | null
  
  /** The active Test record for the model. */
  singleTest: Test | null
  enumPkId: EnumPkId | null
  enumPk: EnumPk | null
  dateTimeOffset: Date | null
  dateTimeOffsetNullable: Date | null
  dateTime: Date | null
  dateTimeNullable: Date | null
  systemDateOnly: Date | null
  systemTimeOnly: Date | null
  dateOnlyViaAttribute: Date | null
  unmappedSettableString: string | null
  adminReadableString: string | null
  
  /** 
    This is a multiline string in an attribute.
    This is a second line in the string.
  */
  restrictedString: string | null
  
  /** 
    This is a multiline string
     via explicit escaped newline
  */
  restrictInit: string | null
  adminReadableReferenceNavigationId: number | null
  adminReadableReferenceNavigation: ComplexModel | null
  referenceNavigationId: number | null
  referenceNavigation: ComplexModel | null
  noAutoIncludeReferenceNavigationId: number | null
  noAutoIncludeReferenceNavigation: ComplexModel | null
  noAutoIncludeByClassReferenceNavigationId: number | null
  noAutoIncludeByClassReferenceNavigation: Company | null
  name: string | null
  isActive: boolean | null
  byteArrayProp: string | null
  string: string | null
  stringWithDefault: string | null
  intWithDefault: number | null
  doubleWithDefault: number | null
  enumWithDefault: EnumPkId | null
  color: string | null
  stringSearchedEqualsInsensitive: string | null
  stringSearchedEqualsNatural: string | null
  int: number | null
  intNullable: number | null
  decimalNullable: number | null
  long: number | null
  guid: string | null
  guidNullable: string | null
  intCollection: number[] | null
  enumCollection: Statuses[] | null
  nonNullNonZeroInt: number | null
  clientValidationInt: number | null
  clientValidationString: string | null
  enumNullable: Statuses | null
  readOnlyPrimitiveCollection: string[] | null
  mutablePrimitiveCollection: string[] | null
  primitiveEnumerable: string[] | null
}
export class ComplexModel {
  
  /** Mutates the input object and its descendents into a valid ComplexModel implementation. */
  static convert(data?: Partial<ComplexModel>): ComplexModel {
    return convertToModel(data || {}, metadata.ComplexModel) 
  }
  
  /** Maps the input object and its descendents to a new, valid ComplexModel implementation. */
  static map(data?: Partial<ComplexModel>): ComplexModel {
    return mapToModel(data || {}, metadata.ComplexModel) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ComplexModel; }
  
  /** Instantiate a new ComplexModel, optionally basing it on the given data. */
  constructor(data?: Partial<ComplexModel> | {[k: string]: any}) {
    Object.assign(this, ComplexModel.map(data || {}));
  }
}


export interface ComplexModelDependent extends Model<typeof metadata.ComplexModelDependent> {
  id: number | null
  parentId: number | null
  name: string | null
}
export class ComplexModelDependent {
  
  /** Mutates the input object and its descendents into a valid ComplexModelDependent implementation. */
  static convert(data?: Partial<ComplexModelDependent>): ComplexModelDependent {
    return convertToModel(data || {}, metadata.ComplexModelDependent) 
  }
  
  /** Maps the input object and its descendents to a new, valid ComplexModelDependent implementation. */
  static map(data?: Partial<ComplexModelDependent>): ComplexModelDependent {
    return mapToModel(data || {}, metadata.ComplexModelDependent) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ComplexModelDependent; }
  
  /** Instantiate a new ComplexModelDependent, optionally basing it on the given data. */
  constructor(data?: Partial<ComplexModelDependent> | {[k: string]: any}) {
    Object.assign(this, ComplexModelDependent.map(data || {}));
  }
}


export interface EnumPk extends Model<typeof metadata.EnumPk> {
  enumPkId: EnumPkId | null
  name: string | null
}
export class EnumPk {
  
  /** Mutates the input object and its descendents into a valid EnumPk implementation. */
  static convert(data?: Partial<EnumPk>): EnumPk {
    return convertToModel(data || {}, metadata.EnumPk) 
  }
  
  /** Maps the input object and its descendents to a new, valid EnumPk implementation. */
  static map(data?: Partial<EnumPk>): EnumPk {
    return mapToModel(data || {}, metadata.EnumPk) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.EnumPk; }
  
  /** Instantiate a new EnumPk, optionally basing it on the given data. */
  constructor(data?: Partial<EnumPk> | {[k: string]: any}) {
    Object.assign(this, EnumPk.map(data || {}));
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
  secretPhrase: string | null
  
  /** Genetic Gender of the person. */
  gender: Genders | null
  
  /** List of cases assigned to the person */
  casesAssigned: Case[] | null
  
  /** List of cases reported by the person. */
  casesReported: Case[] | null
  birthDate: Date | null
  
  /** 
    Calculated name of the person. eg., Mr. Michael Stokesbary.
    A concatenation of Title, FirstName, and LastName.
  */
  name: string | null
  
  /** Company ID this person is employed by */
  companyId: number | null
  
  /** Company loaded from the Company ID */
  company: Company | null
  siblingRelationships: Sibling[] | null
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Person; }
  
  /** Instantiate a new Person, optionally basing it on the given data. */
  constructor(data?: Partial<Person> | {[k: string]: any}) {
    Object.assign(this, Person.map(data || {}));
  }
}
export namespace Person {
  export namespace DataSources {
    
    /** People whose last name starts with B or c */
    export class BorCPeople implements DataSource<typeof metadata.Person.dataSources.borCPeople> {
      readonly $metadata = metadata.Person.dataSources.borCPeople
    }
    
    export class NamesStartingWithAWithCases implements DataSource<typeof metadata.Person.dataSources.namesStartingWithAWithCases> {
      readonly $metadata = metadata.Person.dataSources.namesStartingWithAWithCases
      allowedStatuses: Statuses[] | null = null
      hasEmail: boolean | null = null
      
      constructor(params?: Omit<Partial<NamesStartingWithAWithCases>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
    }
    
    export class ParameterTestsSource implements DataSource<typeof metadata.Person.dataSources.parameterTestsSource> {
      readonly $metadata = metadata.Person.dataSources.parameterTestsSource
      personCriterion: PersonCriteria | null = null
      personCriteriaArray: PersonCriteria[] | null = null
      personCriteriaList: PersonCriteria[] | null = null
      personCriteriaICollection: PersonCriteria[] | null = null
      intArray: number[] | null = null
      intList: number[] | null = null
      intICollection: number[] | null = null
      bytes: string | null = null
      
      constructor(params?: Omit<Partial<ParameterTestsSource>, '$metadata'>) {
        if (params) Object.assign(this, params);
        return reactiveDataSource(this);
      }
    }
    
    export class WithoutCases implements DataSource<typeof metadata.Person.dataSources.withoutCases> {
      readonly $metadata = metadata.Person.dataSources.withoutCases
    }
  }
}


export interface Product extends Model<typeof metadata.Product> {
  productId: number | null
  name: string | null
  uniqueId1: string | null
  uniqueId2: string | null
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Product; }
  
  /** Instantiate a new Product, optionally basing it on the given data. */
  constructor(data?: Partial<Product> | {[k: string]: any}) {
    Object.assign(this, Product.map(data || {}));
  }
}


export interface ReadOnlyEntityUsedAsMethodInput extends Model<typeof metadata.ReadOnlyEntityUsedAsMethodInput> {
  id: number | null
  name: string | null
}
export class ReadOnlyEntityUsedAsMethodInput {
  
  /** Mutates the input object and its descendents into a valid ReadOnlyEntityUsedAsMethodInput implementation. */
  static convert(data?: Partial<ReadOnlyEntityUsedAsMethodInput>): ReadOnlyEntityUsedAsMethodInput {
    return convertToModel(data || {}, metadata.ReadOnlyEntityUsedAsMethodInput) 
  }
  
  /** Maps the input object and its descendents to a new, valid ReadOnlyEntityUsedAsMethodInput implementation. */
  static map(data?: Partial<ReadOnlyEntityUsedAsMethodInput>): ReadOnlyEntityUsedAsMethodInput {
    return mapToModel(data || {}, metadata.ReadOnlyEntityUsedAsMethodInput) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ReadOnlyEntityUsedAsMethodInput; }
  
  /** Instantiate a new ReadOnlyEntityUsedAsMethodInput, optionally basing it on the given data. */
  constructor(data?: Partial<ReadOnlyEntityUsedAsMethodInput> | {[k: string]: any}) {
    Object.assign(this, ReadOnlyEntityUsedAsMethodInput.map(data || {}));
  }
}


export interface RecursiveHierarchy extends Model<typeof metadata.RecursiveHierarchy> {
  id: number | null
  name: string | null
  parentId: number | null
  parent: RecursiveHierarchy | null
  children: RecursiveHierarchy[] | null
}
export class RecursiveHierarchy {
  
  /** Mutates the input object and its descendents into a valid RecursiveHierarchy implementation. */
  static convert(data?: Partial<RecursiveHierarchy>): RecursiveHierarchy {
    return convertToModel(data || {}, metadata.RecursiveHierarchy) 
  }
  
  /** Maps the input object and its descendents to a new, valid RecursiveHierarchy implementation. */
  static map(data?: Partial<RecursiveHierarchy>): RecursiveHierarchy {
    return mapToModel(data || {}, metadata.RecursiveHierarchy) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.RecursiveHierarchy; }
  
  /** Instantiate a new RecursiveHierarchy, optionally basing it on the given data. */
  constructor(data?: Partial<RecursiveHierarchy> | {[k: string]: any}) {
    Object.assign(this, RecursiveHierarchy.map(data || {}));
  }
}


export interface RequiredAndInitModel extends Model<typeof metadata.RequiredAndInitModel> {
  id: number | null
  requiredRef: string | null
  requiredValue: number | null
  requiredInitRef: string | null
  requiredInitValue: number | null
  initRef: string | null
  initValue: number | null
  nullableInitValue: number | null
}
export class RequiredAndInitModel {
  
  /** Mutates the input object and its descendents into a valid RequiredAndInitModel implementation. */
  static convert(data?: Partial<RequiredAndInitModel>): RequiredAndInitModel {
    return convertToModel(data || {}, metadata.RequiredAndInitModel) 
  }
  
  /** Maps the input object and its descendents to a new, valid RequiredAndInitModel implementation. */
  static map(data?: Partial<RequiredAndInitModel>): RequiredAndInitModel {
    return mapToModel(data || {}, metadata.RequiredAndInitModel) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.RequiredAndInitModel; }
  
  /** Instantiate a new RequiredAndInitModel, optionally basing it on the given data. */
  constructor(data?: Partial<RequiredAndInitModel> | {[k: string]: any}) {
    Object.assign(this, RequiredAndInitModel.map(data || {}));
  }
}


export interface Sibling extends Model<typeof metadata.Sibling> {
  siblingId: number | null
  personId: number | null
  person: Person | null
  personTwoId: number | null
  personTwo: Person | null
}
export class Sibling {
  
  /** Mutates the input object and its descendents into a valid Sibling implementation. */
  static convert(data?: Partial<Sibling>): Sibling {
    return convertToModel(data || {}, metadata.Sibling) 
  }
  
  /** Maps the input object and its descendents to a new, valid Sibling implementation. */
  static map(data?: Partial<Sibling>): Sibling {
    return mapToModel(data || {}, metadata.Sibling) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Sibling; }
  
  /** Instantiate a new Sibling, optionally basing it on the given data. */
  constructor(data?: Partial<Sibling> | {[k: string]: any}) {
    Object.assign(this, Sibling.map(data || {}));
  }
}


export interface StringIdentity extends Model<typeof metadata.StringIdentity> {
  stringIdentityId: string | null
  parentId: string | null
  parent: StringIdentity | null
  parentReqId: string | null
  parentReq: StringIdentity | null
  children: StringIdentity[] | null
}
export class StringIdentity {
  
  /** Mutates the input object and its descendents into a valid StringIdentity implementation. */
  static convert(data?: Partial<StringIdentity>): StringIdentity {
    return convertToModel(data || {}, metadata.StringIdentity) 
  }
  
  /** Maps the input object and its descendents to a new, valid StringIdentity implementation. */
  static map(data?: Partial<StringIdentity>): StringIdentity {
    return mapToModel(data || {}, metadata.StringIdentity) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.StringIdentity; }
  
  /** Instantiate a new StringIdentity, optionally basing it on the given data. */
  constructor(data?: Partial<StringIdentity> | {[k: string]: any}) {
    Object.assign(this, StringIdentity.map(data || {}));
  }
}


export interface Test extends Model<typeof metadata.Test> {
  testId: number | null
  complexModelId: number | null
  complexModel: ComplexModel | null
  testName: string | null
}
export class Test {
  
  /** Mutates the input object and its descendents into a valid Test implementation. */
  static convert(data?: Partial<Test>): Test {
    return convertToModel(data || {}, metadata.Test) 
  }
  
  /** Maps the input object and its descendents to a new, valid Test implementation. */
  static map(data?: Partial<Test>): Test {
    return mapToModel(data || {}, metadata.Test) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Test; }
  
  /** Instantiate a new Test, optionally basing it on the given data. */
  constructor(data?: Partial<Test> | {[k: string]: any}) {
    Object.assign(this, Test.map(data || {}));
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ZipCode; }
  
  /** Instantiate a new ZipCode, optionally basing it on the given data. */
  constructor(data?: Partial<ZipCode> | {[k: string]: any}) {
    Object.assign(this, ZipCode.map(data || {}));
  }
}


export interface ExternalChild extends Model<typeof metadata.ExternalChild> {
  value: string | null
}
export class ExternalChild {
  
  /** Mutates the input object and its descendents into a valid ExternalChild implementation. */
  static convert(data?: Partial<ExternalChild>): ExternalChild {
    return convertToModel(data || {}, metadata.ExternalChild) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalChild implementation. */
  static map(data?: Partial<ExternalChild>): ExternalChild {
    return mapToModel(data || {}, metadata.ExternalChild) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalChild; }
  
  /** Instantiate a new ExternalChild, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalChild> | {[k: string]: any}) {
    Object.assign(this, ExternalChild.map(data || {}));
  }
}


export interface ExternalChildAsInputOnly extends Model<typeof metadata.ExternalChildAsInputOnly> {
  value: string | null
  recursive: ExternalParentAsInputOnly | null
}
export class ExternalChildAsInputOnly {
  
  /** Mutates the input object and its descendents into a valid ExternalChildAsInputOnly implementation. */
  static convert(data?: Partial<ExternalChildAsInputOnly>): ExternalChildAsInputOnly {
    return convertToModel(data || {}, metadata.ExternalChildAsInputOnly) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalChildAsInputOnly implementation. */
  static map(data?: Partial<ExternalChildAsInputOnly>): ExternalChildAsInputOnly {
    return mapToModel(data || {}, metadata.ExternalChildAsInputOnly) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalChildAsInputOnly; }
  
  /** Instantiate a new ExternalChildAsInputOnly, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalChildAsInputOnly> | {[k: string]: any}) {
    Object.assign(this, ExternalChildAsInputOnly.map(data || {}));
  }
}


export interface ExternalChildAsOutputOnly extends Model<typeof metadata.ExternalChildAsOutputOnly> {
  value: string | null
  recursive: ExternalParentAsOutputOnly | null
}
export class ExternalChildAsOutputOnly {
  
  /** Mutates the input object and its descendents into a valid ExternalChildAsOutputOnly implementation. */
  static convert(data?: Partial<ExternalChildAsOutputOnly>): ExternalChildAsOutputOnly {
    return convertToModel(data || {}, metadata.ExternalChildAsOutputOnly) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalChildAsOutputOnly implementation. */
  static map(data?: Partial<ExternalChildAsOutputOnly>): ExternalChildAsOutputOnly {
    return mapToModel(data || {}, metadata.ExternalChildAsOutputOnly) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalChildAsOutputOnly; }
  
  /** Instantiate a new ExternalChildAsOutputOnly, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalChildAsOutputOnly> | {[k: string]: any}) {
    Object.assign(this, ExternalChildAsOutputOnly.map(data || {}));
  }
}


export interface ExternalParent extends Model<typeof metadata.ExternalParent> {
  valueArray: number[] | null
  valueNullableArray: number[] | null
  valueArrayNullable: number[] | null
  valueICollection: number[] | null
  valueNullableICollection: number[] | null
  valueICollectionNullable: number[] | null
  valueList: number[] | null
  stringICollection: string[] | null
  stringList: string[] | null
  refArray: ExternalChild[] | null
  refNullableArray: ExternalChild[] | null
  refArrayNullable: ExternalChild[] | null
  refICollection: ExternalChild[] | null
  refNullableICollection: ExternalChild[] | null
  refICollectionNullable: ExternalChild[] | null
  refList: ExternalChild[] | null
  refNullableList: ExternalChild[] | null
  refListNullable: ExternalChild[] | null
}
export class ExternalParent {
  
  /** Mutates the input object and its descendents into a valid ExternalParent implementation. */
  static convert(data?: Partial<ExternalParent>): ExternalParent {
    return convertToModel(data || {}, metadata.ExternalParent) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalParent implementation. */
  static map(data?: Partial<ExternalParent>): ExternalParent {
    return mapToModel(data || {}, metadata.ExternalParent) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalParent; }
  
  /** Instantiate a new ExternalParent, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalParent> | {[k: string]: any}) {
    Object.assign(this, ExternalParent.map(data || {}));
  }
}


export interface ExternalParentAsInputOnly extends Model<typeof metadata.ExternalParentAsInputOnly> {
  child: ExternalChildAsInputOnly | null
}
export class ExternalParentAsInputOnly {
  
  /** Mutates the input object and its descendents into a valid ExternalParentAsInputOnly implementation. */
  static convert(data?: Partial<ExternalParentAsInputOnly>): ExternalParentAsInputOnly {
    return convertToModel(data || {}, metadata.ExternalParentAsInputOnly) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalParentAsInputOnly implementation. */
  static map(data?: Partial<ExternalParentAsInputOnly>): ExternalParentAsInputOnly {
    return mapToModel(data || {}, metadata.ExternalParentAsInputOnly) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalParentAsInputOnly; }
  
  /** Instantiate a new ExternalParentAsInputOnly, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalParentAsInputOnly> | {[k: string]: any}) {
    Object.assign(this, ExternalParentAsInputOnly.map(data || {}));
  }
}


export interface ExternalParentAsOutputOnly extends Model<typeof metadata.ExternalParentAsOutputOnly> {
  child: ExternalChildAsOutputOnly | null
}
export class ExternalParentAsOutputOnly {
  
  /** Mutates the input object and its descendents into a valid ExternalParentAsOutputOnly implementation. */
  static convert(data?: Partial<ExternalParentAsOutputOnly>): ExternalParentAsOutputOnly {
    return convertToModel(data || {}, metadata.ExternalParentAsOutputOnly) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalParentAsOutputOnly implementation. */
  static map(data?: Partial<ExternalParentAsOutputOnly>): ExternalParentAsOutputOnly {
    return mapToModel(data || {}, metadata.ExternalParentAsOutputOnly) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalParentAsOutputOnly; }
  
  /** Instantiate a new ExternalParentAsOutputOnly, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalParentAsOutputOnly> | {[k: string]: any}) {
    Object.assign(this, ExternalParentAsOutputOnly.map(data || {}));
  }
}


export interface ExternalTypeWithDtoProp extends Model<typeof metadata.ExternalTypeWithDtoProp> {
  case: CaseDtoStandalone | null
  cases: CaseDtoStandalone[] | null
  casesList: CaseDtoStandalone[] | null
  casesArray: CaseDtoStandalone[] | null
}
export class ExternalTypeWithDtoProp {
  
  /** Mutates the input object and its descendents into a valid ExternalTypeWithDtoProp implementation. */
  static convert(data?: Partial<ExternalTypeWithDtoProp>): ExternalTypeWithDtoProp {
    return convertToModel(data || {}, metadata.ExternalTypeWithDtoProp) 
  }
  
  /** Maps the input object and its descendents to a new, valid ExternalTypeWithDtoProp implementation. */
  static map(data?: Partial<ExternalTypeWithDtoProp>): ExternalTypeWithDtoProp {
    return mapToModel(data || {}, metadata.ExternalTypeWithDtoProp) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ExternalTypeWithDtoProp; }
  
  /** Instantiate a new ExternalTypeWithDtoProp, optionally basing it on the given data. */
  constructor(data?: Partial<ExternalTypeWithDtoProp> | {[k: string]: any}) {
    Object.assign(this, ExternalTypeWithDtoProp.map(data || {}));
  }
}


export interface InitRecordWithDefaultCtor extends Model<typeof metadata.InitRecordWithDefaultCtor> {
  string: string | null
  num: number | null
  nestedRecord: PositionalRecord | null
}
export class InitRecordWithDefaultCtor {
  
  /** Mutates the input object and its descendents into a valid InitRecordWithDefaultCtor implementation. */
  static convert(data?: Partial<InitRecordWithDefaultCtor>): InitRecordWithDefaultCtor {
    return convertToModel(data || {}, metadata.InitRecordWithDefaultCtor) 
  }
  
  /** Maps the input object and its descendents to a new, valid InitRecordWithDefaultCtor implementation. */
  static map(data?: Partial<InitRecordWithDefaultCtor>): InitRecordWithDefaultCtor {
    return mapToModel(data || {}, metadata.InitRecordWithDefaultCtor) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.InitRecordWithDefaultCtor; }
  
  /** Instantiate a new InitRecordWithDefaultCtor, optionally basing it on the given data. */
  constructor(data?: Partial<InitRecordWithDefaultCtor> | {[k: string]: any}) {
    Object.assign(this, InitRecordWithDefaultCtor.map(data || {}));
  }
}


export interface InputOutputOnlyExternalTypeWithRequiredNonscalarProp extends Model<typeof metadata.InputOutputOnlyExternalTypeWithRequiredNonscalarProp> {
  id: number | null
  externalChild: ExternalChild | null
}
export class InputOutputOnlyExternalTypeWithRequiredNonscalarProp {
  
  /** Mutates the input object and its descendents into a valid InputOutputOnlyExternalTypeWithRequiredNonscalarProp implementation. */
  static convert(data?: Partial<InputOutputOnlyExternalTypeWithRequiredNonscalarProp>): InputOutputOnlyExternalTypeWithRequiredNonscalarProp {
    return convertToModel(data || {}, metadata.InputOutputOnlyExternalTypeWithRequiredNonscalarProp) 
  }
  
  /** Maps the input object and its descendents to a new, valid InputOutputOnlyExternalTypeWithRequiredNonscalarProp implementation. */
  static map(data?: Partial<InputOutputOnlyExternalTypeWithRequiredNonscalarProp>): InputOutputOnlyExternalTypeWithRequiredNonscalarProp {
    return mapToModel(data || {}, metadata.InputOutputOnlyExternalTypeWithRequiredNonscalarProp) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.InputOutputOnlyExternalTypeWithRequiredNonscalarProp; }
  
  /** Instantiate a new InputOutputOnlyExternalTypeWithRequiredNonscalarProp, optionally basing it on the given data. */
  constructor(data?: Partial<InputOutputOnlyExternalTypeWithRequiredNonscalarProp> | {[k: string]: any}) {
    Object.assign(this, InputOutputOnlyExternalTypeWithRequiredNonscalarProp.map(data || {}));
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Location; }
  
  /** Instantiate a new Location, optionally basing it on the given data. */
  constructor(data?: Partial<Location> | {[k: string]: any}) {
    Object.assign(this, Location.map(data || {}));
  }
}


export interface OutputOnlyExternalTypeWithoutDefaultCtor extends Model<typeof metadata.OutputOnlyExternalTypeWithoutDefaultCtor> {
  bar: string | null
  baz: string | null
}
export class OutputOnlyExternalTypeWithoutDefaultCtor {
  
  /** Mutates the input object and its descendents into a valid OutputOnlyExternalTypeWithoutDefaultCtor implementation. */
  static convert(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtor>): OutputOnlyExternalTypeWithoutDefaultCtor {
    return convertToModel(data || {}, metadata.OutputOnlyExternalTypeWithoutDefaultCtor) 
  }
  
  /** Maps the input object and its descendents to a new, valid OutputOnlyExternalTypeWithoutDefaultCtor implementation. */
  static map(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtor>): OutputOnlyExternalTypeWithoutDefaultCtor {
    return mapToModel(data || {}, metadata.OutputOnlyExternalTypeWithoutDefaultCtor) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.OutputOnlyExternalTypeWithoutDefaultCtor; }
  
  /** Instantiate a new OutputOnlyExternalTypeWithoutDefaultCtor, optionally basing it on the given data. */
  constructor(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtor> | {[k: string]: any}) {
    Object.assign(this, OutputOnlyExternalTypeWithoutDefaultCtor.map(data || {}));
  }
}


export interface OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties extends Model<typeof metadata.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties> {
  message: string | null
}
export class OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties {
  
  /** Mutates the input object and its descendents into a valid OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties implementation. */
  static convert(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties>): OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties {
    return convertToModel(data || {}, metadata.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties) 
  }
  
  /** Maps the input object and its descendents to a new, valid OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties implementation. */
  static map(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties>): OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties {
    return mapToModel(data || {}, metadata.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties; }
  
  /** Instantiate a new OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties, optionally basing it on the given data. */
  constructor(data?: Partial<OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties> | {[k: string]: any}) {
    Object.assign(this, OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties.map(data || {}));
  }
}


export interface OutputOnlyExternalTypeWithRequiredEntityProp extends Model<typeof metadata.OutputOnlyExternalTypeWithRequiredEntityProp> {
  id: number | null
  entity: ComplexModel | null
}
export class OutputOnlyExternalTypeWithRequiredEntityProp {
  
  /** Mutates the input object and its descendents into a valid OutputOnlyExternalTypeWithRequiredEntityProp implementation. */
  static convert(data?: Partial<OutputOnlyExternalTypeWithRequiredEntityProp>): OutputOnlyExternalTypeWithRequiredEntityProp {
    return convertToModel(data || {}, metadata.OutputOnlyExternalTypeWithRequiredEntityProp) 
  }
  
  /** Maps the input object and its descendents to a new, valid OutputOnlyExternalTypeWithRequiredEntityProp implementation. */
  static map(data?: Partial<OutputOnlyExternalTypeWithRequiredEntityProp>): OutputOnlyExternalTypeWithRequiredEntityProp {
    return mapToModel(data || {}, metadata.OutputOnlyExternalTypeWithRequiredEntityProp) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.OutputOnlyExternalTypeWithRequiredEntityProp; }
  
  /** Instantiate a new OutputOnlyExternalTypeWithRequiredEntityProp, optionally basing it on the given data. */
  constructor(data?: Partial<OutputOnlyExternalTypeWithRequiredEntityProp> | {[k: string]: any}) {
    Object.assign(this, OutputOnlyExternalTypeWithRequiredEntityProp.map(data || {}));
  }
}


export interface PersonCriteria extends Model<typeof metadata.PersonCriteria> {
  personIds: number[] | null
  name: string | null
  subCriteria: PersonCriteria[] | null
  gender: Genders | null
  date: Date | null
  adminOnly: string | null
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.PersonCriteria; }
  
  /** Instantiate a new PersonCriteria, optionally basing it on the given data. */
  constructor(data?: Partial<PersonCriteria> | {[k: string]: any}) {
    Object.assign(this, PersonCriteria.map(data || {}));
  }
}


export interface PositionalRecord extends Model<typeof metadata.PositionalRecord> {
  string: string | null
  num: number | null
  date: Date | null
}
export class PositionalRecord {
  
  /** Mutates the input object and its descendents into a valid PositionalRecord implementation. */
  static convert(data?: Partial<PositionalRecord>): PositionalRecord {
    return convertToModel(data || {}, metadata.PositionalRecord) 
  }
  
  /** Maps the input object and its descendents to a new, valid PositionalRecord implementation. */
  static map(data?: Partial<PositionalRecord>): PositionalRecord {
    return mapToModel(data || {}, metadata.PositionalRecord) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.PositionalRecord; }
  
  /** Instantiate a new PositionalRecord, optionally basing it on the given data. */
  constructor(data?: Partial<PositionalRecord> | {[k: string]: any}) {
    Object.assign(this, PositionalRecord.map(data || {}));
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
  
  /** Mutates the input object and its descendents into a valid StandaloneReadWrite implementation. */
  static convert(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return convertToModel(data || {}, metadata.StandaloneReadWrite) 
  }
  
  /** Maps the input object and its descendents to a new, valid StandaloneReadWrite implementation. */
  static map(data?: Partial<StandaloneReadWrite>): StandaloneReadWrite {
    return mapToModel(data || {}, metadata.StandaloneReadWrite) 
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


export interface ValidationTarget extends Model<typeof metadata.ValidationTarget> {
  id: number | null
  productId: string | null
  email: string | null
  number: number | null
  optionalChild: ValidationTargetChild | null
  requiredChild: ValidationTargetChild | null
  nonInputtableNonNullableChild: ValidationTargetChild | null
}
export class ValidationTarget {
  
  /** Mutates the input object and its descendents into a valid ValidationTarget implementation. */
  static convert(data?: Partial<ValidationTarget>): ValidationTarget {
    return convertToModel(data || {}, metadata.ValidationTarget) 
  }
  
  /** Maps the input object and its descendents to a new, valid ValidationTarget implementation. */
  static map(data?: Partial<ValidationTarget>): ValidationTarget {
    return mapToModel(data || {}, metadata.ValidationTarget) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ValidationTarget; }
  
  /** Instantiate a new ValidationTarget, optionally basing it on the given data. */
  constructor(data?: Partial<ValidationTarget> | {[k: string]: any}) {
    Object.assign(this, ValidationTarget.map(data || {}));
  }
}


export interface ValidationTargetChild extends Model<typeof metadata.ValidationTargetChild> {
  string: string | null
  requiredVal: string | null
}
export class ValidationTargetChild {
  
  /** Mutates the input object and its descendents into a valid ValidationTargetChild implementation. */
  static convert(data?: Partial<ValidationTargetChild>): ValidationTargetChild {
    return convertToModel(data || {}, metadata.ValidationTargetChild) 
  }
  
  /** Maps the input object and its descendents to a new, valid ValidationTargetChild implementation. */
  static map(data?: Partial<ValidationTargetChild>): ValidationTargetChild {
    return mapToModel(data || {}, metadata.ValidationTargetChild) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.ValidationTargetChild; }
  
  /** Instantiate a new ValidationTargetChild, optionally basing it on the given data. */
  constructor(data?: Partial<ValidationTargetChild> | {[k: string]: any}) {
    Object.assign(this, ValidationTargetChild.map(data || {}));
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
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.WeatherData; }
  
  /** Instantiate a new WeatherData, optionally basing it on the given data. */
  constructor(data?: Partial<WeatherData> | {[k: string]: any}) {
    Object.assign(this, WeatherData.map(data || {}));
  }
}


declare module "coalesce-vue/lib/model" {
  interface EnumTypeLookup {
    EnumPkId: EnumPkId
    Genders: Genders
    SkyConditions: SkyConditions
    Statuses: Statuses
    Titles: Titles
  }
  interface ModelTypeLookup {
    AbstractImpl: AbstractImpl
    AbstractModel: AbstractModel
    Case: Case
    CaseDtoStandalone: CaseDtoStandalone
    CaseProduct: CaseProduct
    Company: Company
    ComplexModel: ComplexModel
    ComplexModelDependent: ComplexModelDependent
    EnumPk: EnumPk
    ExternalChild: ExternalChild
    ExternalChildAsInputOnly: ExternalChildAsInputOnly
    ExternalChildAsOutputOnly: ExternalChildAsOutputOnly
    ExternalParent: ExternalParent
    ExternalParentAsInputOnly: ExternalParentAsInputOnly
    ExternalParentAsOutputOnly: ExternalParentAsOutputOnly
    ExternalTypeWithDtoProp: ExternalTypeWithDtoProp
    InitRecordWithDefaultCtor: InitRecordWithDefaultCtor
    InputOutputOnlyExternalTypeWithRequiredNonscalarProp: InputOutputOnlyExternalTypeWithRequiredNonscalarProp
    Location: Location
    OutputOnlyExternalTypeWithoutDefaultCtor: OutputOnlyExternalTypeWithoutDefaultCtor
    OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties: OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties
    OutputOnlyExternalTypeWithRequiredEntityProp: OutputOnlyExternalTypeWithRequiredEntityProp
    Person: Person
    PersonCriteria: PersonCriteria
    PositionalRecord: PositionalRecord
    Product: Product
    ReadOnlyEntityUsedAsMethodInput: ReadOnlyEntityUsedAsMethodInput
    RecursiveHierarchy: RecursiveHierarchy
    RequiredAndInitModel: RequiredAndInitModel
    Sibling: Sibling
    StandaloneReadonly: StandaloneReadonly
    StandaloneReadWrite: StandaloneReadWrite
    StringIdentity: StringIdentity
    Test: Test
    ValidationTarget: ValidationTarget
    ValidationTargetChild: ValidationTargetChild
    WeatherData: WeatherData
    ZipCode: ZipCode
  }
}
