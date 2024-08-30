import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ViewModelCollection, ServiceViewModel, DeepPartial, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface AppRoleViewModel extends $models.AppRole {
  name: string | null;
  get roleClaims(): ViewModelCollection<AppRoleClaimViewModel, $models.AppRoleClaim>;
  set roleClaims(value: (AppRoleClaimViewModel | $models.AppRoleClaim)[] | null);
  permissions: $models.Permission[] | null;
  id: string | null;
}
export class AppRoleViewModel extends ViewModel<$models.AppRole, $apiClients.AppRoleApiClient, string> implements $models.AppRole  {
  
  
  public addToRoleClaims(initialData?: DeepPartial<$models.AppRoleClaim> | null) {
    return this.$addChild('roleClaims', initialData) as AppRoleClaimViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.AppRole> | null) {
    super($metadata.AppRole, new $apiClients.AppRoleApiClient(), initialData)
  }
}
defineProps(AppRoleViewModel, $metadata.AppRole)

export class AppRoleListViewModel extends ListViewModel<$models.AppRole, $apiClients.AppRoleApiClient, AppRoleViewModel> {
  
  constructor() {
    super($metadata.AppRole, new $apiClients.AppRoleApiClient())
  }
}


export interface AppRoleClaimViewModel extends $models.AppRoleClaim {
  get role(): AppRoleViewModel | null;
  set role(value: AppRoleViewModel | $models.AppRole | null);
  id: number | null;
  roleId: string | null;
  claimType: string | null;
  claimValue: string | null;
}
export class AppRoleClaimViewModel extends ViewModel<$models.AppRoleClaim, $apiClients.AppRoleClaimApiClient, number> implements $models.AppRoleClaim  {
  
  constructor(initialData?: DeepPartial<$models.AppRoleClaim> | null) {
    super($metadata.AppRoleClaim, new $apiClients.AppRoleClaimApiClient(), initialData)
  }
}
defineProps(AppRoleClaimViewModel, $metadata.AppRoleClaim)

export class AppRoleClaimListViewModel extends ListViewModel<$models.AppRoleClaim, $apiClients.AppRoleClaimApiClient, AppRoleClaimViewModel> {
  
  constructor() {
    super($metadata.AppRoleClaim, new $apiClients.AppRoleClaimApiClient())
  }
}


export interface AppUserViewModel extends $models.AppUser {
  userName: string | null;
  accessFailedCount: number | null;
  lockoutEnd: Date | null;
  lockoutEnabled: boolean | null;
  get userRoles(): ViewModelCollection<AppUserRoleViewModel, $models.AppUserRole>;
  set userRoles(value: (AppUserRoleViewModel | $models.AppUserRole)[] | null);
  id: string | null;
}
export class AppUserViewModel extends ViewModel<$models.AppUser, $apiClients.AppUserApiClient, string> implements $models.AppUser  {
  
  
  public addToUserRoles(initialData?: DeepPartial<$models.AppUserRole> | null) {
    return this.$addChild('userRoles', initialData) as AppUserRoleViewModel
  }
  
  get roles(): ReadonlyArray<AppRoleViewModel> {
    return (this.userRoles || []).map($ => $.role!).filter($ => $)
  }
  
  constructor(initialData?: DeepPartial<$models.AppUser> | null) {
    super($metadata.AppUser, new $apiClients.AppUserApiClient(), initialData)
  }
}
defineProps(AppUserViewModel, $metadata.AppUser)

export class AppUserListViewModel extends ListViewModel<$models.AppUser, $apiClients.AppUserApiClient, AppUserViewModel> {
  
  constructor() {
    super($metadata.AppUser, new $apiClients.AppUserApiClient())
  }
}


export interface AppUserRoleViewModel extends $models.AppUserRole {
  id: string | null;
  get user(): AppUserViewModel | null;
  set user(value: AppUserViewModel | $models.AppUser | null);
  get role(): AppRoleViewModel | null;
  set role(value: AppRoleViewModel | $models.AppRole | null);
  userId: string | null;
  roleId: string | null;
}
export class AppUserRoleViewModel extends ViewModel<$models.AppUserRole, $apiClients.AppUserRoleApiClient, string> implements $models.AppUserRole  {
  static DataSources = $models.AppUserRole.DataSources;
  
  constructor(initialData?: DeepPartial<$models.AppUserRole> | null) {
    super($metadata.AppUserRole, new $apiClients.AppUserRoleApiClient(), initialData)
  }
}
defineProps(AppUserRoleViewModel, $metadata.AppUserRole)

export class AppUserRoleListViewModel extends ListViewModel<$models.AppUserRole, $apiClients.AppUserRoleApiClient, AppUserRoleViewModel> {
  static DataSources = $models.AppUserRole.DataSources;
  
  constructor() {
    super($metadata.AppUserRole, new $apiClients.AppUserRoleApiClient())
  }
}


export interface AuditLogViewModel extends $models.AuditLog {
  userId: string | null;
  get user(): AppUserViewModel | null;
  set user(value: AppUserViewModel | $models.AppUser | null);
  id: number | null;
  type: string | null;
  keyValue: string | null;
  description: string | null;
  state: $models.AuditEntryState | null;
  date: Date | null;
  get properties(): ViewModelCollection<AuditLogPropertyViewModel, $models.AuditLogProperty>;
  set properties(value: (AuditLogPropertyViewModel | $models.AuditLogProperty)[] | null);
  clientIp: string | null;
  referrer: string | null;
  endpoint: string | null;
}
export class AuditLogViewModel extends ViewModel<$models.AuditLog, $apiClients.AuditLogApiClient, number> implements $models.AuditLog  {
  
  
  public addToProperties(initialData?: DeepPartial<$models.AuditLogProperty> | null) {
    return this.$addChild('properties', initialData) as AuditLogPropertyViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.AuditLog> | null) {
    super($metadata.AuditLog, new $apiClients.AuditLogApiClient(), initialData)
  }
}
defineProps(AuditLogViewModel, $metadata.AuditLog)

export class AuditLogListViewModel extends ListViewModel<$models.AuditLog, $apiClients.AuditLogApiClient, AuditLogViewModel> {
  
  constructor() {
    super($metadata.AuditLog, new $apiClients.AuditLogApiClient())
  }
}


export interface AuditLogPropertyViewModel extends $models.AuditLogProperty {
  id: number | null;
  parentId: number | null;
  propertyName: string | null;
  oldValue: string | null;
  oldValueDescription: string | null;
  newValue: string | null;
  newValueDescription: string | null;
}
export class AuditLogPropertyViewModel extends ViewModel<$models.AuditLogProperty, $apiClients.AuditLogPropertyApiClient, number> implements $models.AuditLogProperty  {
  
  constructor(initialData?: DeepPartial<$models.AuditLogProperty> | null) {
    super($metadata.AuditLogProperty, new $apiClients.AuditLogPropertyApiClient(), initialData)
  }
}
defineProps(AuditLogPropertyViewModel, $metadata.AuditLogProperty)

export class AuditLogPropertyListViewModel extends ListViewModel<$models.AuditLogProperty, $apiClients.AuditLogPropertyApiClient, AuditLogPropertyViewModel> {
  
  constructor() {
    super($metadata.AuditLogProperty, new $apiClients.AuditLogPropertyApiClient())
  }
}


export class SecurityServiceViewModel extends ServiceViewModel<typeof $metadata.SecurityService, $apiClients.SecurityServiceApiClient> {
  
  public get whoAmI() {
    const whoAmI = this.$apiClient.$makeCaller(
      this.$metadata.methods.whoAmI,
      (c) => c.whoAmI(),
      () => ({}),
      (c, args) => c.whoAmI())
    
    Object.defineProperty(this, 'whoAmI', {value: whoAmI});
    return whoAmI
  }
  
  constructor() {
    super($metadata.SecurityService, new $apiClients.SecurityServiceApiClient())
  }
}


const viewModelTypeLookup = ViewModel.typeLookup = {
  AppRole: AppRoleViewModel,
  AppRoleClaim: AppRoleClaimViewModel,
  AppUser: AppUserViewModel,
  AppUserRole: AppUserRoleViewModel,
  AuditLog: AuditLogViewModel,
  AuditLogProperty: AuditLogPropertyViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  AppRole: AppRoleListViewModel,
  AppRoleClaim: AppRoleClaimListViewModel,
  AppUser: AppUserListViewModel,
  AppUserRole: AppUserRoleListViewModel,
  AuditLog: AuditLogListViewModel,
  AuditLogProperty: AuditLogPropertyListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  SecurityService: SecurityServiceViewModel,
}

