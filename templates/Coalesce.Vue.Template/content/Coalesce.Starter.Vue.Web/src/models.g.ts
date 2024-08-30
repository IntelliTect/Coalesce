import * as metadata from './metadata.g'
import { Model, DataSource, convertToModel, mapToModel, reactiveDataSource } from 'coalesce-vue/lib/model'

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


export interface AppRole extends Model<typeof metadata.AppRole> {
  name: string | null
  roleClaims: AppRoleClaim[] | null
  permissions: Permission[] | null
  id: string | null
}
export class AppRole {
  
  /** Mutates the input object and its descendents into a valid AppRole implementation. */
  static convert(data?: Partial<AppRole>): AppRole {
    return convertToModel(data || {}, metadata.AppRole) 
  }
  
  /** Maps the input object and its descendents to a new, valid AppRole implementation. */
  static map(data?: Partial<AppRole>): AppRole {
    return mapToModel(data || {}, metadata.AppRole) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AppRole; }
  
  /** Instantiate a new AppRole, optionally basing it on the given data. */
  constructor(data?: Partial<AppRole> | {[k: string]: any}) {
    Object.assign(this, AppRole.map(data || {}));
  }
}


export interface AppRoleClaim extends Model<typeof metadata.AppRoleClaim> {
  role: AppRole | null
  id: number | null
  roleId: string | null
  claimType: string | null
  claimValue: string | null
}
export class AppRoleClaim {
  
  /** Mutates the input object and its descendents into a valid AppRoleClaim implementation. */
  static convert(data?: Partial<AppRoleClaim>): AppRoleClaim {
    return convertToModel(data || {}, metadata.AppRoleClaim) 
  }
  
  /** Maps the input object and its descendents to a new, valid AppRoleClaim implementation. */
  static map(data?: Partial<AppRoleClaim>): AppRoleClaim {
    return mapToModel(data || {}, metadata.AppRoleClaim) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AppRoleClaim; }
  
  /** Instantiate a new AppRoleClaim, optionally basing it on the given data. */
  constructor(data?: Partial<AppRoleClaim> | {[k: string]: any}) {
    Object.assign(this, AppRoleClaim.map(data || {}));
  }
}


export interface AppUser extends Model<typeof metadata.AppUser> {
  userName: string | null
  accessFailedCount: number | null
  lockoutEnd: Date | null
  lockoutEnabled: boolean | null
  userRoles: AppUserRole[] | null
  id: string | null
}
export class AppUser {
  
  /** Mutates the input object and its descendents into a valid AppUser implementation. */
  static convert(data?: Partial<AppUser>): AppUser {
    return convertToModel(data || {}, metadata.AppUser) 
  }
  
  /** Maps the input object and its descendents to a new, valid AppUser implementation. */
  static map(data?: Partial<AppUser>): AppUser {
    return mapToModel(data || {}, metadata.AppUser) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AppUser; }
  
  /** Instantiate a new AppUser, optionally basing it on the given data. */
  constructor(data?: Partial<AppUser> | {[k: string]: any}) {
    Object.assign(this, AppUser.map(data || {}));
  }
}


export interface AppUserRole extends Model<typeof metadata.AppUserRole> {
  id: string | null
  user: AppUser | null
  role: AppRole | null
  userId: string | null
  roleId: string | null
}
export class AppUserRole {
  
  /** Mutates the input object and its descendents into a valid AppUserRole implementation. */
  static convert(data?: Partial<AppUserRole>): AppUserRole {
    return convertToModel(data || {}, metadata.AppUserRole) 
  }
  
  /** Maps the input object and its descendents to a new, valid AppUserRole implementation. */
  static map(data?: Partial<AppUserRole>): AppUserRole {
    return mapToModel(data || {}, metadata.AppUserRole) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AppUserRole; }
  
  /** Instantiate a new AppUserRole, optionally basing it on the given data. */
  constructor(data?: Partial<AppUserRole> | {[k: string]: any}) {
    Object.assign(this, AppUserRole.map(data || {}));
  }
}
export namespace AppUserRole {
  export namespace DataSources {
    
    export class DefaultSource implements DataSource<typeof metadata.AppUserRole.dataSources.defaultSource> {
      readonly $metadata = metadata.AppUserRole.dataSources.defaultSource
    }
  }
}


export interface AuditLog extends Model<typeof metadata.AuditLog> {
  userId: string | null
  user: AppUser | null
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
  
  /** Mutates the input object and its descendents into a valid AuditLog implementation. */
  static convert(data?: Partial<AuditLog>): AuditLog {
    return convertToModel(data || {}, metadata.AuditLog) 
  }
  
  /** Maps the input object and its descendents to a new, valid AuditLog implementation. */
  static map(data?: Partial<AuditLog>): AuditLog {
    return mapToModel(data || {}, metadata.AuditLog) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLog; }
  
  /** Instantiate a new AuditLog, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLog> | {[k: string]: any}) {
    Object.assign(this, AuditLog.map(data || {}));
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
  
  /** Mutates the input object and its descendents into a valid AuditLogProperty implementation. */
  static convert(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return convertToModel(data || {}, metadata.AuditLogProperty) 
  }
  
  /** Maps the input object and its descendents to a new, valid AuditLogProperty implementation. */
  static map(data?: Partial<AuditLogProperty>): AuditLogProperty {
    return mapToModel(data || {}, metadata.AuditLogProperty) 
  }
  
  static [Symbol.hasInstance](x: any) { return x?.$metadata === metadata.AuditLogProperty; }
  
  /** Instantiate a new AuditLogProperty, optionally basing it on the given data. */
  constructor(data?: Partial<AuditLogProperty> | {[k: string]: any}) {
    Object.assign(this, AuditLogProperty.map(data || {}));
  }
}


export interface UserInfo extends Model<typeof metadata.UserInfo> {
  id: string | null
  userName: string | null
  roles: string[] | null
  permissions: string[] | null
}
export class UserInfo {
  
  /** Mutates the input object and its descendents into a valid UserInfo implementation. */
  static convert(data?: Partial<UserInfo>): UserInfo {
    return convertToModel(data || {}, metadata.UserInfo) 
  }
  
  /** Maps the input object and its descendents to a new, valid UserInfo implementation. */
  static map(data?: Partial<UserInfo>): UserInfo {
    return mapToModel(data || {}, metadata.UserInfo) 
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
  }
  interface ModelTypeLookup {
    AppRole: AppRole
    AppRoleClaim: AppRoleClaim
    AppUser: AppUser
    AppUserRole: AppUserRole
    AuditLog: AuditLog
    AuditLogProperty: AuditLogProperty
    UserInfo: UserInfo
  }
}
