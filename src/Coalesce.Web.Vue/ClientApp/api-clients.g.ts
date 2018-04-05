import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as qs from 'qs'
import * as $isValid from 'date-fns/isValid'
import * as $format from 'date-fns/format'
import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'
import { AxiosResponse, AxiosRequestConfig } from 'axios'

export class PersonApiClient extends ModelApiClient<typeof $metadata.Person, $models.Person> {
  constructor() { super($metadata.Person) }
  public rename(id: number, name: string | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.rename, {
      id,
      name,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/Rename`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Person>>>(r => this.$hydrateItemResult(r, $metadata.Person))
  }
  
  public changeSpacesToDashesInName(id: number, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.changeSpacesToDashesInName, {
      id,
    })
    return AxiosClient
      .post<ItemResult<void>>(
        `/${this.$metadata.controllerRoute}/ChangeSpacesToDashesInName`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public add(numberOne: number | null, numberTwo: number | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.add, {
      numberOne,
      numberTwo,
    })
    return AxiosClient
      .post<ItemResult<number>>(
        `/${this.$metadata.controllerRoute}/Add`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public getUser($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getUser, {
    })
    return AxiosClient
      .post<ItemResult<string>>(
        `/${this.$metadata.controllerRoute}/GetUser`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public personCount(lastNameStartsWith: string | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.personCount, {
      lastNameStartsWith,
    })
    return AxiosClient
      .get<ItemResult<number>>(
        `/${this.$metadata.controllerRoute}/PersonCount`,
        this.$options(undefined, $config, $params)
      )
  }
  
  public fullNameAndAge(id: number, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.fullNameAndAge, {
      id,
    })
    return AxiosClient
      .get<ItemResult<string>>(
        `/${this.$metadata.controllerRoute}/FullNameAndAge`,
        this.$options(undefined, $config, $params)
      )
  }
  
  public removePersonById(id: number | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.removePersonById, {
      id,
    })
    return AxiosClient
      .delete<ItemResult<boolean>>(
        `/${this.$metadata.controllerRoute}/RemovePersonById`,
        this.$options(undefined, $config, $params)
      )
  }
  
  public obfuscateEmail(id: number, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.obfuscateEmail, {
      id,
    })
    return AxiosClient
      .put<ItemResult<string>>(
        `/${this.$metadata.controllerRoute}/ObfuscateEmail`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public changeFirstName(id: number, firstName: string | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.changeFirstName, {
      id,
      firstName,
    })
    return AxiosClient
      .patch(
        `/${this.$metadata.controllerRoute}/ChangeFirstName`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Person>>>(r => this.$hydrateItemResult(r, $metadata.Person))
  }
  
  public getUserPublic($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getUserPublic, {
    })
    return AxiosClient
      .post<ItemResult<string>>(
        `/${this.$metadata.controllerRoute}/GetUserPublic`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public namesStartingWith(characters: string | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.namesStartingWith, {
      characters,
    })
    return AxiosClient
      .post<ItemResult<string[]>>(
        `/${this.$metadata.controllerRoute}/NamesStartingWith`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public searchPeople(criteria: $models.PersonCriteria | null, page: number | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.searchPeople, {
      criteria,
      page,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/SearchPeople`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ListResult<$models.Person>>>(r => this.$hydrateListResult(r, $metadata.Person))
  }
  
}


export class CaseApiClient extends ModelApiClient<typeof $metadata.Case, $models.Case> {
  constructor() { super($metadata.Case) }
  public getSomeCases($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getSomeCases, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetSomeCases`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Case[]>>>(r => this.$hydrateItemResult(r, $metadata.Case))
  }
  
  public getAllOpenCasesCount($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getAllOpenCasesCount, {
    })
    return AxiosClient
      .post<ItemResult<number>>(
        `/${this.$metadata.controllerRoute}/GetAllOpenCasesCount`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public randomizeDatesAndStatus($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.randomizeDatesAndStatus, {
    })
    return AxiosClient
      .post<ItemResult<void>>(
        `/${this.$metadata.controllerRoute}/RandomizeDatesAndStatus`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
  }
  
  public getCaseSummary($config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getCaseSummary, {
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCaseSummary`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.CaseSummary>>>(r => this.$hydrateItemResult(r, $metadata.CaseSummary))
  }
  
}


export class CompanyApiClient extends ModelApiClient<typeof $metadata.Company, $models.Company> {
  constructor() { super($metadata.Company) }
  public getCertainItems(isDeleted: boolean | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getCertainItems, {
      isDeleted,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCertainItems`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.Company[]>>>(r => this.$hydrateItemResult(r, $metadata.Company))
  }
  
}


export class ProductApiClient extends ModelApiClient<typeof $metadata.Product, $models.Product> {
  constructor() { super($metadata.Product) }
}


export class CaseProductApiClient extends ModelApiClient<typeof $metadata.CaseProduct, $models.CaseProduct> {
  constructor() { super($metadata.CaseProduct) }
}


export class WeatherServiceApiClient extends ServiceApiClient<typeof $metadata.WeatherService> {
  constructor() { super($metadata.WeatherService) }
  public getWeather(location: $models.Location | null, dateTime: Date | null, $config?: AxiosRequestConfig) {
    const $params = this.$mapParams(this.$metadata.methods.getWeather, {
      location,
      dateTime,
    })
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetWeather`,
        qs.stringify($params),
        this.$options(undefined, $config)
      )
      .then<AxiosResponse<ItemResult<$models.WeatherData>>>(r => this.$hydrateItemResult(r, $metadata.WeatherData))
  }
  
}


