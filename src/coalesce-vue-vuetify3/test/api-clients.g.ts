import * as $metadata from './metadata.g'
import * as $models from './models.g'
import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'
import { AxiosPromise, AxiosResponse, AxiosRequestConfig } from 'axios'

export class AbstractImplApiClient extends ModelApiClient<$models.AbstractImpl> {
  constructor() { super($metadata.AbstractImpl) }
}


export class AbstractModelApiClient extends ModelApiClient<$models.AbstractModel> {
  constructor() { super($metadata.AbstractModel) }
}


export class CaseApiClient extends ModelApiClient<$models.Case> {
  constructor() { super($metadata.Case) }
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
  public methodWithManyParams(id: number, singleExternal: $models.ExternalParent | null, collectionExternal: $models.ExternalParent[] | null, strParam: string | null, dateTime: Date | null, integer: number | null, model: $models.Company | null, modelCollection: $models.Company[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalParent>> {
    const $method = this.$metadata.methods.methodWithManyParams
    const $params =  {
      id,
      singleExternal,
      collectionExternal,
      strParam,
      dateTime,
      integer,
      model,
      modelCollection,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithExternalTypesWithSinglePurpose(id: number, single: $models.ExternalParentAsInputOnly | null, collection: $models.ExternalParentAsInputOnly[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalParentAsOutputOnly>> {
    const $method = this.$metadata.methods.methodWithExternalTypesWithSinglePurpose
    const $params =  {
      id,
      single,
      collection,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithOutputOnlyExternalType(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithoutDefaultCtor>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithOutputOnlyExternalType2(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType2
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithOutputOnlyExternalType3(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.OutputOnlyExternalTypeWithRequiredEntityProp>> {
    const $method = this.$metadata.methods.methodWithOutputOnlyExternalType3
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp(id: number, i: $models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.InputOutputOnlyExternalTypeWithRequiredNonscalarProp>> {
    const $method = this.$metadata.methods.methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp
    const $params =  {
      id,
      i,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithSingleFileParameter(id: number, file: File | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithSingleFileParameter
    const $params =  {
      id,
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithMultiFileParameter(id: number, files: File[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithMultiFileParameter
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithStringArrayParameterAndReturn(strings: string[] | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.methodWithStringArrayParameterAndReturn
    const $params =  {
      strings,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment_VaryByteArray(id: number, etag: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryByteArray
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment_VaryDate(id: number, etag: Date | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryDate
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment_VaryString(id: number, etag: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryString
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment_VaryInt(id: number, etag: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryInt
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachment_VaryGuid(id: number, etag: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadAttachment_VaryGuid
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadAttachmentItemResult(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
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
  
  public methodWithOptionalCancellationToken(id: number, q: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithOptionalCancellationToken
    const $params =  {
      id,
      q,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithOptionalEnumParam(id: number, status: $models.Statuses | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithOptionalEnumParam
    const $params =  {
      id,
      status,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public externalTypeWithDtoProp(id: number, input: $models.ExternalTypeWithDtoProp | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.ExternalTypeWithDtoProp>> {
    const $method = this.$metadata.methods.externalTypeWithDtoProp
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public customDto(id: number, input: $models.CaseDtoStandalone | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.CaseDtoStandalone>> {
    const $method = this.$metadata.methods.customDto
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public hasTopLevelParamWithSameNameAsObjectProp(complexModelId: number | null, model: $models.ComplexModel | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.hasTopLevelParamWithSameNameAsObjectProp
    const $params =  {
      complexModelId,
      model,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithPositionRecord(id: number, rec: $models.PositionalRecord | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.PositionalRecord>> {
    const $method = this.$metadata.methods.methodWithPositionRecord
    const $params =  {
      id,
      rec,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithInitRecord(id: number, rec: $models.InitRecordWithDefaultCtor | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.InitRecordWithDefaultCtor>> {
    const $method = this.$metadata.methods.methodWithInitRecord
    const $params =  {
      id,
      rec,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithValidationExplicitOff(id: number, target: $models.ValidationTarget | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.methodWithValidationExplicitOff
    const $params =  {
      id,
      target,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithValidationExplicitOn(id: number, target: $models.ValidationTarget | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
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
}


export class EnumPkApiClient extends ModelApiClient<$models.EnumPk> {
  constructor() { super($metadata.EnumPk) }
}


export class PersonApiClient extends ModelApiClient<$models.Person> {
  constructor() { super($metadata.Person) }
  public rename(id: number, name: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
    const $method = this.$metadata.methods.rename
    const $params =  {
      id,
      name,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public changeSpacesToDashesInName(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.changeSpacesToDashesInName
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public add(numberOne: number | null, numberTwo: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.add
    const $params =  {
      numberOne,
      numberTwo,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getUser($config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.getUser
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getUserPublic($config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.getUserPublic
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public namesStartingWith(characters: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.namesStartingWith
    const $params =  {
      characters,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class ProductApiClient extends ModelApiClient<$models.Product> {
  constructor() { super($metadata.Product) }
}


export class ReadOnlyEntityUsedAsMethodInputApiClient extends ModelApiClient<$models.ReadOnlyEntityUsedAsMethodInput> {
  constructor() { super($metadata.ReadOnlyEntityUsedAsMethodInput) }
  public staticCreate(foo: $models.ReadOnlyEntityUsedAsMethodInput | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.staticCreate
    const $params =  {
      foo,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class RequiredAndInitModelApiClient extends ModelApiClient<$models.RequiredAndInitModel> {
  constructor() { super($metadata.RequiredAndInitModel) }
}


export class SiblingApiClient extends ModelApiClient<$models.Sibling> {
  constructor() { super($metadata.Sibling) }
}


export class StandaloneReadonlyApiClient extends ModelApiClient<$models.StandaloneReadonly> {
  constructor() { super($metadata.StandaloneReadonly) }
  public instanceMethod(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
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


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  public getWeather(location: $models.Location | null, dateTime: Date | null, conditions: $models.SkyConditions | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.WeatherData>> {
    const $method = this.$metadata.methods.getWeather
    const $params =  {
      location,
      dateTime,
      conditions,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


