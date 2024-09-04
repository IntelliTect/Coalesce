import * as $metadata from './metadata.g'
import * as $models from './models.g'
import * as $apiClients from './api-clients.g'
import { ViewModel, ListViewModel, ViewModelCollection, ServiceViewModel, DeepPartial, defineProps } from 'coalesce-vue/lib/viewmodel'

export interface AuditLogViewModel extends $models.AuditLog {
  userId: string | null;
  get user(): UserViewModel | null;
  set user(value: UserViewModel | $models.User | null);
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


export interface RoleViewModel extends $models.Role {
  name: string | null;
  get roleClaims(): ViewModelCollection<RoleClaimViewModel, $models.RoleClaim>;
  set roleClaims(value: (RoleClaimViewModel | $models.RoleClaim)[] | null);
  permissions: $models.Permission[] | null;
  id: string | null;
}
export class RoleViewModel extends ViewModel<$models.Role, $apiClients.RoleApiClient, string> implements $models.Role  {
  
  
  public addToRoleClaims(initialData?: DeepPartial<$models.RoleClaim> | null) {
    return this.$addChild('roleClaims', initialData) as RoleClaimViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.Role> | null) {
    super($metadata.Role, new $apiClients.RoleApiClient(), initialData)
  }
}
defineProps(RoleViewModel, $metadata.Role)

export class RoleListViewModel extends ListViewModel<$models.Role, $apiClients.RoleApiClient, RoleViewModel> {
  
  constructor() {
    super($metadata.Role, new $apiClients.RoleApiClient())
  }
}


export interface RoleClaimViewModel extends $models.RoleClaim {
  get role(): RoleViewModel | null;
  set role(value: RoleViewModel | $models.Role | null);
  id: number | null;
  roleId: string | null;
  claimType: string | null;
  claimValue: string | null;
}
export class RoleClaimViewModel extends ViewModel<$models.RoleClaim, $apiClients.RoleClaimApiClient, number> implements $models.RoleClaim  {
  
  constructor(initialData?: DeepPartial<$models.RoleClaim> | null) {
    super($metadata.RoleClaim, new $apiClients.RoleClaimApiClient(), initialData)
  }
}
defineProps(RoleClaimViewModel, $metadata.RoleClaim)

export class RoleClaimListViewModel extends ListViewModel<$models.RoleClaim, $apiClients.RoleClaimApiClient, RoleClaimViewModel> {
  
  constructor() {
    super($metadata.RoleClaim, new $apiClients.RoleClaimApiClient())
  }
}


export interface UserViewModel extends $models.User {
  fullName: string | null;
  photoMD5: string | null;
  userName: string | null;
  accessFailedCount: number | null;
  lockoutEnd: Date | null;
  lockoutEnabled: boolean | null;
  get userRoles(): ViewModelCollection<UserRoleViewModel, $models.UserRole>;
  set userRoles(value: (UserRoleViewModel | $models.UserRole)[] | null);
  id: string | null;
}
export class UserViewModel extends ViewModel<$models.User, $apiClients.UserApiClient, string> implements $models.User  {
  
  
  public addToUserRoles(initialData?: DeepPartial<$models.UserRole> | null) {
    return this.$addChild('userRoles', initialData) as UserRoleViewModel
  }
  
  get roles(): ReadonlyArray<RoleViewModel> {
    return (this.userRoles || []).map($ => $.role!).filter($ => $)
  }
  
  public get getPhoto() {
    const getPhoto = this.$apiClient.$makeCaller(
      this.$metadata.methods.getPhoto,
      (c) => c.getPhoto(this.$primaryKey, this.photoMD5),
      () => ({}),
      (c, args) => c.getPhoto(this.$primaryKey, this.photoMD5))
    
    Object.defineProperty(this, 'getPhoto', {value: getPhoto});
    return getPhoto
  }
  
  constructor(initialData?: DeepPartial<$models.User> | null) {
    super($metadata.User, new $apiClients.UserApiClient(), initialData)
  }
}
defineProps(UserViewModel, $metadata.User)

export class UserListViewModel extends ListViewModel<$models.User, $apiClients.UserApiClient, UserViewModel> {
  
  constructor() {
    super($metadata.User, new $apiClients.UserApiClient())
  }
}


export interface UserRoleViewModel extends $models.UserRole {
  id: string | null;
  get user(): UserViewModel | null;
  set user(value: UserViewModel | $models.User | null);
  get role(): RoleViewModel | null;
  set role(value: RoleViewModel | $models.Role | null);
  userId: string | null;
  roleId: string | null;
}
export class UserRoleViewModel extends ViewModel<$models.UserRole, $apiClients.UserRoleApiClient, string> implements $models.UserRole  {
  static DataSources = $models.UserRole.DataSources;
  
  constructor(initialData?: DeepPartial<$models.UserRole> | null) {
    super($metadata.UserRole, new $apiClients.UserRoleApiClient(), initialData)
  }
}
defineProps(UserRoleViewModel, $metadata.UserRole)

export class UserRoleListViewModel extends ListViewModel<$models.UserRole, $apiClients.UserRoleApiClient, UserRoleViewModel> {
  static DataSources = $models.UserRole.DataSources;
  
  constructor() {
    super($metadata.UserRole, new $apiClients.UserRoleApiClient())
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
  AuditLog: AuditLogViewModel,
  AuditLogProperty: AuditLogPropertyViewModel,
  Role: RoleViewModel,
  RoleClaim: RoleClaimViewModel,
  User: UserViewModel,
  UserRole: UserRoleViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  AuditLog: AuditLogListViewModel,
  AuditLogProperty: AuditLogPropertyListViewModel,
  Role: RoleListViewModel,
  RoleClaim: RoleClaimListViewModel,
  User: UserListViewModel,
  UserRole: UserRoleListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  SecurityService: SecurityServiceViewModel,
}

