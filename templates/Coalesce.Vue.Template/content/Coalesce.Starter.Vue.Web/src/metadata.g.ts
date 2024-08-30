import {
  Domain, getEnumMeta, solidify, ModelType, ObjectType,
  PrimitiveProperty, ForeignKeyProperty, PrimaryKeyProperty,
  ModelCollectionNavigationProperty, ModelReferenceNavigationProperty,
  HiddenAreas, BehaviorFlags
} from 'coalesce-vue/lib/metadata'


const domain: Domain = { enums: {}, types: {}, services: {} }
export const AuditEntryState = domain.enums.AuditEntryState = {
  name: "AuditEntryState" as const,
  displayName: "Audit Entry State",
  type: "enum",
  ...getEnumMeta<"EntityAdded"|"EntityDeleted"|"EntityModified">([
  {
    value: 0,
    strValue: "EntityAdded",
    displayName: "Entity Added",
  },
  {
    value: 1,
    strValue: "EntityDeleted",
    displayName: "Entity Deleted",
  },
  {
    value: 2,
    strValue: "EntityModified",
    displayName: "Entity Modified",
  },
  ]),
}
export const Permission = domain.enums.Permission = {
  name: "Permission" as const,
  displayName: "Permission",
  type: "enum",
  ...getEnumMeta<"Admin"|"UserAdmin"|"ViewAuditLogs">([
  {
    value: 1,
    strValue: "Admin",
    displayName: "Admin - General",
    description: "Modify application configuration and other administrative functions excluding user/role management.",
  },
  {
    value: 2,
    strValue: "UserAdmin",
    displayName: "Admin - Users",
    description: "Add and modify users accounts and their assigned roles. Edit roles and their permissions.",
  },
  {
    value: 3,
    strValue: "ViewAuditLogs",
    displayName: "View Audit Logs",
  },
  ]),
}
export const AppRole = domain.types.AppRole = {
  name: "AppRole" as const,
  displayName: "App Role",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "AppRole",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 0 as BehaviorFlags,
  props: {
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Name is required.",
      }
    },
    roleClaims: {
      name: "roleClaims",
      displayName: "Role Claims",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.AppRoleClaim as ModelType & { name: "AppRoleClaim" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.AppRoleClaim as ModelType & { name: "AppRoleClaim" }).props.roleId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.AppRoleClaim as ModelType & { name: "AppRoleClaim" }).props.role as ModelReferenceNavigationProperty },
      hidden: 1 as HiddenAreas,
      dontSerialize: true,
    },
    permissions: {
      name: "permissions",
      displayName: "Permissions",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "enum",
        get typeDef() { return Permission },
      },
      role: "value",
      hidden: 3 as HiddenAreas,
      dontSerialize: true,
    },
    id: {
      name: "id",
      displayName: "Id",
      type: "string",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const AppRoleClaim = domain.types.AppRoleClaim = {
  name: "AppRoleClaim" as const,
  displayName: "App Role Claim",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "AppRoleClaim",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 0 as BehaviorFlags,
  props: {
    role: {
      name: "role",
      displayName: "Role",
      type: "model",
      get typeDef() { return (domain.types.AppRole as ModelType & { name: "AppRole" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.AppRoleClaim as ModelType & { name: "AppRoleClaim" }).props.roleId as ForeignKeyProperty },
      get principalKey() { return (domain.types.AppRole as ModelType & { name: "AppRole" }).props.id as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.AppRole as ModelType & { name: "AppRole" }).props.roleClaims as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    roleId: {
      name: "roleId",
      displayName: "Role Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.AppRole as ModelType & { name: "AppRole" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.AppRole as ModelType & { name: "AppRole" }) },
      get navigationProp() { return (domain.types.AppRoleClaim as ModelType & { name: "AppRoleClaim" }).props.role as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => (val != null && val !== '') || "Role is required.",
      }
    },
    claimType: {
      name: "claimType",
      displayName: "Claim Type",
      type: "string",
      role: "value",
    },
    claimValue: {
      name: "claimValue",
      displayName: "Claim Value",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const AppUser = domain.types.AppUser = {
  name: "AppUser" as const,
  displayName: "App User",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "AppUser",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 2 as BehaviorFlags,
  props: {
    userName: {
      name: "userName",
      displayName: "User Name",
      type: "string",
      role: "value",
    },
    accessFailedCount: {
      name: "accessFailedCount",
      displayName: "Access Failed Count",
      type: "number",
      role: "value",
      dontSerialize: true,
    },
    lockoutEnd: {
      name: "lockoutEnd",
      displayName: "Lockout End",
      type: "date",
      dateKind: "datetime",
      role: "value",
      dontSerialize: true,
    },
    lockoutEnabled: {
      name: "lockoutEnabled",
      displayName: "Lockout Enabled",
      type: "boolean",
      role: "value",
      dontSerialize: true,
    },
    userRoles: {
      name: "userRoles",
      displayName: "User Roles",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.userId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.user as ModelReferenceNavigationProperty },
      manyToMany: {
        name: "roles",
        displayName: "Roles",
        get typeDef() { return (domain.types.AppRole as ModelType & { name: "AppRole" }) },
        get farForeignKey() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.roleId as ForeignKeyProperty },
        get farNavigationProp() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.role as ModelReferenceNavigationProperty },
        get nearForeignKey() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.userId as ForeignKeyProperty },
        get nearNavigationProp() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.user as ModelReferenceNavigationProperty },
      },
      dontSerialize: true,
    },
    id: {
      name: "id",
      displayName: "Id",
      type: "string",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const AppUserRole = domain.types.AppUserRole = {
  name: "AppUserRole" as const,
  displayName: "App User Role",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "AppUserRole",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 5 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "string",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    user: {
      name: "user",
      displayName: "User",
      type: "model",
      get typeDef() { return (domain.types.AppUser as ModelType & { name: "AppUser" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.userId as ForeignKeyProperty },
      get principalKey() { return (domain.types.AppUser as ModelType & { name: "AppUser" }).props.id as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.AppUser as ModelType & { name: "AppUser" }).props.userRoles as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    role: {
      name: "role",
      displayName: "Role",
      type: "model",
      get typeDef() { return (domain.types.AppRole as ModelType & { name: "AppRole" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.roleId as ForeignKeyProperty },
      get principalKey() { return (domain.types.AppRole as ModelType & { name: "AppRole" }).props.id as PrimaryKeyProperty },
      dontSerialize: true,
    },
    userId: {
      name: "userId",
      displayName: "User Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.AppUser as ModelType & { name: "AppUser" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.AppUser as ModelType & { name: "AppUser" }) },
      get navigationProp() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.user as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => (val != null && val !== '') || "User is required.",
      }
    },
    roleId: {
      name: "roleId",
      displayName: "Role Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.AppRole as ModelType & { name: "AppRole" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.AppRole as ModelType & { name: "AppRole" }) },
      get navigationProp() { return (domain.types.AppUserRole as ModelType & { name: "AppUserRole" }).props.role as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => (val != null && val !== '') || "Role is required.",
      }
    },
  },
  methods: {
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource" as const,
      displayName: "Default Source",
      isDefault: true,
      props: {
      },
    },
  },
}
export const AuditLog = domain.types.AuditLog = {
  name: "AuditLog" as const,
  displayName: "Audit Log",
  get displayProp() { return this.props.type }, 
  type: "model",
  controllerRoute: "AuditLog",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 0 as BehaviorFlags,
  props: {
    userId: {
      name: "userId",
      displayName: "User Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.AppUser as ModelType & { name: "AppUser" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.AppUser as ModelType & { name: "AppUser" }) },
      get navigationProp() { return (domain.types.AuditLog as ModelType & { name: "AuditLog" }).props.user as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    user: {
      name: "user",
      displayName: "Changed By",
      type: "model",
      get typeDef() { return (domain.types.AppUser as ModelType & { name: "AppUser" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.AuditLog as ModelType & { name: "AuditLog" }).props.userId as ForeignKeyProperty },
      get principalKey() { return (domain.types.AppUser as ModelType & { name: "AppUser" }).props.id as PrimaryKeyProperty },
      dontSerialize: true,
    },
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    type: {
      name: "type",
      displayName: "Type",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Type is required.",
        maxLength: val => !val || val.length <= 100 || "Type may not be more than 100 characters.",
      }
    },
    keyValue: {
      name: "keyValue",
      displayName: "Key Value",
      type: "string",
      role: "value",
    },
    description: {
      name: "description",
      displayName: "Description",
      type: "string",
      role: "value",
    },
    state: {
      name: "state",
      displayName: "Change Type",
      type: "enum",
      get typeDef() { return AuditEntryState },
      role: "value",
    },
    date: {
      name: "date",
      displayName: "Date",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
    properties: {
      name: "properties",
      displayName: "Properties",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.AuditLogProperty as ModelType & { name: "AuditLogProperty" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.AuditLogProperty as ModelType & { name: "AuditLogProperty" }).props.parentId as ForeignKeyProperty },
      dontSerialize: true,
    },
    clientIp: {
      name: "clientIp",
      displayName: "Client IP",
      type: "string",
      role: "value",
    },
    referrer: {
      name: "referrer",
      displayName: "Referrer",
      type: "string",
      role: "value",
    },
    endpoint: {
      name: "endpoint",
      displayName: "Endpoint",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const AuditLogProperty = domain.types.AuditLogProperty = {
  name: "AuditLogProperty" as const,
  displayName: "Audit Log Property",
  get displayProp() { return this.props.propertyName }, 
  type: "model",
  controllerRoute: "AuditLogProperty",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 0 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    parentId: {
      name: "parentId",
      displayName: "Parent Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.AuditLog as ModelType & { name: "AuditLog" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.AuditLog as ModelType & { name: "AuditLog" }) },
      rules: {
        required: val => val != null || "Parent Id is required.",
      }
    },
    propertyName: {
      name: "propertyName",
      displayName: "Property Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Property Name is required.",
        maxLength: val => !val || val.length <= 100 || "Property Name may not be more than 100 characters.",
      }
    },
    oldValue: {
      name: "oldValue",
      displayName: "Old Value",
      type: "string",
      role: "value",
    },
    oldValueDescription: {
      name: "oldValueDescription",
      displayName: "Old Value Description",
      type: "string",
      role: "value",
    },
    newValue: {
      name: "newValue",
      displayName: "New Value",
      type: "string",
      role: "value",
    },
    newValueDescription: {
      name: "newValueDescription",
      displayName: "New Value Description",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const UserInfo = domain.types.UserInfo = {
  name: "UserInfo" as const,
  displayName: "User Info",
  type: "object",
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Id is required.",
      }
    },
    userName: {
      name: "userName",
      displayName: "User Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "User Name is required.",
      }
    },
    roles: {
      name: "roles",
      displayName: "Roles",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
    permissions: {
      name: "permissions",
      displayName: "Permissions",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
  },
}
export const SecurityService = domain.services.SecurityService = {
  name: "SecurityService",
  displayName: "Security Service",
  type: "service",
  controllerRoute: "SecurityService",
  methods: {
    whoAmI: {
      name: "whoAmI",
      displayName: "Who AmI",
      transportType: "item",
      httpMethod: "GET",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.UserInfo as ObjectType & { name: "UserInfo" }) },
        role: "value",
      },
    },
  },
}

interface AppDomain extends Domain {
  enums: {
    AuditEntryState: typeof AuditEntryState
    Permission: typeof Permission
  }
  types: {
    AppRole: typeof AppRole
    AppRoleClaim: typeof AppRoleClaim
    AppUser: typeof AppUser
    AppUserRole: typeof AppUserRole
    AuditLog: typeof AuditLog
    AuditLogProperty: typeof AuditLogProperty
    UserInfo: typeof UserInfo
  }
  services: {
    SecurityService: typeof SecurityService
  }
}

solidify(domain)

export default domain as unknown as AppDomain
