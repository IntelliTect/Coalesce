import * as $metadata from './metadata.g'
import * as $models from './models.g'
import { AxiosPromise, AxiosRequestConfig, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'

export class AuditLogApiClient extends ModelApiClient<$models.AuditLog> {
  constructor() { super($metadata.AuditLog) }
}


export class AuditLogPropertyApiClient extends ModelApiClient<$models.AuditLogProperty> {
  constructor() { super($metadata.AuditLogProperty) }
}


export class RoleApiClient extends ModelApiClient<$models.Role> {
  constructor() { super($metadata.Role) }
}


export class TenantApiClient extends ModelApiClient<$models.Tenant> {
  constructor() { super($metadata.Tenant) }
  public create(name: string | null, adminEmail: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.create
    const $params =  {
      name,
      adminEmail,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class UserApiClient extends ModelApiClient<$models.User> {
  constructor() { super($metadata.User) }
  public getPhoto(id: string | null, etag?: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<File>> {
    const $method = this.$metadata.methods.getPhoto
    const $params =  {
      id,
      etag,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public evict(id: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.evict
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public inviteUser(email: string | null, role?: $models.Role | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.inviteUser
    const $params =  {
      email,
      role,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public setEmail(id: string | null, newEmail: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.setEmail
    const $params =  {
      id,
      newEmail,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public sendEmailConfirmation(id: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.sendEmailConfirmation
    const $params =  {
      id,
    }
    return this.$invoke($method, $params, $config)
  }
  
  public setPassword(id: string | null, currentPassword: string | null, newPassword: string | null, confirmNewPassword: string | null, $config?: AxiosRequestConfig): AxiosPromise<ItemResult<void>> {
    const $method = this.$metadata.methods.setPassword
    const $params =  {
      id,
      currentPassword,
      newPassword,
      confirmNewPassword,
    }
    return this.$invoke($method, $params, $config)
  }
  
}


export class UserRoleApiClient extends ModelApiClient<$models.UserRole> {
  constructor() { super($metadata.UserRole) }
}


export class WidgetApiClient extends ModelApiClient<$models.Widget> {
  constructor() { super($metadata.Widget) }
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


