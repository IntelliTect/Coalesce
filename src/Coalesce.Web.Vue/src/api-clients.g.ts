import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as qs from 'qs'
import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'
import { AxiosPromise, AxiosResponse, AxiosRequestConfig } from 'axios'

export class CaseApiClient extends ModelApiClient<$models.Case> {
  constructor() { super($metadata.Case) }
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
  
  public uploadAttachment(id: number, file: File | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.uploadAttachment
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
  public asyncMethodOnIClassDto(id: number, input: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string>> {
    const $method = this.$metadata.methods.asyncMethodOnIClassDto
    const $params =  {
      id,
      input,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class CaseProductApiClient extends ModelApiClient<$models.CaseProduct> {
  constructor() { super($metadata.CaseProduct) }
}


export class CompanyApiClient extends ModelApiClient<$models.Company> {
  constructor() { super($metadata.Company) }
  public getCertainItems(isDeleted: boolean | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Company[]>> {
    const $method = this.$metadata.methods.getCertainItems
    const $params =  {
      isDeleted,
    }
    return this.$invoke($method, $params, $config)
  }
  
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
  
  public personCount(lastNameStartsWith: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<number>> {
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
  
  public removePersonById(id: number | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<boolean>> {
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
  
  public changeFirstName(id: number, firstName: string | null, title: $models.Titles | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.Person>> {
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
  
  public namesStartingWith(characters: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<string[]>> {
    const $method = this.$metadata.methods.namesStartingWith
    const $params =  {
      characters,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public searchPeople(criteria: $models.PersonCriteria | null, page: number | null, $config?: AxiosRequestConfig): AxiosPromise<ListResult<$models.Person>> {
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


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  public getWeather(location: $models.Location | null, dateTime: Date | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.WeatherData>> {
    const $method = this.$metadata.methods.getWeather
    const $params =  {
      location,
      dateTime,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public getWeatherAsync(location: $models.Location | null, dateTime: Date | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.WeatherData>> {
    const $method = this.$metadata.methods.getWeatherAsync
    const $params =  {
      location,
      dateTime,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


