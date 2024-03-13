import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ServiceViewModel, DeepPartial, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface AbstractImplViewModel extends $models.AbstractImpl {
  implOnlyField: string | null;
  id: number | null;
  discriminatior: string | null;
}
export class AbstractImplViewModel extends ViewModel<$models.AbstractImpl, $apiClients.AbstractImplApiClient, number> implements $models.AbstractImpl  {
  
  constructor(initialData?: DeepPartial<$models.AbstractImpl> | null) {
    super($metadata.AbstractImpl, new $apiClients.AbstractImplApiClient(), initialData)
  }
}
defineProps(AbstractImplViewModel, $metadata.AbstractImpl)

export class AbstractImplListViewModel extends ListViewModel<$models.AbstractImpl, $apiClients.AbstractImplApiClient, AbstractImplViewModel> {
  
  constructor() {
    super($metadata.AbstractImpl, new $apiClients.AbstractImplApiClient())
  }
}


export interface AbstractModelViewModel extends $models.AbstractModel {
  id: number | null;
  discriminatior: string | null;
}
export class AbstractModelViewModel extends ViewModel<$models.AbstractModel, $apiClients.AbstractModelApiClient, number> implements $models.AbstractModel  {
  
  constructor(initialData?: DeepPartial<$models.AbstractModel> | null) {
    super($metadata.AbstractModel, new $apiClients.AbstractModelApiClient(), initialData)
  }
}
defineProps(AbstractModelViewModel, $metadata.AbstractModel)

export class AbstractModelListViewModel extends ListViewModel<$models.AbstractModel, $apiClients.AbstractModelApiClient, AbstractModelViewModel> {
  
  constructor() {
    super($metadata.AbstractModel, new $apiClients.AbstractModelApiClient())
  }
}


export interface CaseViewModel extends $models.Case {
  caseKey: number | null;
  title: string | null;
  description: string | null;
  openedAt: Date | null;
  assignedToId: number | null;
  assignedTo: PersonViewModel | null;
  reportedById: number | null;
  reportedBy: PersonViewModel | null;
  attachment: string | null;
  status: $models.Statuses | null;
  caseProducts: CaseProductViewModel[] | null;
}
export class CaseViewModel extends ViewModel<$models.Case, $apiClients.CaseApiClient, number> implements $models.Case  {
  
  
  public addToCaseProducts(initialData?: DeepPartial<$models.CaseProduct> | null) {
    return this.$addChild('caseProducts', initialData) as CaseProductViewModel
  }
  
  get products(): ReadonlyArray<ProductViewModel> {
    return (this.caseProducts || []).map($ => $.product!).filter($ => $)
  }
  
  constructor(initialData?: DeepPartial<$models.Case> | null) {
    super($metadata.Case, new $apiClients.CaseApiClient(), initialData)
  }
}
defineProps(CaseViewModel, $metadata.Case)

export class CaseListViewModel extends ListViewModel<$models.Case, $apiClients.CaseApiClient, CaseViewModel> {
  
  constructor() {
    super($metadata.Case, new $apiClients.CaseApiClient())
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
  phone: string | null;
  websiteUrl: string | null;
  logoUrl: string | null;
  employees: PersonViewModel[] | null;
  altName: string | null;
}
export class CompanyViewModel extends ViewModel<$models.Company, $apiClients.CompanyApiClient, number> implements $models.Company  {
  
  
  public addToEmployees(initialData?: DeepPartial<$models.Person> | null) {
    return this.$addChild('employees', initialData) as PersonViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.Company> | null) {
    super($metadata.Company, new $apiClients.CompanyApiClient(), initialData)
  }
}
defineProps(CompanyViewModel, $metadata.Company)

export class CompanyListViewModel extends ListViewModel<$models.Company, $apiClients.CompanyApiClient, CompanyViewModel> {
  
  constructor() {
    super($metadata.Company, new $apiClients.CompanyApiClient())
  }
}


export interface ComplexModelViewModel extends $models.ComplexModel {
  complexModelId: number | null;
  tests: TestViewModel[] | null;
  
  /** 
    Test case for foreign keys without a reference navigation prop.
    This configuration *will* be picked up by EF conventions.
  */
  childrenWithoutRefNavProp: ComplexModelDependentViewModel[] | null;
  singleTestId: number | null;
  singleTest: TestViewModel | null;
  enumPkId: $models.EnumPkId | null;
  enumPk: EnumPkViewModel | null;
  dateTimeOffset: Date | null;
  dateTimeOffsetNullable: Date | null;
  dateTime: Date | null;
  dateTimeNullable: Date | null;
  systemDateOnly: Date | null;
  systemTimeOnly: Date | null;
  dateOnlyViaAttribute: Date | null;
  unmappedSettableString: string | null;
  adminReadableString: string | null;
  restrictedString: string | null;
  restrictInit: string | null;
  adminReadableReferenceNavigationId: number | null;
  adminReadableReferenceNavigation: ComplexModelViewModel | null;
  referenceNavigationId: number | null;
  referenceNavigation: ComplexModelViewModel | null;
  noAutoIncludeReferenceNavigationId: number | null;
  noAutoIncludeReferenceNavigation: ComplexModelViewModel | null;
  noAutoIncludeByClassReferenceNavigationId: number | null;
  noAutoIncludeByClassReferenceNavigation: CompanyViewModel | null;
  name: string | null;
  byteArrayProp: string | null;
  string: string | null;
  stringWithDefault: string | null;
  intWithDefault: number | null;
  doubleWithDefault: number | null;
  enumWithDefault: $models.EnumPkId | null;
  color: string | null;
  stringSearchedEqualsInsensitive: string | null;
  stringSearchedEqualsNatural: string | null;
  int: number | null;
  intNullable: number | null;
  decimalNullable: number | null;
  long: number | null;
  guid: string | null;
  guidNullable: string | null;
  intCollection: number[] | null;
  nonNullNonZeroInt: number | null;
  clientValidationInt: number | null;
  clientValidationString: string | null;
  enumNullable: $models.Statuses | null;
  readOnlyPrimitiveCollection: string[] | null;
  mutablePrimitiveCollection: string[] | null;
  primitiveEnumerable: string[] | null;
}
export class ComplexModelViewModel extends ViewModel<$models.ComplexModel, $apiClients.ComplexModelApiClient, number> implements $models.ComplexModel  {
  
  
  public addToTests(initialData?: DeepPartial<$models.Test> | null) {
    return this.$addChild('tests', initialData) as TestViewModel
  }
  
  
  public addToChildrenWithoutRefNavProp(initialData?: DeepPartial<$models.ComplexModelDependent> | null) {
    return this.$addChild('childrenWithoutRefNavProp', initialData) as ComplexModelDependentViewModel
  }
  
  public get methodWithManyParams() {
    const methodWithManyParams = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithManyParams,
      (c, singleExternal?: $models.ExternalParent | null, collectionExternal?: $models.ExternalParent[] | null, file?: File | null, strParam?: string | null, stringsParam?: string[] | null, dateTime?: Date | null, integer?: number | null, boolParam?: boolean | null, enumParam?: $models.Statuses | null, model?: $models.Company | null, modelCollection?: $models.Company[] | null) => c.methodWithManyParams(this.$primaryKey, singleExternal, collectionExternal, file, strParam, stringsParam, dateTime, integer, boolParam, enumParam, model, modelCollection),
      () => ({singleExternal: null as $models.ExternalParent | null, collectionExternal: null as $models.ExternalParent[] | null, file: null as File | null, strParam: null as string | null, stringsParam: null as string[] | null, dateTime: null as Date | null, integer: null as number | null, boolParam: null as boolean | null, enumParam: null as $models.Statuses | null, model: null as $models.Company | null, modelCollection: null as $models.Company[] | null, }),
      (c, args) => c.methodWithManyParams(this.$primaryKey, args.singleExternal, args.collectionExternal, args.file, args.strParam, args.stringsParam, args.dateTime, args.integer, args.boolParam, args.enumParam, args.model, args.modelCollection))
    
    Object.defineProperty(this, 'methodWithManyParams', {value: methodWithManyParams});
    return methodWithManyParams
  }
  
  public get methodWithOptionalParams() {
    const methodWithOptionalParams = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOptionalParams,
      (c, requiredInt: number | null, plainInt?: number | null, nullableInt?: number | null, intWithDefault?: number | null, enumWithDefault?: $models.Statuses | null, stringWithDefault?: string | null) => c.methodWithOptionalParams(this.$primaryKey, requiredInt, plainInt, nullableInt, intWithDefault, enumWithDefault, stringWithDefault),
      () => ({requiredInt: null as number | null, plainInt: null as number | null, nullableInt: null as number | null, intWithDefault: null as number | null, enumWithDefault: null as $models.Statuses | null, stringWithDefault: null as string | null, }),
      (c, args) => c.methodWithOptionalParams(this.$primaryKey, args.requiredInt, args.plainInt, args.nullableInt, args.intWithDefault, args.enumWithDefault, args.stringWithDefault))
    
    Object.defineProperty(this, 'methodWithOptionalParams', {value: methodWithOptionalParams});
    return methodWithOptionalParams
  }
  
  public get methodWithRequiredAfterOptional() {
    const methodWithRequiredAfterOptional = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithRequiredAfterOptional,
      (c, optionalInt: number | null, singleExternal: $models.ExternalParent | null) => c.methodWithRequiredAfterOptional(this.$primaryKey, optionalInt, singleExternal),
      () => ({optionalInt: null as number | null, singleExternal: null as $models.ExternalParent | null, }),
      (c, args) => c.methodWithRequiredAfterOptional(this.$primaryKey, args.optionalInt, args.singleExternal))
    
    Object.defineProperty(this, 'methodWithRequiredAfterOptional', {value: methodWithRequiredAfterOptional});
    return methodWithRequiredAfterOptional
  }
  
  public get methodWithExternalTypesWithSinglePurpose() {
    const methodWithExternalTypesWithSinglePurpose = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithExternalTypesWithSinglePurpose,
      (c, single?: $models.ExternalParentAsInputOnly | null, collection?: $models.ExternalParentAsInputOnly[] | null) => c.methodWithExternalTypesWithSinglePurpose(this.$primaryKey, single, collection),
      () => ({single: null as $models.ExternalParentAsInputOnly | null, collection: null as $models.ExternalParentAsInputOnly[] | null, }),
      (c, args) => c.methodWithExternalTypesWithSinglePurpose(this.$primaryKey, args.single, args.collection))
    
    Object.defineProperty(this, 'methodWithExternalTypesWithSinglePurpose', {value: methodWithExternalTypesWithSinglePurpose});
    return methodWithExternalTypesWithSinglePurpose
  }
  
  public get methodWithOutputOnlyExternalType() {
    const methodWithOutputOnlyExternalType = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOutputOnlyExternalType,
      (c) => c.methodWithOutputOnlyExternalType(this.$primaryKey),
      () => ({}),
      (c, args) => c.methodWithOutputOnlyExternalType(this.$primaryKey))
    
    Object.defineProperty(this, 'methodWithOutputOnlyExternalType', {value: methodWithOutputOnlyExternalType});
    return methodWithOutputOnlyExternalType
  }
  
  public get methodWithOutputOnlyExternalType2() {
    const methodWithOutputOnlyExternalType2 = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOutputOnlyExternalType2,
      (c) => c.methodWithOutputOnlyExternalType2(this.$primaryKey),
      () => ({}),
      (c, args) => c.methodWithOutputOnlyExternalType2(this.$primaryKey))
    
    Object.defineProperty(this, 'methodWithOutputOnlyExternalType2', {value: methodWithOutputOnlyExternalType2});
    return methodWithOutputOnlyExternalType2
  }
  
  public get methodWithOutputOnlyExternalType3() {
    const methodWithOutputOnlyExternalType3 = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOutputOnlyExternalType3,
      (c) => c.methodWithOutputOnlyExternalType3(this.$primaryKey),
      () => ({}),
      (c, args) => c.methodWithOutputOnlyExternalType3(this.$primaryKey))
    
    Object.defineProperty(this, 'methodWithOutputOnlyExternalType3', {value: methodWithOutputOnlyExternalType3});
    return methodWithOutputOnlyExternalType3
  }
  
  public get methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp() {
    const methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp,
      (c, i?: $models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp | null) => c.methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp(this.$primaryKey, i),
      () => ({i: null as $models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp | null, }),
      (c, args) => c.methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp(this.$primaryKey, args.i))
    
    Object.defineProperty(this, 'methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp', {value: methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp});
    return methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp
  }
  
  public get methodWithSingleFileParameter() {
    const methodWithSingleFileParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithSingleFileParameter,
      (c, file?: File | null) => c.methodWithSingleFileParameter(this.$primaryKey, file),
      () => ({file: null as File | null, }),
      (c, args) => c.methodWithSingleFileParameter(this.$primaryKey, args.file))
    
    Object.defineProperty(this, 'methodWithSingleFileParameter', {value: methodWithSingleFileParameter});
    return methodWithSingleFileParameter
  }
  
  public get methodWithMultiFileParameter() {
    const methodWithMultiFileParameter = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithMultiFileParameter,
      (c, files?: File[] | null) => c.methodWithMultiFileParameter(this.$primaryKey, files),
      () => ({files: null as File[] | null, }),
      (c, args) => c.methodWithMultiFileParameter(this.$primaryKey, args.files))
    
    Object.defineProperty(this, 'methodWithMultiFileParameter', {value: methodWithMultiFileParameter});
    return methodWithMultiFileParameter
  }
  
  public get downloadAttachment() {
    const downloadAttachment = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment,
      (c) => c.downloadAttachment(this.$primaryKey),
      () => ({}),
      (c, args) => c.downloadAttachment(this.$primaryKey))
    
    Object.defineProperty(this, 'downloadAttachment', {value: downloadAttachment});
    return downloadAttachment
  }
  
  public get downloadAttachment_VaryByteArray() {
    const downloadAttachment_VaryByteArray = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment_VaryByteArray,
      (c) => c.downloadAttachment_VaryByteArray(this.$primaryKey, this.byteArrayProp),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryByteArray(this.$primaryKey, this.byteArrayProp))
    
    Object.defineProperty(this, 'downloadAttachment_VaryByteArray', {value: downloadAttachment_VaryByteArray});
    return downloadAttachment_VaryByteArray
  }
  
  public get downloadAttachment_VaryDate() {
    const downloadAttachment_VaryDate = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment_VaryDate,
      (c) => c.downloadAttachment_VaryDate(this.$primaryKey, this.dateTimeOffset),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryDate(this.$primaryKey, this.dateTimeOffset))
    
    Object.defineProperty(this, 'downloadAttachment_VaryDate', {value: downloadAttachment_VaryDate});
    return downloadAttachment_VaryDate
  }
  
  public get downloadAttachment_VaryString() {
    const downloadAttachment_VaryString = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment_VaryString,
      (c) => c.downloadAttachment_VaryString(this.$primaryKey, this.name),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryString(this.$primaryKey, this.name))
    
    Object.defineProperty(this, 'downloadAttachment_VaryString', {value: downloadAttachment_VaryString});
    return downloadAttachment_VaryString
  }
  
  public get downloadAttachment_VaryInt() {
    const downloadAttachment_VaryInt = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment_VaryInt,
      (c) => c.downloadAttachment_VaryInt(this.$primaryKey, this.int),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryInt(this.$primaryKey, this.int))
    
    Object.defineProperty(this, 'downloadAttachment_VaryInt', {value: downloadAttachment_VaryInt});
    return downloadAttachment_VaryInt
  }
  
  public get downloadAttachment_VaryGuid() {
    const downloadAttachment_VaryGuid = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachment_VaryGuid,
      (c) => c.downloadAttachment_VaryGuid(this.$primaryKey, this.guid),
      () => ({}),
      (c, args) => c.downloadAttachment_VaryGuid(this.$primaryKey, this.guid))
    
    Object.defineProperty(this, 'downloadAttachment_VaryGuid', {value: downloadAttachment_VaryGuid});
    return downloadAttachment_VaryGuid
  }
  
  public get downloadAttachmentItemResult() {
    const downloadAttachmentItemResult = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachmentItemResult,
      (c) => c.downloadAttachmentItemResult(this.$primaryKey),
      () => ({}),
      (c, args) => c.downloadAttachmentItemResult(this.$primaryKey))
    
    Object.defineProperty(this, 'downloadAttachmentItemResult', {value: downloadAttachmentItemResult});
    return downloadAttachmentItemResult
  }
  
  public get methodWithOptionalCancellationToken() {
    const methodWithOptionalCancellationToken = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOptionalCancellationToken,
      (c, q?: string | null) => c.methodWithOptionalCancellationToken(this.$primaryKey, q),
      () => ({q: null as string | null, }),
      (c, args) => c.methodWithOptionalCancellationToken(this.$primaryKey, args.q))
    
    Object.defineProperty(this, 'methodWithOptionalCancellationToken', {value: methodWithOptionalCancellationToken});
    return methodWithOptionalCancellationToken
  }
  
  public get methodWithOptionalEnumParam() {
    const methodWithOptionalEnumParam = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithOptionalEnumParam,
      (c, status?: $models.Statuses | null) => c.methodWithOptionalEnumParam(this.$primaryKey, status),
      () => ({status: null as $models.Statuses | null, }),
      (c, args) => c.methodWithOptionalEnumParam(this.$primaryKey, args.status))
    
    Object.defineProperty(this, 'methodWithOptionalEnumParam', {value: methodWithOptionalEnumParam});
    return methodWithOptionalEnumParam
  }
  
  public get externalTypeWithDtoProp() {
    const externalTypeWithDtoProp = this.$apiClient.$makeCaller(
      this.$metadata.methods.externalTypeWithDtoProp,
      (c, input?: $models.ExternalTypeWithDtoProp | null) => c.externalTypeWithDtoProp(this.$primaryKey, input),
      () => ({input: null as $models.ExternalTypeWithDtoProp | null, }),
      (c, args) => c.externalTypeWithDtoProp(this.$primaryKey, args.input))
    
    Object.defineProperty(this, 'externalTypeWithDtoProp', {value: externalTypeWithDtoProp});
    return externalTypeWithDtoProp
  }
  
  public get customDto() {
    const customDto = this.$apiClient.$makeCaller(
      this.$metadata.methods.customDto,
      (c, input?: $models.CaseDtoStandalone | null) => c.customDto(this.$primaryKey, input),
      () => ({input: null as $models.CaseDtoStandalone | null, }),
      (c, args) => c.customDto(this.$primaryKey, args.input))
    
    Object.defineProperty(this, 'customDto', {value: customDto});
    return customDto
  }
  
  public get methodWithPositionRecord() {
    const methodWithPositionRecord = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithPositionRecord,
      (c, rec?: $models.PositionalRecord | null) => c.methodWithPositionRecord(this.$primaryKey, rec),
      () => ({rec: null as $models.PositionalRecord | null, }),
      (c, args) => c.methodWithPositionRecord(this.$primaryKey, args.rec))
    
    Object.defineProperty(this, 'methodWithPositionRecord', {value: methodWithPositionRecord});
    return methodWithPositionRecord
  }
  
  public get methodWithInitRecord() {
    const methodWithInitRecord = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithInitRecord,
      (c, rec?: $models.InitRecordWithDefaultCtor | null) => c.methodWithInitRecord(this.$primaryKey, rec),
      () => ({rec: null as $models.InitRecordWithDefaultCtor | null, }),
      (c, args) => c.methodWithInitRecord(this.$primaryKey, args.rec))
    
    Object.defineProperty(this, 'methodWithInitRecord', {value: methodWithInitRecord});
    return methodWithInitRecord
  }
  
  public get methodWithValidationExplicitOff() {
    const methodWithValidationExplicitOff = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithValidationExplicitOff,
      (c, target: $models.ValidationTarget | null) => c.methodWithValidationExplicitOff(this.$primaryKey, target),
      () => ({target: null as $models.ValidationTarget | null, }),
      (c, args) => c.methodWithValidationExplicitOff(this.$primaryKey, args.target))
    
    Object.defineProperty(this, 'methodWithValidationExplicitOff', {value: methodWithValidationExplicitOff});
    return methodWithValidationExplicitOff
  }
  
  public get methodWithValidationExplicitOn() {
    const methodWithValidationExplicitOn = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithValidationExplicitOn,
      (c, target: $models.ValidationTarget | null) => c.methodWithValidationExplicitOn(this.$primaryKey, target),
      () => ({target: null as $models.ValidationTarget | null, }),
      (c, args) => c.methodWithValidationExplicitOn(this.$primaryKey, args.target))
    
    Object.defineProperty(this, 'methodWithValidationExplicitOn', {value: methodWithValidationExplicitOn});
    return methodWithValidationExplicitOn
  }
  
  constructor(initialData?: DeepPartial<$models.ComplexModel> | null) {
    super($metadata.ComplexModel, new $apiClients.ComplexModelApiClient(), initialData)
  }
}
defineProps(ComplexModelViewModel, $metadata.ComplexModel)

export class ComplexModelListViewModel extends ListViewModel<$models.ComplexModel, $apiClients.ComplexModelApiClient, ComplexModelViewModel> {
  
  public get methodWithStringArrayParameterAndReturn() {
    const methodWithStringArrayParameterAndReturn = this.$apiClient.$makeCaller(
      this.$metadata.methods.methodWithStringArrayParameterAndReturn,
      (c, strings?: string[] | null) => c.methodWithStringArrayParameterAndReturn(strings),
      () => ({strings: null as string[] | null, }),
      (c, args) => c.methodWithStringArrayParameterAndReturn(args.strings))
    
    Object.defineProperty(this, 'methodWithStringArrayParameterAndReturn', {value: methodWithStringArrayParameterAndReturn});
    return methodWithStringArrayParameterAndReturn
  }
  
  public get downloadAttachmentStatic() {
    const downloadAttachmentStatic = this.$apiClient.$makeCaller(
      this.$metadata.methods.downloadAttachmentStatic,
      (c) => c.downloadAttachmentStatic(),
      () => ({}),
      (c, args) => c.downloadAttachmentStatic())
    
    Object.defineProperty(this, 'downloadAttachmentStatic', {value: downloadAttachmentStatic});
    return downloadAttachmentStatic
  }
  
  public get hasTopLevelParamWithSameNameAsObjectProp() {
    const hasTopLevelParamWithSameNameAsObjectProp = this.$apiClient.$makeCaller(
      this.$metadata.methods.hasTopLevelParamWithSameNameAsObjectProp,
      (c, complexModelId?: number | null, model?: $models.ComplexModel | null) => c.hasTopLevelParamWithSameNameAsObjectProp(complexModelId, model),
      () => ({complexModelId: null as number | null, model: null as $models.ComplexModel | null, }),
      (c, args) => c.hasTopLevelParamWithSameNameAsObjectProp(args.complexModelId, args.model))
    
    Object.defineProperty(this, 'hasTopLevelParamWithSameNameAsObjectProp', {value: hasTopLevelParamWithSameNameAsObjectProp});
    return hasTopLevelParamWithSameNameAsObjectProp
  }
  
  constructor() {
    super($metadata.ComplexModel, new $apiClients.ComplexModelApiClient())
  }
}


export interface ComplexModelDependentViewModel extends $models.ComplexModelDependent {
  id: number | null;
  parentId: number | null;
  name: string | null;
}
export class ComplexModelDependentViewModel extends ViewModel<$models.ComplexModelDependent, $apiClients.ComplexModelDependentApiClient, number> implements $models.ComplexModelDependent  {
  
  constructor(initialData?: DeepPartial<$models.ComplexModelDependent> | null) {
    super($metadata.ComplexModelDependent, new $apiClients.ComplexModelDependentApiClient(), initialData)
  }
}
defineProps(ComplexModelDependentViewModel, $metadata.ComplexModelDependent)

export class ComplexModelDependentListViewModel extends ListViewModel<$models.ComplexModelDependent, $apiClients.ComplexModelDependentApiClient, ComplexModelDependentViewModel> {
  
  constructor() {
    super($metadata.ComplexModelDependent, new $apiClients.ComplexModelDependentApiClient())
  }
}


export interface EnumPkViewModel extends $models.EnumPk {
  enumPkId: $models.EnumPkId | null;
  name: string | null;
}
export class EnumPkViewModel extends ViewModel<$models.EnumPk, $apiClients.EnumPkApiClient, number> implements $models.EnumPk  {
  
  constructor(initialData?: DeepPartial<$models.EnumPk> | null) {
    super($metadata.EnumPk, new $apiClients.EnumPkApiClient(), initialData)
  }
}
defineProps(EnumPkViewModel, $metadata.EnumPk)

export class EnumPkListViewModel extends ListViewModel<$models.EnumPk, $apiClients.EnumPkApiClient, EnumPkViewModel> {
  
  constructor() {
    super($metadata.EnumPk, new $apiClients.EnumPkApiClient())
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
  secretPhrase: string | null;
  
  /** Genetic Gender of the person. */
  gender: $models.Genders | null;
  
  /** List of cases assigned to the person */
  casesAssigned: CaseViewModel[] | null;
  
  /** List of cases reported by the person. */
  casesReported: CaseViewModel[] | null;
  birthDate: Date | null;
  
  /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
  name: string | null;
  
  /** Company ID this person is employed by */
  companyId: number | null;
  
  /** Company loaded from the Company ID */
  company: CompanyViewModel | null;
  siblingRelationships: SiblingViewModel[] | null;
}
export class PersonViewModel extends ViewModel<$models.Person, $apiClients.PersonApiClient, number> implements $models.Person  {
  
  
  public addToCasesAssigned(initialData?: DeepPartial<$models.Case> | null) {
    return this.$addChild('casesAssigned', initialData) as CaseViewModel
  }
  
  
  public addToCasesReported(initialData?: DeepPartial<$models.Case> | null) {
    return this.$addChild('casesReported', initialData) as CaseViewModel
  }
  
  
  public addToSiblingRelationships(initialData?: DeepPartial<$models.Sibling> | null) {
    return this.$addChild('siblingRelationships', initialData) as SiblingViewModel
  }
  
  get siblings(): ReadonlyArray<PersonViewModel> {
    return (this.siblingRelationships || []).map($ => $.personTwo!).filter($ => $)
  }
  
  /** Sets the FirstName to the given text. */
  public get rename() {
    const rename = this.$apiClient.$makeCaller(
      this.$metadata.methods.rename,
      (c, name?: string | null) => c.rename(this.$primaryKey, name),
      () => ({name: null as string | null, }),
      (c, args) => c.rename(this.$primaryKey, args.name))
    
    Object.defineProperty(this, 'rename', {value: rename});
    return rename
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
  
  constructor(initialData?: DeepPartial<$models.Person> | null) {
    super($metadata.Person, new $apiClients.PersonApiClient(), initialData)
  }
}
defineProps(PersonViewModel, $metadata.Person)

export class PersonListViewModel extends ListViewModel<$models.Person, $apiClients.PersonApiClient, PersonViewModel> {
  
  /** Adds two numbers. */
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
      (c, characters?: string | null) => c.namesStartingWith(characters),
      () => ({characters: null as string | null, }),
      (c, args) => c.namesStartingWith(args.characters))
    
    Object.defineProperty(this, 'namesStartingWith', {value: namesStartingWith});
    return namesStartingWith
  }
  
  constructor() {
    super($metadata.Person, new $apiClients.PersonApiClient())
  }
}


export interface ProductViewModel extends $models.Product {
  productId: number | null;
  name: string | null;
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


export interface ReadOnlyEntityUsedAsMethodInputViewModel extends $models.ReadOnlyEntityUsedAsMethodInput {
  id: number | null;
  name: string | null;
}
export class ReadOnlyEntityUsedAsMethodInputViewModel extends ViewModel<$models.ReadOnlyEntityUsedAsMethodInput, $apiClients.ReadOnlyEntityUsedAsMethodInputApiClient, number> implements $models.ReadOnlyEntityUsedAsMethodInput  {
  
  constructor(initialData?: DeepPartial<$models.ReadOnlyEntityUsedAsMethodInput> | null) {
    super($metadata.ReadOnlyEntityUsedAsMethodInput, new $apiClients.ReadOnlyEntityUsedAsMethodInputApiClient(), initialData)
  }
}
defineProps(ReadOnlyEntityUsedAsMethodInputViewModel, $metadata.ReadOnlyEntityUsedAsMethodInput)

export class ReadOnlyEntityUsedAsMethodInputListViewModel extends ListViewModel<$models.ReadOnlyEntityUsedAsMethodInput, $apiClients.ReadOnlyEntityUsedAsMethodInputApiClient, ReadOnlyEntityUsedAsMethodInputViewModel> {
  
  public get staticCreate() {
    const staticCreate = this.$apiClient.$makeCaller(
      this.$metadata.methods.staticCreate,
      (c, foo?: $models.ReadOnlyEntityUsedAsMethodInput | null) => c.staticCreate(foo),
      () => ({foo: null as $models.ReadOnlyEntityUsedAsMethodInput | null, }),
      (c, args) => c.staticCreate(args.foo))
    
    Object.defineProperty(this, 'staticCreate', {value: staticCreate});
    return staticCreate
  }
  
  constructor() {
    super($metadata.ReadOnlyEntityUsedAsMethodInput, new $apiClients.ReadOnlyEntityUsedAsMethodInputApiClient())
  }
}


export interface RecursiveHierarchyViewModel extends $models.RecursiveHierarchy {
  id: number | null;
  name: string | null;
  parentId: number | null;
  parent: RecursiveHierarchyViewModel | null;
  children: RecursiveHierarchyViewModel[] | null;
}
export class RecursiveHierarchyViewModel extends ViewModel<$models.RecursiveHierarchy, $apiClients.RecursiveHierarchyApiClient, number> implements $models.RecursiveHierarchy  {
  
  
  public addToChildren(initialData?: DeepPartial<$models.RecursiveHierarchy> | null) {
    return this.$addChild('children', initialData) as RecursiveHierarchyViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.RecursiveHierarchy> | null) {
    super($metadata.RecursiveHierarchy, new $apiClients.RecursiveHierarchyApiClient(), initialData)
  }
}
defineProps(RecursiveHierarchyViewModel, $metadata.RecursiveHierarchy)

export class RecursiveHierarchyListViewModel extends ListViewModel<$models.RecursiveHierarchy, $apiClients.RecursiveHierarchyApiClient, RecursiveHierarchyViewModel> {
  
  constructor() {
    super($metadata.RecursiveHierarchy, new $apiClients.RecursiveHierarchyApiClient())
  }
}


export interface RequiredAndInitModelViewModel extends $models.RequiredAndInitModel {
  id: number | null;
  requiredRef: string | null;
  requiredValue: number | null;
  requiredInitRef: string | null;
  requiredInitValue: number | null;
  initRef: string | null;
  initValue: number | null;
}
export class RequiredAndInitModelViewModel extends ViewModel<$models.RequiredAndInitModel, $apiClients.RequiredAndInitModelApiClient, number> implements $models.RequiredAndInitModel  {
  
  constructor(initialData?: DeepPartial<$models.RequiredAndInitModel> | null) {
    super($metadata.RequiredAndInitModel, new $apiClients.RequiredAndInitModelApiClient(), initialData)
  }
}
defineProps(RequiredAndInitModelViewModel, $metadata.RequiredAndInitModel)

export class RequiredAndInitModelListViewModel extends ListViewModel<$models.RequiredAndInitModel, $apiClients.RequiredAndInitModelApiClient, RequiredAndInitModelViewModel> {
  
  constructor() {
    super($metadata.RequiredAndInitModel, new $apiClients.RequiredAndInitModelApiClient())
  }
}


export interface SiblingViewModel extends $models.Sibling {
  siblingId: number | null;
  personId: number | null;
  person: PersonViewModel | null;
  personTwoId: number | null;
  personTwo: PersonViewModel | null;
}
export class SiblingViewModel extends ViewModel<$models.Sibling, $apiClients.SiblingApiClient, number> implements $models.Sibling  {
  
  constructor(initialData?: DeepPartial<$models.Sibling> | null) {
    super($metadata.Sibling, new $apiClients.SiblingApiClient(), initialData)
  }
}
defineProps(SiblingViewModel, $metadata.Sibling)

export class SiblingListViewModel extends ListViewModel<$models.Sibling, $apiClients.SiblingApiClient, SiblingViewModel> {
  
  constructor() {
    super($metadata.Sibling, new $apiClients.SiblingApiClient())
  }
}


export interface StandaloneReadonlyViewModel extends $models.StandaloneReadonly {
  id: number | null;
  name: string | null;
  description: string | null;
}
export class StandaloneReadonlyViewModel extends ViewModel<$models.StandaloneReadonly, $apiClients.StandaloneReadonlyApiClient, number> implements $models.StandaloneReadonly  {
  
  public get instanceMethod() {
    const instanceMethod = this.$apiClient.$makeCaller(
      this.$metadata.methods.instanceMethod,
      (c) => c.instanceMethod(this.$primaryKey),
      () => ({}),
      (c, args) => c.instanceMethod(this.$primaryKey))
    
    Object.defineProperty(this, 'instanceMethod', {value: instanceMethod});
    return instanceMethod
  }
  
  constructor(initialData?: DeepPartial<$models.StandaloneReadonly> | null) {
    super($metadata.StandaloneReadonly, new $apiClients.StandaloneReadonlyApiClient(), initialData)
  }
}
defineProps(StandaloneReadonlyViewModel, $metadata.StandaloneReadonly)

export class StandaloneReadonlyListViewModel extends ListViewModel<$models.StandaloneReadonly, $apiClients.StandaloneReadonlyApiClient, StandaloneReadonlyViewModel> {
  
  public get staticMethod() {
    const staticMethod = this.$apiClient.$makeCaller(
      this.$metadata.methods.staticMethod,
      (c) => c.staticMethod(),
      () => ({}),
      (c, args) => c.staticMethod())
    
    Object.defineProperty(this, 'staticMethod', {value: staticMethod});
    return staticMethod
  }
  
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
  
  constructor(initialData?: DeepPartial<$models.StandaloneReadWrite> | null) {
    super($metadata.StandaloneReadWrite, new $apiClients.StandaloneReadWriteApiClient(), initialData)
  }
}
defineProps(StandaloneReadWriteViewModel, $metadata.StandaloneReadWrite)

export class StandaloneReadWriteListViewModel extends ListViewModel<$models.StandaloneReadWrite, $apiClients.StandaloneReadWriteApiClient, StandaloneReadWriteViewModel> {
  
  constructor() {
    super($metadata.StandaloneReadWrite, new $apiClients.StandaloneReadWriteApiClient())
  }
}


export interface StringIdentityViewModel extends $models.StringIdentity {
  stringIdentityId: string | null;
  parentId: string | null;
  parent: StringIdentityViewModel | null;
  parentReqId: string | null;
  parentReq: StringIdentityViewModel | null;
  children: StringIdentityViewModel[] | null;
}
export class StringIdentityViewModel extends ViewModel<$models.StringIdentity, $apiClients.StringIdentityApiClient, string> implements $models.StringIdentity  {
  
  
  public addToChildren(initialData?: DeepPartial<$models.StringIdentity> | null) {
    return this.$addChild('children', initialData) as StringIdentityViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.StringIdentity> | null) {
    super($metadata.StringIdentity, new $apiClients.StringIdentityApiClient(), initialData)
  }
}
defineProps(StringIdentityViewModel, $metadata.StringIdentity)

export class StringIdentityListViewModel extends ListViewModel<$models.StringIdentity, $apiClients.StringIdentityApiClient, StringIdentityViewModel> {
  
  constructor() {
    super($metadata.StringIdentity, new $apiClients.StringIdentityApiClient())
  }
}


export interface TestViewModel extends $models.Test {
  testId: number | null;
  complexModelId: number | null;
  complexModel: ComplexModelViewModel | null;
  testName: string | null;
}
export class TestViewModel extends ViewModel<$models.Test, $apiClients.TestApiClient, number> implements $models.Test  {
  
  constructor(initialData?: DeepPartial<$models.Test> | null) {
    super($metadata.Test, new $apiClients.TestApiClient(), initialData)
  }
}
defineProps(TestViewModel, $metadata.Test)

export class TestListViewModel extends ListViewModel<$models.Test, $apiClients.TestApiClient, TestViewModel> {
  
  constructor() {
    super($metadata.Test, new $apiClients.TestApiClient())
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
  AbstractImpl: AbstractImplViewModel,
  AbstractModel: AbstractModelViewModel,
  Case: CaseViewModel,
  CaseDtoStandalone: CaseDtoStandaloneViewModel,
  CaseProduct: CaseProductViewModel,
  Company: CompanyViewModel,
  ComplexModel: ComplexModelViewModel,
  ComplexModelDependent: ComplexModelDependentViewModel,
  EnumPk: EnumPkViewModel,
  Person: PersonViewModel,
  Product: ProductViewModel,
  ReadOnlyEntityUsedAsMethodInput: ReadOnlyEntityUsedAsMethodInputViewModel,
  RecursiveHierarchy: RecursiveHierarchyViewModel,
  RequiredAndInitModel: RequiredAndInitModelViewModel,
  Sibling: SiblingViewModel,
  StandaloneReadonly: StandaloneReadonlyViewModel,
  StandaloneReadWrite: StandaloneReadWriteViewModel,
  StringIdentity: StringIdentityViewModel,
  Test: TestViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  AbstractImpl: AbstractImplListViewModel,
  AbstractModel: AbstractModelListViewModel,
  Case: CaseListViewModel,
  CaseDtoStandalone: CaseDtoStandaloneListViewModel,
  CaseProduct: CaseProductListViewModel,
  Company: CompanyListViewModel,
  ComplexModel: ComplexModelListViewModel,
  ComplexModelDependent: ComplexModelDependentListViewModel,
  EnumPk: EnumPkListViewModel,
  Person: PersonListViewModel,
  Product: ProductListViewModel,
  ReadOnlyEntityUsedAsMethodInput: ReadOnlyEntityUsedAsMethodInputListViewModel,
  RecursiveHierarchy: RecursiveHierarchyListViewModel,
  RequiredAndInitModel: RequiredAndInitModelListViewModel,
  Sibling: SiblingListViewModel,
  StandaloneReadonly: StandaloneReadonlyListViewModel,
  StandaloneReadWrite: StandaloneReadWriteListViewModel,
  StringIdentity: StringIdentityListViewModel,
  Test: TestListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  WeatherService: WeatherServiceViewModel,
}

