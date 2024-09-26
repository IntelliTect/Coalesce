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
  static DataSources = $models.AuditLog.DataSources;
  
  
  public addToProperties(initialData?: DeepPartial<$models.AuditLogProperty> | null) {
    return this.$addChild('properties', initialData) as AuditLogPropertyViewModel
  }
  
  constructor(initialData?: DeepPartial<$models.AuditLog> | null) {
    super($metadata.AuditLog, new $apiClients.AuditLogApiClient(), initialData)
  }
}
defineProps(AuditLogViewModel, $metadata.AuditLog)

export class AuditLogListViewModel extends ListViewModel<$models.AuditLog, $apiClients.AuditLogApiClient, AuditLogViewModel> {
  static DataSources = $models.AuditLog.DataSources;
  
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
  permissions: $models.Permission[] | null;
  id: string | null;
}
export class RoleViewModel extends ViewModel<$models.Role, $apiClients.RoleApiClient, string> implements $models.Role  {
  
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


export interface TenantViewModel extends $models.Tenant {
  tenantId: string | null;
  name: string | null;
  
  /** The external origin of this tenant. Other users who sign in with accounts from this external source will automatically join this organization. */
  externalId: string | null;
}
export class TenantViewModel extends ViewModel<$models.Tenant, $apiClients.TenantApiClient, string> implements $models.Tenant  {
  static DataSources = $models.Tenant.DataSources;
  
  constructor(initialData?: DeepPartial<$models.Tenant> | null) {
    super($metadata.Tenant, new $apiClients.TenantApiClient(), initialData)
  }
}
defineProps(TenantViewModel, $metadata.Tenant)

export class TenantListViewModel extends ListViewModel<$models.Tenant, $apiClients.TenantApiClient, TenantViewModel> {
  static DataSources = $models.Tenant.DataSources;
  
  public get create() {
    const create = this.$apiClient.$makeCaller(
      this.$metadata.methods.create,
      (c, name: string | null, adminEmail: string | null) => c.create(name, adminEmail),
      () => ({name: null as string | null, adminEmail: null as string | null, }),
      (c, args) => c.create(args.name, args.adminEmail))
    
    Object.defineProperty(this, 'create', {value: create});
    return create
  }
  
  constructor() {
    super($metadata.Tenant, new $apiClients.TenantApiClient())
  }
}


export interface UserViewModel extends $models.User {
  fullName: string | null;
  photoHash: string | null;
  userName: string | null;
  email: string | null;
  emailConfirmed: boolean | null;
  get userRoles(): ViewModelCollection<UserRoleViewModel, $models.UserRole>;
  set userRoles(value: (UserRoleViewModel | $models.UserRole)[] | null);
  roleNames: string[] | null;
  
  /** The user is a global administrator, able to perform administrative actions against all tenants. */
  isGlobalAdmin: boolean | null;
  id: string | null;
}
export class UserViewModel extends ViewModel<$models.User, $apiClients.UserApiClient, string> implements $models.User  {
  static DataSources = $models.User.DataSources;
  
  
  public addToUserRoles(initialData?: DeepPartial<$models.UserRole> | null) {
    return this.$addChild('userRoles', initialData) as UserRoleViewModel
  }
  
  get roles(): ReadonlyArray<RoleViewModel> {
    return (this.userRoles || []).map($ => $.role!).filter($ => $)
  }
  
  public get getPhoto() {
    const getPhoto = this.$apiClient.$makeCaller(
      this.$metadata.methods.getPhoto,
      (c) => c.getPhoto(this.$primaryKey, this.photoHash),
      () => ({}),
      (c, args) => c.getPhoto(this.$primaryKey, this.photoHash))
    
    Object.defineProperty(this, 'getPhoto', {value: getPhoto});
    return getPhoto
  }
  
  public get evict() {
    const evict = this.$apiClient.$makeCaller(
      this.$metadata.methods.evict,
      (c) => c.evict(this.$primaryKey),
      () => ({}),
      (c, args) => c.evict(this.$primaryKey))
    
    Object.defineProperty(this, 'evict', {value: evict});
    return evict
  }
  
  constructor(initialData?: DeepPartial<$models.User> | null) {
    super($metadata.User, new $apiClients.UserApiClient(), initialData)
  }
}
defineProps(UserViewModel, $metadata.User)

export class UserListViewModel extends ListViewModel<$models.User, $apiClients.UserApiClient, UserViewModel> {
  static DataSources = $models.User.DataSources;
  
  public get inviteUser() {
    const inviteUser = this.$apiClient.$makeCaller(
      this.$metadata.methods.inviteUser,
      (c, email: string | null, role?: $models.Role | null) => c.inviteUser(email, role),
      () => ({email: null as string | null, role: null as $models.Role | null, }),
      (c, args) => c.inviteUser(args.email, args.role))
    
    Object.defineProperty(this, 'inviteUser', {value: inviteUser});
    return inviteUser
  }
  
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


export interface WidgetViewModel extends $models.Widget {
  widgetId: number | null;
  name: string | null;
  category: $models.WidgetCategory | null;
  inventedOn: Date | null;
  get modifiedBy(): UserViewModel | null;
  set modifiedBy(value: UserViewModel | $models.User | null);
  modifiedById: string | null;
  modifiedOn: Date | null;
  get createdBy(): UserViewModel | null;
  set createdBy(value: UserViewModel | $models.User | null);
  createdById: string | null;
  createdOn: Date | null;
}
export class WidgetViewModel extends ViewModel<$models.Widget, $apiClients.WidgetApiClient, number> implements $models.Widget  {
  
  constructor(initialData?: DeepPartial<$models.Widget> | null) {
    super($metadata.Widget, new $apiClients.WidgetApiClient(), initialData)
  }
}
defineProps(WidgetViewModel, $metadata.Widget)

export class WidgetListViewModel extends ListViewModel<$models.Widget, $apiClients.WidgetApiClient, WidgetViewModel> {
  
  constructor() {
    super($metadata.Widget, new $apiClients.WidgetApiClient())
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
  Tenant: TenantViewModel,
  User: UserViewModel,
  UserRole: UserRoleViewModel,
  Widget: WidgetViewModel,
}
const listViewModelTypeLookup = ListViewModel.typeLookup = {
  AuditLog: AuditLogListViewModel,
  AuditLogProperty: AuditLogPropertyListViewModel,
  Role: RoleListViewModel,
  Tenant: TenantListViewModel,
  User: UserListViewModel,
  UserRole: UserRoleListViewModel,
  Widget: WidgetListViewModel,
}
const serviceViewModelTypeLookup = ServiceViewModel.typeLookup = {
  SecurityService: SecurityServiceViewModel,
}

