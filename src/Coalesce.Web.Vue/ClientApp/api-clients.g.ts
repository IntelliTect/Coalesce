import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as qs from 'qs'
import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'
import { AxiosResponse, AxiosRequestConfig } from 'axios'

export class CaseApiClient extends ModelApiClient<$models.Case> {
  constructor() { super($metadata.Case) }
  public getSomeCases($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getSomeCases
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetSomeCases`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Case[]>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public getAllOpenCasesCount($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getAllOpenCasesCount
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetAllOpenCasesCount`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<number>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public randomizeDatesAndStatus($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.randomizeDatesAndStatus
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/RandomizeDatesAndStatus`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<void>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public getCaseSummary($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getCaseSummary
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCaseSummary`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.CaseSummary>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
}


export class CaseProductApiClient extends ModelApiClient<$models.CaseProduct> {
  constructor() { super($metadata.CaseProduct) }
}


export class CompanyApiClient extends ModelApiClient<$models.Company> {
  constructor() { super($metadata.Company) }
  public getCertainItems(isDeleted: boolean | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getCertainItems
    const $params = this.$mapParams($method, {
      isDeleted,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCertainItems`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Company[]>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
}


export class PersonApiClient extends ModelApiClient<$models.Person> {
  constructor() { super($metadata.Person) }
  public rename(id: number, name: string | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.rename
    const $params = this.$mapParams($method, {
      id,
      name,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/Rename`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Person>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public changeSpacesToDashesInName(id: number, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.changeSpacesToDashesInName
    const $params = this.$mapParams($method, {
      id,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/ChangeSpacesToDashesInName`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<void>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public add(numberOne: number | null, numberTwo: number | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.add
    const $params = this.$mapParams($method, {
      numberOne,
      numberTwo,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/Add`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<number>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public getUser($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getUser
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetUser`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<string>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public personCount(lastNameStartsWith: string | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.personCount
    const $params = this.$mapParams($method, {
      lastNameStartsWith,
    })
    return AxiosClient
      .get(
        `/${this.$metadata.controllerRoute}/PersonCount`,
        this.$options(undefined, $config, $params)
      )
      .then<AxiosResponse<ItemResult<number>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public fullNameAndAge(id: number, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.fullNameAndAge
    const $params = this.$mapParams($method, {
      id,
    })
    return AxiosClient
      .get(
        `/${this.$metadata.controllerRoute}/FullNameAndAge`,
        this.$options(undefined, $config, $params)
      )
      .then<AxiosResponse<ItemResult<string>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public removePersonById(id: number | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.removePersonById
    const $params = this.$mapParams($method, {
      id,
    })
    return AxiosClient
      .delete(
        `/${this.$metadata.controllerRoute}/RemovePersonById`,
        this.$options(undefined, $config, $params)
      )
      .then<AxiosResponse<ItemResult<boolean>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public obfuscateEmail(id: number, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.obfuscateEmail
    const $params = this.$mapParams($method, {
      id,
    })
    return AxiosClient
      .put(
        `/${this.$metadata.controllerRoute}/ObfuscateEmail`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<string>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public changeFirstName(id: number, firstName: string | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.changeFirstName
    const $params = this.$mapParams($method, {
      id,
      firstName,
    })
    return AxiosClient
      .patch(
        `/${this.$metadata.controllerRoute}/ChangeFirstName`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Person>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public getUserPublic($config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getUserPublic
    const $params = this.$mapParams($method, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetUserPublic`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<string>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public namesStartingWith(characters: string | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.namesStartingWith
    const $params = this.$mapParams($method, {
      characters,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/NamesStartingWith`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<string[]>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public searchPeople(criteria: $models.PersonCriteria | null, page: number | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.searchPeople
    const $params = this.$mapParams($method, {
      criteria,
      page,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/SearchPeople`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ListResult<$models.Person>>>(r => this.$hydrateListResult(r, $method.return))
  }
  
}


export class ProductApiClient extends ModelApiClient<$models.Product> {
  constructor() { super($metadata.Product) }
}


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  public getWeather(location: $models.Location | null, dateTime: Date | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getWeather
    const $params = this.$mapParams($method, {
      location,
      dateTime,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetWeather`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.WeatherData>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
  public getWeatherAsync(location: $models.Location | null, dateTime: Date | null, $config?: AxiosRequestConfig) {
    const $method = this.$metadata.methods.getWeatherAsync
    const $params = this.$mapParams($method, {
      location,
      dateTime,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetWeatherAsync`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.WeatherData>>>(r => this.$hydrateItemResult(r, $method.return))
  }
  
}


