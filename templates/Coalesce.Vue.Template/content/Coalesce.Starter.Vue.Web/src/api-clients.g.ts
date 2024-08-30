import * as $metadata from './metadata.g'
import * as $models from './models.g'
import { AxiosPromise, AxiosRequestConfig, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'

export class AppRoleApiClient extends ModelApiClient<$models.AppRole> {
  constructor() { super($metadata.AppRole) }
}


export class AppRoleClaimApiClient extends ModelApiClient<$models.AppRoleClaim> {
  constructor() { super($metadata.AppRoleClaim) }
}


export class AppUserApiClient extends ModelApiClient<$models.AppUser> {
  constructor() { super($metadata.AppUser) }
}


export class AppUserRoleApiClient extends ModelApiClient<$models.AppUserRole> {
  constructor() { super($metadata.AppUserRole) }
}


export class AuditLogApiClient extends ModelApiClient<$models.AuditLog> {
  constructor() { super($metadata.AuditLog) }
}


export class AuditLogPropertyApiClient extends ModelApiClient<$models.AuditLogProperty> {
  constructor() { super($metadata.AuditLogProperty) }
}


export class SecurityServiceApiClient extends ServiceApiClient<typeof $metadata.SecurityService> {
  constructor() { super($metadata.SecurityService) }
  public whoAmI($config?: AxiosRequestConfig): AxiosPromise<ItemResult<$models.UserInfo>> {
    const $method = this.$metadata.methods.whoAmI
    const $params =  {
    }
    return this.$invoke($method, $params, $config)
  }
  
}


