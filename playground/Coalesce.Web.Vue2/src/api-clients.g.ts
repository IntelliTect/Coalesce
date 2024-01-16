import * as $metadata from './metadata.g'
import * as $models from './models.g'
import { AxiosPromise, AxiosRequestConfig, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'

export class AuditLogApiClient extends ModelApiClient<$models.AuditLog> {
  constructor() { super($metadata.AuditLog) }
}


export class AuditLogPropertyApiClient extends ModelApiClient<$models.AuditLogProperty> {
  constructor() { super($metadata.AuditLogProperty) }
}


export class CaseApiClient extends ModelApiClient<$models.Case> {
  constructor() { super($metadata.Case) }
  public getCaseTitles(search: string, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.getCaseTitles
    const $params =  {
      search,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getSomeCases($config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Case[]>> {
    const $method = this.$metadata.methods.getSomeCases
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getAllOpenCasesCount($config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.getAllOpenCasesCount
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public randomizeDatesAndStatus($config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.randomizeDatesAndStatus
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public uploadImage(id: number, file: File, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.uploadImage
    const $params =  {
      id,
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public downloadImage(id: number, etag?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.downloadImage
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public uploadAndDownload(id: number, file: File, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.uploadAndDownload
    const $params =  {
      id,
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public uploadImages(id: number, files: File[], $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.uploadImages
    const $params =  {
      id,
      files,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public uploadByteArray(id: number, file: string | Uint8Array, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.uploadByteArray
    const $params =  {
      id,
      file,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getCaseSummary($config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.CaseSummary>> {
    const $method = this.$metadata.methods.getCaseSummary
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class CaseDtoApiClient extends ModelApiClient<$models.CaseDto> {
  constructor() { super($metadata.CaseDto) }
  public asyncMethodOnIClassDto(id: number, input: string, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.asyncMethodOnIClassDto
    const $params =  {
      id,
      input,
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
  public conflictingParameterNames(id: number, companyParam: $models.Company, name: string, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.conflictingParameterNames
    const $params =  {
      id,
      companyParam,
      name,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getCertainItems(isDeleted?: boolean | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Company[]>> {
    const $method = this.$metadata.methods.getCertainItems
    const $params =  {
      isDeleted,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class LogApiClient extends ModelApiClient<$models.Log> {
  constructor() { super($metadata.Log) }
}


export class PersonApiClient extends ModelApiClient<$models.Person> {
  constructor() { super($metadata.Person) }
  public rename(id: number, name: string, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
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
  
  public add(numberOne?: number | null, numberTwo?: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
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
  
  public getBirthdate(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<Date>> {
    const $method = this.$metadata.methods.getBirthdate
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public setBirthDate(id: number, date?: Date | null, time?: Date | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.setBirthDate
    const $params =  {
      id,
      date,
      time,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public personCount(lastNameStartsWith?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
    const $method = this.$metadata.methods.personCount
    const $params =  {
      lastNameStartsWith,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public fullNameAndAge(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.fullNameAndAge
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public removePersonById(id?: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<boolean>> {
    const $method = this.$metadata.methods.removePersonById
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public obfuscateEmail(id: number, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.obfuscateEmail
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public changeFirstName(id: number, firstName: string, title?: $models.Titles | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
    const $method = this.$metadata.methods.changeFirstName
    const $params =  {
      id,
      firstName,
      title,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getUserPublic($config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.getUserPublic
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
  public namesStartingWith(characters: string, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.namesStartingWith
    const $params =  {
      characters,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithStringArrayParameter(strings: string[], $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.methodWithStringArrayParameter
    const $params =  {
      strings,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public methodWithEntityParameter(person: $models.Person, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
    const $method = this.$metadata.methods.methodWithEntityParameter
    const $params =  {
      person,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public searchPeople(criteria: $models.PersonCriteria, page?: number | null, $config?: AxiosRequestConfig): AxiosPromise<ListResult<$models.Person>> {
    const $method = this.$metadata.methods.searchPeople
    const $params =  {
      criteria,
      page,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class ProductApiClient extends ModelApiClient<$models.Product> {
  constructor() { super($metadata.Product) }
}


export class StandaloneReadCreateApiClient extends ModelApiClient<$models.StandaloneReadCreate> {
  constructor() { super($metadata.StandaloneReadCreate) }
}


export class StandaloneReadonlyApiClient extends ModelApiClient<$models.StandaloneReadonly> {
  constructor() { super($metadata.StandaloneReadonly) }
}


export class StandaloneReadWriteApiClient extends ModelApiClient<$models.StandaloneReadWrite> {
  constructor() { super($metadata.StandaloneReadWrite) }
}


export class ZipCodeApiClient extends ModelApiClient<$models.ZipCode> {
  constructor() { super($metadata.ZipCode) }
}


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  public getWeather(location: $models.Location, dateTime?: Date | null, conditions?: $models.SkyConditions | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.WeatherData>> {
    const $method = this.$metadata.methods.getWeather
    const $params =  {
      location,
      dateTime,
      conditions,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


