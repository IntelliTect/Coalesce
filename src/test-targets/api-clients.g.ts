import * as $metadata from './metadata.g'
import * as $models from './models.g'
import { ModelApiClient, ServiceApiClient } from 'coalesce-vue/lib/api-client'
import type { AxiosPromise, AxiosRequestConfig, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'

export class AbstractImpl1ApiClient extends ModelApiClient<$models.AbstractImpl1> {
  constructor() { super($metadata.AbstractImpl1) }
  
  public getId(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getId
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public getCount($config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getCount
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public echoAbstractModel(model?: $models.AbstractModel | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.AbstractModel>> {
    const $method = this.$metadata.methods.echoAbstractModel
    const $params =  {
      model,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class AbstractImpl2ApiClient extends ModelApiClient<$models.AbstractImpl2> {
  constructor() { super($metadata.AbstractImpl2) }
  
  public getId(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getId
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public getCount($config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getCount
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public echoAbstractModel(model?: $models.AbstractModel | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.AbstractModel>> {
    const $method = this.$metadata.methods.echoAbstractModel
    const $params =  {
      model,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class AbstractModelApiClient extends ModelApiClient<$models.AbstractModel> {
  constructor() { super($metadata.AbstractModel) }
  
  public getId(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getId
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public getCount($config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getCount
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public echoAbstractModel(model?: $models.AbstractModel | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.AbstractModel>> {
    const $method = this.$metadata.methods.echoAbstractModel
    const $params =  {
      model,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class AbstractModelPersonApiClient extends ModelApiClient<$models.AbstractModelPerson> {
  constructor() { super($metadata.AbstractModelPerson) }
}


export class CaseApiClient extends ModelApiClient<$models.Case> {
  constructor() { super($metadata.Case) }
  
  public methodWithJsReservedParamName(id: number | null, case_?: $models.Case | null, function_?: string | null, var_?: number | null, async_?: boolean | null, await_?: string | null, arguments_?: string[] | null, implements_?: string | null, delete_?: boolean | null, true_?: boolean | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.methodWithJsReservedParamName
    const $params =  {
      id,
      case: case_,
      function: function_,
      var: var_,
      async: async_,
      await: await_,
      arguments: arguments_,
      implements: implements_,
      delete: delete_,
      true: true_,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class CaseDtoStandaloneApiClient extends ModelApiClient<$models.CaseDtoStandalone> {
  constructor() { super($metadata.CaseDtoStandalone) }
}


export class CaseProductApiClient extends ModelApiClient<$models.CaseProduct> {
  constructor() { super($metadata.CaseProduct) }
}


export class CompanyApiClient extends ModelApiClient<$models.Company> {
  constructor() { super($metadata.Company) }
}


export class ComplexModelApiClient extends ModelApiClient<$models.ComplexModel> {
  constructor() { super($metadata.ComplexModel) }
  
  public methodWithManyParams(id: number | null, singleExternal?: $models.ExternalParent | null, collectionExternal?: $models.ExternalParent[] | null, file?: File | null, strParam?: string | null, stringsParam?: string[] | null, dateTime?: Date | null, integer?: number | null, boolParam?: boolean | null, enumParam?: $models.Statuses | null, enumsParam?: $models.Statuses[] | null, model?: $models.Test | null, modelCollection?: $models.Test[] | null, uri?: string | null, uris?: string[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalParent>> {
    const $method = this.$metadata.methods.methodWithManyParams
    const $params =  {
      id,
      singleExternal,
      collectionExternal,
      file,
      strParam,
      stringsParam,
      dateTime,
      integer,
      boolParam,
      enumParam,
      enumsParam,
      model,
      modelCollection,
      uri,
      uris,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOptionalParams(id: number | null, requiredInt: number | null, plainInt?: number | null, nullableInt?: number | null, intWithDefault?: number | null, enumWithDefault?: $models.Statuses | null, stringWithDefault?: string | null, optionalObject?: $models.Test | null, optionalObjectCollection?: $models.Test[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.methodWithOptionalParams
    const $params =  {
      id,
      requiredInt,
      plainInt,
      nullableInt,
      intWithDefault,
      enumWithDefault,
      stringWithDefault,
      optionalObject,
      optionalObjectCollection,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithRequiredAfterOptional(id: number | null, optionalInt: number | null, singleExternal: $models.ExternalParent | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.methodWithRequiredAfterOptional
    const $params =  {
      id,
      optionalInt,
      singleExternal,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public instanceGetMethodWithObjParam(id: number | null, obj?: $models.ExternalParent | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalParent>> {
    const $method = this.$metadata.methods.instanceGetMethodWithObjParam
    const $params =  {
      id,
      obj,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithExternalTypesWithSinglePurpose(id: number | null, single?: $models.ExternalParentAsInputOnly | null, collection?: $models.ExternalParentAsInputOnly[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalParentAsOutputOnly>> {
    const $method = this.$metadata.methods.methodWithExternalTypesWithSinglePurpose
    const $params =  {
      id,
      single,
      collection,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOutputOnlyExternalType(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithoutDefaultCtor>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOutputOnlyExternalType2(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType2
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOutputOnlyExternalType3(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithRequiredEntityProp>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType3
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp(id: number | null, i?: $models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp>> {
    const $method = this.$metadata.methods.methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp
    const $params =  {
      id,
      i,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithSingleFileParameter(id: number | null, file?: File | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithSingleFileParameter
    const $params =  {
      id,
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameter(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameter
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameterConcrete(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameterConcrete
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameterConcreteParam(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameterConcreteParam
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameterList(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameterList
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameterListConcrete(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameterListConcrete
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithMultiFileParameterListConcreteParam(id: number | null, files?: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameterListConcreteParam
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithStringArrayParameterAndReturn(strings?: string[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.methodWithStringArrayParameterAndReturn
    const $params =  {
      strings,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment_VaryByteArray(id: number | null, etag?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryByteArray
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment_VaryDate(id: number | null, etag?: Date | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryDate
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment_VaryString(id: number | null, etag?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryString
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment_VaryInt(id: number | null, etag?: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryInt
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachment_VaryGuid(id: number | null, etag?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryGuid
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachmentItemResult(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachmentItemResult
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public downloadAttachmentStatic($config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachmentStatic
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOptionalCancellationToken(id: number | null, q?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithOptionalCancellationToken
    const $params =  {
      id,
      q,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithOptionalEnumParam(id: number | null, status?: $models.Statuses | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithOptionalEnumParam
    const $params =  {
      id,
      status,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public externalTypeWithDtoProp(id: number | null, input?: $models.ExternalTypeWithDtoProp | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalTypeWithDtoProp>> {
    const $method = this.$metadata.methods.externalTypeWithDtoProp
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public customDto(id: number | null, input?: $models.CaseDtoStandalone | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.CaseDtoStandalone>> {
    const $method = this.$metadata.methods.customDto
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public sameMethodNameAsMethodOnDifferentType(id: number | null, input?: $models.CaseDtoStandalone | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.CaseDtoStandalone>> {
    const $method = this.$metadata.methods.sameMethodNameAsMethodOnDifferentType
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public hasTopLevelParamWithSameNameAsObjectProp(complexModelId?: number | null, model?: $models.ComplexModel | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.hasTopLevelParamWithSameNameAsObjectProp
    const $params =  {
      complexModelId,
      model,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithPositionRecord(id: number | null, rec?: $models.PositionalRecord | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.PositionalRecord>> {
    const $method = this.$metadata.methods.methodWithPositionRecord
    const $params =  {
      id,
      rec,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithInitRecord(id: number | null, rec?: $models.InitRecordWithDefaultCtor | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.InitRecordWithDefaultCtor>> {
    const $method = this.$metadata.methods.methodWithInitRecord
    const $params =  {
      id,
      rec,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithValidationExplicitOff(id: number | null, target: $models.ValidationTarget | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithValidationExplicitOff
    const $params =  {
      id,
      target,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithValidationExplicitOn(id: number | null, target: $models.ValidationTarget | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithValidationExplicitOn
    const $params =  {
      id,
      target,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class ComplexModelDependentApiClient extends ModelApiClient<$models.ComplexModelDependent> {
  constructor() { super($metadata.ComplexModelDependent) }
  
  public sameMethodNameAsMethodOnDifferentType(id: number | null, input?: $models.CaseDtoStandalone | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.CaseDtoStandalone>> {
    const $method = this.$metadata.methods.sameMethodNameAsMethodOnDifferentType
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class EnumPkApiClient extends ModelApiClient<$models.EnumPk> {
  constructor() { super($metadata.EnumPk) }
}


export class MultipleParentsApiClient extends ModelApiClient<$models.MultipleParents> {
  constructor() { super($metadata.MultipleParents) }
}


export class OneToOneChild1ApiClient extends ModelApiClient<$models.OneToOneChild1> {
  constructor() { super($metadata.OneToOneChild1) }
}


export class OneToOneChild2ApiClient extends ModelApiClient<$models.OneToOneChild2> {
  constructor() { super($metadata.OneToOneChild2) }
}


export class OneToOneManyChildrenApiClient extends ModelApiClient<$models.OneToOneManyChildren> {
  constructor() { super($metadata.OneToOneManyChildren) }
}


export class OneToOneParentApiClient extends ModelApiClient<$models.OneToOneParent> {
  constructor() { super($metadata.OneToOneParent) }
}


export class Parent1ApiClient extends ModelApiClient<$models.Parent1> {
  constructor() { super($metadata.Parent1) }
}


export class Parent2ApiClient extends ModelApiClient<$models.Parent2> {
  constructor() { super($metadata.Parent2) }
}


export class PersonApiClient extends ModelApiClient<$models.Person> {
  constructor() { super($metadata.Person) }
  
  /** Sets the FirstName to the given text. */
  public rename(id: number | null, name?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
    const $method = this.$metadata.methods.rename
    const $params =  {
      id,
      name,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  /** Removes spaces from the name and puts in dashes */
  public changeSpacesToDashesInName(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.changeSpacesToDashesInName
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  /** 
    Adds two numbers.
    
    This comment also includes multiple lines so I can test multi-line xmldoc comments.
  */
  public add(numberOne?: number | null, numberTwo?: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.add
    const $params =  {
      numberOne,
      numberTwo,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  /** Returns the user name */
  public getUser($config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.getUser
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  /** Returns the user name */
  public getUserPublic($config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.getUserPublic
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  /** Gets all the first names starting with the characters. */
  public namesStartingWith(characters?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.namesStartingWith
    const $params =  {
      characters,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public methodWithExplicitlyInjectedDataSource($config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
    const $method = this.$metadata.methods.methodWithExplicitlyInjectedDataSource
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class ProductApiClient extends ModelApiClient<$models.Product> {
  constructor() { super($metadata.Product) }
}


export class ReadOnlyEntityUsedAsMethodInputApiClient extends ModelApiClient<$models.ReadOnlyEntityUsedAsMethodInput> {
  constructor() { super($metadata.ReadOnlyEntityUsedAsMethodInput) }
  
  public staticCreate(foo?: $models.ReadOnlyEntityUsedAsMethodInput | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.staticCreate
    const $params =  {
      foo,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class RecursiveHierarchyApiClient extends ModelApiClient<$models.RecursiveHierarchy> {
  constructor() { super($metadata.RecursiveHierarchy) }
}


export class RequiredAndInitModelApiClient extends ModelApiClient<$models.RequiredAndInitModel> {
  constructor() { super($metadata.RequiredAndInitModel) }
}


export class SiblingApiClient extends ModelApiClient<$models.Sibling> {
  constructor() { super($metadata.Sibling) }
}


export class StandaloneReadonlyApiClient extends ModelApiClient<$models.StandaloneReadonly> {
  constructor() { super($metadata.StandaloneReadonly) }
  
  public instanceMethod(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.instanceMethod
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public staticMethod($config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.staticMethod
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class StandaloneReadWriteApiClient extends ModelApiClient<$models.StandaloneReadWrite> {
  constructor() { super($metadata.StandaloneReadWrite) }
}


export class StringIdentityApiClient extends ModelApiClient<$models.StringIdentity> {
  constructor() { super($metadata.StringIdentity) }
}


export class TestApiClient extends ModelApiClient<$models.Test> {
  constructor() { super($metadata.Test) }
}


export class ZipCodeApiClient extends ModelApiClient<$models.ZipCode> {
  constructor() { super($metadata.ZipCode) }
}


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  
  public getWeather(location: $models.Location | null, dateTime?: Date | null, conditions?: $models.SkyConditions | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.WeatherData>> {
    const $method = this.$metadata.methods.getWeather
    const $params =  {
      location,
      dateTime,
      conditions,
    }
    return this.$invoke($method, $params, $config)
  }
  
  
  public fileUploadDownload(file: File | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.fileUploadDownload
    const $params =  {
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


