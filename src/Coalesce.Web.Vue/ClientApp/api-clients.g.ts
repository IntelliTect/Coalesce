import * as models from './models.g'
import * as qs from 'qs'
import { mapToDto as $mapToDto } from 'coalesce-vue/lib/model'
import { AxiosClient, ApiClient } from 'coalesce-vue/lib/api-client'
import { AxiosRequestConfig } from 'axios'

export class PersonApiClient extends ApiClient<models.Person> {
  public rename(id: number, name: string | null, config?: AxiosRequestConfig) {
    const $params = { id: id, name: name }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/Rename`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public changeSpacesToDashesInName(id: number, config?: AxiosRequestConfig) {
    const $params = { id: id }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/ChangeSpacesToDashesInName`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public add(numberOne: number | null, numberTwo: number | null, config?: AxiosRequestConfig) {
    const $params = { numberOne: numberOne, numberTwo: numberTwo }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/Add`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public getUser(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetUser`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public personCount(lastNameStartsWith: string | null, config?: AxiosRequestConfig) {
    const $params = { lastNameStartsWith: lastNameStartsWith }
    return AxiosClient
      .get(
        `/${this.$metadata.controllerRoute}/PersonCount`,
        this.$options(undefined, config, $params)
      )
  }
  
  public fullNameAndAge(id: number, config?: AxiosRequestConfig) {
    const $params = { id: id }
    return AxiosClient
      .get(
        `/${this.$metadata.controllerRoute}/FullNameAndAge`,
        this.$options(undefined, config, $params)
      )
  }
  
  public removePersonById(id: number | null, config?: AxiosRequestConfig) {
    const $params = { id: id }
    return AxiosClient
      .delete(
        `/${this.$metadata.controllerRoute}/RemovePersonById`,
        this.$options(undefined, config, $params)
      )
  }
  
  public obfuscateEmail(id: number, config?: AxiosRequestConfig) {
    const $params = { id: id }
    return AxiosClient
      .put(
        `/${this.$metadata.controllerRoute}/ObfuscateEmail`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public changeFirstName(id: number, firstName: string | null, config?: AxiosRequestConfig) {
    const $params = { id: id, firstName: firstName }
    return AxiosClient
      .patch(
        `/${this.$metadata.controllerRoute}/ChangeFirstName`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public getUserPublic(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetUserPublic`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public namesStartingWith(characters: string | null, config?: AxiosRequestConfig) {
    const $params = { characters: characters }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/NamesStartingWith`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public searchPeople(criteria: models.PersonCriteria | null, page: number | null, config?: AxiosRequestConfig) {
    const $params = { criteria: $mapToDto(criteria), page: page }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/SearchPeople`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
}


export class CaseApiClient extends ApiClient<models.Case> {
  public getSomeCases(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetSomeCases`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public getAllOpenCasesCount(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetAllOpenCasesCount`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public randomizeDatesAndStatus(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/RandomizeDatesAndStatus`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
  public getCaseSummary(config?: AxiosRequestConfig) {
    const $params = {  }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCaseSummary`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
}


export class CompanyApiClient extends ApiClient<models.Company> {
  public getCertainItems(isDeleted: boolean | null, config?: AxiosRequestConfig) {
    const $params = { isDeleted: isDeleted }
    return AxiosClient
      .post(
        `/${this.$metadata.controllerRoute}/GetCertainItems`,
        qs.stringify($params),
        this.$options(undefined, config)
      )
  }
  
}


export class ProductApiClient extends ApiClient<models.Product> {
}


export class CaseProductApiClient extends ApiClient<models.CaseProduct> {
}


