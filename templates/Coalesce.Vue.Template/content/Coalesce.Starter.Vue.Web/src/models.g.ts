import * as metadata from './metadata.g'
import { convertToModel, mapToModel, reactiveDataSource } from 'coalesce-vue/lib/model'
import type { Model, DataSource } from 'coalesce-vue/lib/model'
import type { ClassType } from 'coalesce-vue/lib/metadata'

export enum AuditEntryState {
  EntityAdded = 0,
  EntityDeleted = 1,
  EntityModified = 2,
}


export enum Permission {
  
  /** Modify application configuration and other administrative functions excluding user/role management. */
  Admin = 1,
  
  /** Add and modify users accounts and their assigned roles. Edit roles and their permissions. */
  UserAdmin = 2,
  ViewAuditLogs = 3,
}


export enum WidgetCategory {
  Whizbangs = 0,
  Sprecklesprockets = 1,
  Discombobulators = 2,
}


export interface AuditLog extends Model<typeof metadata.AuditLog> {
  userId: string | null
  user: User | null
  id: number | null
  type: string | null
  keyValue: string | null
  description: string | null
  state: AuditEntryState | null
  date: Date | null
  properties: AuditLogProperty[] | null
  clientIp: string | null
  referrer: string | null
  endpoint: string | null
}
export class AuditLog {
  
  /** Mutates the input object and its descendants into a valid AuditLog implementation. */
  static convert(data?: Partial<AuditLog>): AuditLog {
    return convertToModel<AuditLog>(data || {}, metadata.AuditLog) 
  }
  
  /** Maps the input object and its descendants to a new, valid AuditLog implementation. */
  static map(data?: Partial<AuditLog>): AuditLog {
    return mapToModel<AuditLog>(data || {}, metadata.AuditLog) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLog; }
  
  /** Instantiate a new AuditLog, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLog> | {[k: string]: any}) {
    Object.assign(this, AuditLog.map(data || {}));
  }
}
export namespace AuditLog {
  export namespace DataSources {
    
    export class TenantedDataSource implements DataSource<typeof metadata.AuditLog.dataSources.tenantedDataSource> {
      readonly $metadata = metadata.AuditLog.dataSources.tenantedDataSource
    }
  }
}


export interface AuditLogProperty extends Model<typeof metadata.AuditLogProperty> {
  id: number | null
  parentId: number | null
  propertyName: string | null
  oldValue: string | null
  oldValueDescription: string | null
  newValue: string | null
  newValueDescription: string | null
}
export class AuditLogProperty {
  
  /** Mutates the input object and its descendants into a valid AuditLogProperty implementation. */
  static convert(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return convertToModel<AuditLogProperty>(data || {}, metadata.AuditLogProperty) 
  }
  
  /** Maps the input object and its descendants to a new, valid AuditLogProperty implementation. */
  static map(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return mapToModel<AuditLogProperty>(data || {}, metadata.AuditLogProperty) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLogProperty; }
  
  /** Instantiate a new AuditLogProperty, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLogProperty> | {[k: string]: any}) {
    Object.assign(this, AuditLogProperty.map(data || {}));
  }
}


export interface Role extends Model<typeof metadata.Role> {
  name: string | null
  permissions: Permission[] | null
  id: string | null
}
export class Role {
  
  /** Mutates the input object and its descendants into a valid Role implementation. */
  static convert(data?: Partial<Role>): Role {
    return convertToModel<Role>(data || {}, metadata.Role) 
  }
  
  /** Maps the input object and its descendants to a new, valid Role implementation. */
  static map(data?: Partial<Role>): Role {
    return mapToModel<Role>(data || {}, metadata.Role) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Role; }
  
  /** Instantiate a new Role, optionally basing it on the given data. */
  constructor(data?: Partial<Role> | {[k: string]: any}) {
    Object.assign(this, Role.map(data || {}));
  }
}


export interface Tenant extends Model<typeof metadata.Tenant> {
  tenantId: string | null
  name: string | null
  
  /** The external origin of this tenant. Other users who sign in with accounts from this external source will automatically join this organization. */
  externalId: string | null
}
export class Tenant {
  
  /** Mutates the input object and its descendants into a valid Tenant implementation. */
  static convert(data?: Partial<Tenant>): Tenant {
    return convertToModel<Tenant>(data || {}, metadata.Tenant) 
  }
  
  /** Maps the input object and its descendants to a new, valid Tenant implementation. */
  static map(data?: Partial<Tenant>): Tenant {
    return mapToModel<Tenant>(data || {}, metadata.Tenant) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Tenant; }
  
  /** Instantiate a new Tenant, optionally basing it on the given data. */
  constructor(data?: Partial<Tenant> | {[k: string]: any}) {
    Object.assign(this, Tenant.map(data || {}));
  }
}
export namespace Tenant {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.Tenant.dataSources.defaultSource> {
      readonly $metadata = metadata.Tenant.dataSources.defaultSource
    }
    
    export class GlobalAdminSource implements DataSource<typeof metadata.Tenant.dataSources.globalAdminSource> {
      readonly $metadata = metadata.Tenant.dataSources.globalAdminSource
    }
  }
}


export interface User extends Model<typeof metadata.User> {
  fullName: string | null
  userName: string | null
  email: string | null
  emailConfirmed: boolean | null
  photoHash: string | null
  userRoles: UserRole[] | null
  roleNames: string[] | null
  
  /** Global admins can perform some administrative actions against ALL tenants. */
  isGlobalAdmin: boolean | null
  id: string | null
}
export class User {
  
  /** Mutates the input object and its descendants into a valid User implementation. */
  static convert(data?: Partial<User>): User {
    return convertToModel<User>(data || {}, metadata.User) 
  }
  
  /** Maps the input object and its descendants to a new, valid User implementation. */
  static map(data?: Partial<User>): User {
    return mapToModel<User>(data || {}, metadata.User) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.User; }
  
  /** Instantiate a new User, optionally basing it on the given data. */
  constructor(data?: Partial<User> | {[k: string]: any}) {
    Object.assign(this, User.map(data || {}));
  }
}
export namespace User {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.User.dataSources.defaultSource> {
      readonly $metadata = metadata.User.dataSources.defaultSource
    }
  }
}


export interface UserRole extends Model<typeof metadata.UserRole> {
  id: string | null
  user: User | null
  role: Role | null
  userId: string | null
  roleId: string | null
}
export class UserRole {
  
  /** Mutates the input object and its descendants into a valid UserRole implementation. */
  static convert(data?: Partial<UserRole>): UserRole {
    return convertToModel<UserRole>(data || {}, metadata.UserRole) 
  }
  
  /** Maps the input object and its descendants to a new, valid UserRole implementation. */
  static map(data?: Partial<UserRole>): UserRole {
    return mapToModel<UserRole>(data || {}, metadata.UserRole) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.UserRole; }
  
  /** Instantiate a new UserRole, optionally basing it on the given data. */
  constructor(data?: Partial<UserRole> | {[k: string]: any}) {
    Object.assign(this, UserRole.map(data || {}));
  }
}
export namespace UserRole {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.UserRole.dataSources.defaultSource> {
      readonly $metadata = metadata.UserRole.dataSources.defaultSource
    }
  }
}


export interface Widget extends Model<typeof metadata.Widget> {
  widgetId: number | null
  name: string | null
  category: WidgetCategory | null
  inventedOn: Date | null
  modifiedBy: User | null
  modifiedById: string | null
  modifiedOn: Date | null
  createdBy: User | null
  createdById: string | null
  createdOn: Date | null
}
export class Widget {
  
  /** Mutates the input object and its descendants into a valid Widget implementation. */
  static convert(data?: Partial<Widget>): Widget {
    return convertToModel<Widget>(data || {}, metadata.Widget) 
  }
  
  /** Maps the input object and its descendants to a new, valid Widget implementation. */
  static map(data?: Partial<Widget>): Widget {
    return mapToModel<Widget>(data || {}, metadata.Widget) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.Widget; }
  
  /** Instantiate a new Widget, optionally basing it on the given data. */
  constructor(data?: Partial<Widget> | {[k: string]: any}) {
    Object.assign(this, Widget.map(data || {}));
  }
}


export interface UserInfo extends Model<typeof metadata.UserInfo> {
  id: string | null
  userName: string | null
  email: string | null
  fullName: string | null
  roles: string[] | null
  permissions: string[] | null
  tenantId: string | null
  tenantName: string | null
}
export class UserInfo {
  
  /** Mutates the input object and its descendants into a valid UserInfo implementation. */
  static convert(data?: Partial<UserInfo>): UserInfo {
    return convertToModel<UserInfo>(data || {}, metadata.UserInfo) 
  }
  
  /** Maps the input object and its descendants to a new, valid UserInfo implementation. */
  static map(data?: Partial<UserInfo>): UserInfo {
    return mapToModel<UserInfo>(data || {}, metadata.UserInfo) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.UserInfo; }
  
  /** Instantiate a new UserInfo, optionally basing it on the given data. */
  constructor(data?: Partial<UserInfo> | {[k: string]: any}) {
    Object.assign(this, UserInfo.map(data || {}));
  }
}


declare module "coalesce-vue/lib/model" {
  interface EnumTypeLookup {
    AuditEntryState: AuditEntryState
    Permission: Permission
    WidgetCategory: WidgetCategory
  }
  interface ModelTypeLookup {
    AuditLog: AuditLog
    AuditLogProperty: AuditLogProperty
    Role: Role
    Tenant: Tenant
    User: User
    UserInfo: UserInfo
    UserRole: UserRole
    Widget: Widget
  }
}
