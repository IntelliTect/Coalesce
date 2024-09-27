import {
  Domain, getEnumMeta, solidify, ModelType, ObjectType,
  PrimitiveProperty, ForeignKeyProperty, PrimaryKeyProperty,
  ModelCollectionNavigationProperty, ModelReferenceNavigationProperty,
  HiddenAreas, BehaviorFlags
} from 'coalesce-vue/lib/metadata'


const domain: Domain = { enums: {}, types: {}, services: {} }
export const EnumPkId = domain.enums.EnumPkId = {
  name: "EnumPkId" as const,
  displayName: "Enum Pk Id",
  type: "enum",
  ...getEnumMeta<"Value0"|"Value1"|"Value10">([
  {
    value: 0,
    strValue: "Value0",
    displayName: "Value0",
  },
  {
    value: 1,
    strValue: "Value1",
    displayName: "Value1",
  },
  {
    value: 10,
    strValue: "Value10",
    displayName: "Value 10",
  },
  ]),
}
export const Genders = domain.enums.Genders = {
  name: "Genders" as const,
  displayName: "Genders",
  type: "enum",
  ...getEnumMeta<"NonSpecified"|"Male"|"Female">([
  {
    value: 0,
    strValue: "NonSpecified",
    displayName: "Non Specified",
  },
  {
    value: 1,
    strValue: "Male",
    displayName: "Male",
  },
  {
    value: 2,
    strValue: "Female",
    displayName: "Female",
  },
  ]),
}
export const SkyConditions = domain.enums.SkyConditions = {
  name: "SkyConditions" as const,
  displayName: "Sky Conditions",
  type: "enum",
  ...getEnumMeta<"Cloudy"|"PartyCloudy"|"Sunny">([
  {
    value: 0,
    strValue: "Cloudy",
    displayName: "Cloudy",
  },
  {
    value: 1,
    strValue: "PartyCloudy",
    displayName: "Party Cloudy",
  },
  {
    value: 2,
    strValue: "Sunny",
    displayName: "Sunny",
  },
  ]),
}
export const Statuses = domain.enums.Statuses = {
  name: "Statuses" as const,
  displayName: "Statuses",
  type: "enum",
  ...getEnumMeta<"Open"|"InProgress"|"Resolved"|"ClosedNoSolution"|"Cancelled">([
  {
    value: 0,
    strValue: "Open",
    displayName: "Open",
  },
  {
    value: 1,
    strValue: "InProgress",
    displayName: "In Progress",
  },
  {
    value: 2,
    strValue: "Resolved",
    displayName: "Resolved",
    description: "Closed with a solution.",
  },
  {
    value: 3,
    strValue: "ClosedNoSolution",
    displayName: "Closed, No Solution",
    description: "Closed without any resolution.",
  },
  {
    value: 99,
    strValue: "Cancelled",
    displayName: "Cancelled",
  },
  ]),
}
export const Titles = domain.enums.Titles = {
  name: "Titles" as const,
  displayName: "Titles",
  type: "enum",
  ...getEnumMeta<"Mr"|"Ms"|"Mrs"|"Miss">([
  {
    value: 0,
    strValue: "Mr",
    displayName: "Mr",
  },
  {
    value: 1,
    strValue: "Ms",
    displayName: "Ms",
  },
  {
    value: 2,
    strValue: "Mrs",
    displayName: "Mrs",
  },
  {
    value: 4,
    strValue: "Miss",
    displayName: "Miss",
  },
  ]),
}
export const AbstractImpl = domain.types.AbstractImpl = {
  name: "AbstractImpl" as const,
  displayName: "Abstract Impl",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "AbstractImpl",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 4 as BehaviorFlags,
  props: {
    implOnlyField: {
      name: "implOnlyField",
      displayName: "Impl Only Field",
      type: "string",
      role: "value",
    },
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    discriminatior: {
      name: "discriminatior",
      displayName: "Discriminatior",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const AbstractModel = domain.types.AbstractModel = {
  name: "AbstractModel" as const,
  displayName: "Abstract Model",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "AbstractModel",
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
    discriminatior: {
      name: "discriminatior",
      displayName: "Discriminatior",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Case = domain.types.Case = {
  name: "Case" as const,
  displayName: "Case",
  get displayProp() { return this.props.title }, 
  type: "model",
  controllerRoute: "Case",
  get keyProp() { return this.props.caseKey }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    caseKey: {
      name: "caseKey",
      displayName: "Case Key",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    title: {
      name: "title",
      displayName: "Title",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "You must enter a title for the case.",
      }
    },
    description: {
      name: "description",
      displayName: "Description",
      type: "string",
      subtype: "multiline",
      role: "value",
    },
    openedAt: {
      name: "openedAt",
      displayName: "Opened At",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
    assignedToId: {
      name: "assignedToId",
      displayName: "Assigned To Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType & { name: "Person" }) },
      get navigationProp() { return (domain.types.Case as ModelType & { name: "Case" }).props.assignedTo as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    assignedTo: {
      name: "assignedTo",
      displayName: "Assigned To",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.assignedToId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType & { name: "Person" }).props.casesAssigned as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    reportedById: {
      name: "reportedById",
      displayName: "Reported By Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType & { name: "Person" }) },
      get navigationProp() { return (domain.types.Case as ModelType & { name: "Case" }).props.reportedBy as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    reportedBy: {
      name: "reportedBy",
      displayName: "Reported By",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.reportedById as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType & { name: "Person" }).props.casesReported as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    attachment: {
      name: "attachment",
      displayName: "Attachment",
      type: "binary",
      base64: true,
      role: "value",
    },
    status: {
      name: "status",
      displayName: "Status",
      type: "enum",
      get typeDef() { return Statuses },
      role: "value",
    },
    caseProducts: {
      name: "caseProducts",
      displayName: "Case Products",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.caseId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.case as ModelReferenceNavigationProperty },
      manyToMany: {
        name: "products",
        displayName: "Products",
        get typeDef() { return (domain.types.Product as ModelType & { name: "Product" }) },
        get farForeignKey() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.productId as ForeignKeyProperty },
        get farNavigationProp() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.product as ModelReferenceNavigationProperty },
        get nearForeignKey() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.caseId as ForeignKeyProperty },
        get nearNavigationProp() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.case as ModelReferenceNavigationProperty },
      },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
    allOpenCases: {
      type: "dataSource",
      name: "AllOpenCases" as const,
      displayName: "All Open Cases",
      props: {
        minDate: {
          name: "minDate",
          displayName: "Min Date",
          type: "date",
          dateKind: "datetime",
          role: "value",
        },
      },
    },
  },
}
export const CaseDtoStandalone = domain.types.CaseDtoStandalone = {
  name: "CaseDtoStandalone" as const,
  displayName: "Case Dto Standalone",
  get displayProp() { return this.props.caseId }, 
  type: "model",
  controllerRoute: "CaseDtoStandalone",
  get keyProp() { return this.props.caseId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    title: {
      name: "title",
      displayName: "Title",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const CaseProduct = domain.types.CaseProduct = {
  name: "CaseProduct" as const,
  displayName: "Case Product",
  get displayProp() { return this.props.caseProductId }, 
  type: "model",
  controllerRoute: "CaseProduct",
  get keyProp() { return this.props.caseProductId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    caseProductId: {
      name: "caseProductId",
      displayName: "Case Product Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.caseKey as PrimaryKeyProperty },
      get principalType() { return (domain.types.Case as ModelType & { name: "Case" }) },
      get navigationProp() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.case as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Case is required.",
      }
    },
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      get typeDef() { return (domain.types.Case as ModelType & { name: "Case" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.caseId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.caseKey as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType & { name: "Case" }).props.caseProducts as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Product as ModelType & { name: "Product" }).props.productId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Product as ModelType & { name: "Product" }) },
      get navigationProp() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.product as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Product is required.",
      }
    },
    product: {
      name: "product",
      displayName: "Product",
      type: "model",
      get typeDef() { return (domain.types.Product as ModelType & { name: "Product" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType & { name: "CaseProduct" }).props.productId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Product as ModelType & { name: "Product" }).props.productId as PrimaryKeyProperty },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Company = domain.types.Company = {
  name: "Company" as const,
  displayName: "Company",
  get displayProp() { return this.props.altName }, 
  type: "model",
  controllerRoute: "Company",
  get keyProp() { return this.props.companyId }, 
  behaviorFlags: 6 as BehaviorFlags,
  props: {
    companyId: {
      name: "companyId",
      displayName: "Company Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    address1: {
      name: "address1",
      displayName: "Address1",
      type: "string",
      role: "value",
    },
    address2: {
      name: "address2",
      displayName: "Address2",
      type: "string",
      role: "value",
    },
    city: {
      name: "city",
      displayName: "City",
      type: "string",
      role: "value",
    },
    state: {
      name: "state",
      displayName: "State",
      type: "string",
      role: "value",
    },
    zipCode: {
      name: "zipCode",
      displayName: "Zip Code",
      type: "string",
      role: "value",
    },
    phone: {
      name: "phone",
      displayName: "Phone",
      type: "string",
      subtype: "tel",
      role: "value",
    },
    websiteUrl: {
      name: "websiteUrl",
      displayName: "Website Url",
      type: "string",
      subtype: "url",
      role: "value",
    },
    logoUrl: {
      name: "logoUrl",
      displayName: "Logo Url",
      type: "string",
      subtype: "url-image",
      role: "value",
    },
    employees: {
      name: "employees",
      displayName: "Employees",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.companyId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType & { name: "Person" }).props.company as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
    altName: {
      name: "altName",
      displayName: "Alt Name",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const ComplexModel = domain.types.ComplexModel = {
  name: "ComplexModel" as const,
  displayName: "Complex Model",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "ComplexModel",
  get keyProp() { return this.props.complexModelId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    complexModelId: {
      name: "complexModelId",
      displayName: "Complex Model Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    tests: {
      name: "tests",
      displayName: "Tests",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Test as ModelType & { name: "Test" }).props.complexModelId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Test as ModelType & { name: "Test" }).props.complexModel as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
    childrenWithoutRefNavProp: {
      name: "childrenWithoutRefNavProp",
      displayName: "Children Without Ref Nav Prop",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.ComplexModelDependent as ModelType & { name: "ComplexModelDependent" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.ComplexModelDependent as ModelType & { name: "ComplexModelDependent" }).props.parentId as ForeignKeyProperty },
      dontSerialize: true,
    },
    unmappedCollectionOfMappedModels: {
      name: "unmappedCollectionOfMappedModels",
      displayName: "Unmapped Collection Of Mapped Models",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
      },
      role: "value",
      dontSerialize: true,
    },
    singleTestId: {
      name: "singleTestId",
      displayName: "Single Test Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Test as ModelType & { name: "Test" }).props.testId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Test as ModelType & { name: "Test" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.singleTest as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Single Test is required.",
      }
    },
    singleTest: {
      name: "singleTest",
      displayName: "Single Test",
      description: "The active Test record for the model.",
      type: "model",
      get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.singleTestId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Test as ModelType & { name: "Test" }).props.testId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    enumPkId: {
      name: "enumPkId",
      displayName: "Enum Pk Id",
      type: "enum",
      get typeDef() { return EnumPkId },
      role: "foreignKey",
      get principalKey() { return (domain.types.EnumPk as ModelType & { name: "EnumPk" }).props.enumPkId as PrimaryKeyProperty },
      get principalType() { return (domain.types.EnumPk as ModelType & { name: "EnumPk" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.enumPk as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Enum Pk is required.",
      }
    },
    enumPk: {
      name: "enumPk",
      displayName: "Enum Pk",
      type: "model",
      get typeDef() { return (domain.types.EnumPk as ModelType & { name: "EnumPk" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.enumPkId as ForeignKeyProperty },
      get principalKey() { return (domain.types.EnumPk as ModelType & { name: "EnumPk" }).props.enumPkId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    dateTimeOffset: {
      name: "dateTimeOffset",
      displayName: "Date Time Offset",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
    dateTimeOffsetNullable: {
      name: "dateTimeOffsetNullable",
      displayName: "Date Time Offset Nullable",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
    dateTime: {
      name: "dateTime",
      displayName: "Date Time",
      type: "date",
      dateKind: "datetime",
      noOffset: true,
      role: "value",
    },
    dateTimeNullable: {
      name: "dateTimeNullable",
      displayName: "Date Time Nullable",
      type: "date",
      dateKind: "datetime",
      noOffset: true,
      role: "value",
    },
    systemDateOnly: {
      name: "systemDateOnly",
      displayName: "System Date Only",
      type: "date",
      dateKind: "date",
      noOffset: true,
      role: "value",
    },
    systemTimeOnly: {
      name: "systemTimeOnly",
      displayName: "System Time Only",
      type: "date",
      dateKind: "time",
      noOffset: true,
      role: "value",
    },
    dateOnlyViaAttribute: {
      name: "dateOnlyViaAttribute",
      displayName: "Date Only Via Attribute",
      type: "date",
      dateKind: "date",
      noOffset: true,
      serializeAs: "datetime",
      role: "value",
    },
    unmappedSettableString: {
      name: "unmappedSettableString",
      displayName: "Unmapped Settable String",
      type: "string",
      role: "value",
    },
    adminReadableString: {
      name: "adminReadableString",
      displayName: "Admin Readable String",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
    restrictedString: {
      name: "restrictedString",
      displayName: "Restricted String",
      type: "string",
      role: "value",
    },
    restrictInit: {
      name: "restrictInit",
      displayName: "Restrict Init",
      type: "string",
      role: "value",
      createOnly: true,
    },
    adminReadableReferenceNavigationId: {
      name: "adminReadableReferenceNavigationId",
      displayName: "Admin Readable Reference Navigation Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get principalType() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.adminReadableReferenceNavigation as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      dontSerialize: true,
    },
    adminReadableReferenceNavigation: {
      name: "adminReadableReferenceNavigation",
      displayName: "Admin Readable Reference Navigation",
      type: "model",
      get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.adminReadableReferenceNavigationId as ForeignKeyProperty },
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    referenceNavigationId: {
      name: "referenceNavigationId",
      displayName: "Reference Navigation Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get principalType() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.referenceNavigation as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    referenceNavigation: {
      name: "referenceNavigation",
      displayName: "Reference Navigation",
      type: "model",
      get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.referenceNavigationId as ForeignKeyProperty },
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    noAutoIncludeReferenceNavigationId: {
      name: "noAutoIncludeReferenceNavigationId",
      displayName: "No Auto Include Reference Navigation Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get principalType() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.noAutoIncludeReferenceNavigation as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    noAutoIncludeReferenceNavigation: {
      name: "noAutoIncludeReferenceNavigation",
      displayName: "No Auto Include Reference Navigation",
      type: "model",
      get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.noAutoIncludeReferenceNavigationId as ForeignKeyProperty },
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    noAutoIncludeByClassReferenceNavigationId: {
      name: "noAutoIncludeByClassReferenceNavigationId",
      displayName: "No Auto Include By Class Reference Navigation Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Company as ModelType & { name: "Company" }).props.companyId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Company as ModelType & { name: "Company" }) },
      get navigationProp() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.noAutoIncludeByClassReferenceNavigation as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    noAutoIncludeByClassReferenceNavigation: {
      name: "noAutoIncludeByClassReferenceNavigation",
      displayName: "No Auto Include By Class Reference Navigation",
      type: "model",
      get typeDef() { return (domain.types.Company as ModelType & { name: "Company" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.noAutoIncludeByClassReferenceNavigationId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Company as ModelType & { name: "Company" }).props.companyId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    isActive: {
      name: "isActive",
      displayName: "Is Active",
      type: "boolean",
      role: "value",
    },
    byteArrayProp: {
      name: "byteArrayProp",
      displayName: "Byte Array Prop",
      type: "binary",
      base64: true,
      role: "value",
    },
    string: {
      name: "string",
      displayName: "String",
      type: "string",
      role: "value",
    },
    stringWithDefault: {
      name: "stringWithDefault",
      displayName: "String With Default",
      type: "string",
      role: "value",
      defaultValue: "Inigo",
    },
    intWithDefault: {
      name: "intWithDefault",
      displayName: "Int With Default",
      type: "number",
      role: "value",
      defaultValue: 42,
    },
    doubleWithDefault: {
      name: "doubleWithDefault",
      displayName: "Double With Default",
      type: "number",
      role: "value",
      defaultValue: 3.141592653589793,
      rules: {
        min: val => val == null || val >= -1.7976931348623157E+308 || "Double With Default must be at least -1.7976931348623157E+308.",
        max: val => val == null || val <= 1.7976931348623157E+308 || "Double With Default may not be more than 1.7976931348623157E+308.",
      }
    },
    enumWithDefault: {
      name: "enumWithDefault",
      displayName: "Enum With Default",
      type: "enum",
      get typeDef() { return EnumPkId },
      role: "value",
      defaultValue: 10,
    },
    color: {
      name: "color",
      displayName: "Color",
      type: "string",
      subtype: "color",
      role: "value",
    },
    stringSearchedEqualsInsensitive: {
      name: "stringSearchedEqualsInsensitive",
      displayName: "String Searched Equals Insensitive",
      type: "string",
      role: "value",
    },
    stringSearchedEqualsNatural: {
      name: "stringSearchedEqualsNatural",
      displayName: "String Searched Equals Natural",
      type: "string",
      role: "value",
    },
    int: {
      name: "int",
      displayName: "Int",
      type: "number",
      role: "value",
    },
    intNullable: {
      name: "intNullable",
      displayName: "Int Nullable",
      type: "number",
      role: "value",
    },
    decimalNullable: {
      name: "decimalNullable",
      displayName: "Decimal Nullable",
      type: "number",
      role: "value",
    },
    long: {
      name: "long",
      displayName: "Long",
      type: "number",
      role: "value",
    },
    guid: {
      name: "guid",
      displayName: "Guid",
      type: "string",
      role: "value",
      rules: {
        pattern: val => !val || /^\s*[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?\s*$/.test(val) || "Guid does not match expected format.",
      }
    },
    guidNullable: {
      name: "guidNullable",
      displayName: "Guid Nullable",
      type: "string",
      role: "value",
      rules: {
        pattern: val => !val || /^\s*[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?\s*$/.test(val) || "Guid Nullable does not match expected format.",
      }
    },
    intCollection: {
      name: "intCollection",
      displayName: "Int Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    enumCollection: {
      name: "enumCollection",
      displayName: "Enum Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "enum",
        get typeDef() { return Statuses },
      },
      role: "value",
    },
    nonNullNonZeroInt: {
      name: "nonNullNonZeroInt",
      displayName: "Non Null Non Zero Int",
      type: "number",
      role: "value",
      rules: {
        required: val => val != null || "Non Null Non Zero Int is required.",
        min: val => val == null || val >= 1 || "Non Null Non Zero Int must be at least 1.",
        max: val => val == null || val <= 100 || "Non Null Non Zero Int may not be more than 100.",
      }
    },
    clientValidationInt: {
      name: "clientValidationInt",
      displayName: "Client Validation Int",
      type: "number",
      role: "value",
      rules: {
        min: val => val == null || val >= 0 || "Client Validation Int must be at least 0.",
        max: val => val == null || val <= 10 || "Client Validation Int may not be more than 10.",
      }
    },
    clientValidationString: {
      name: "clientValidationString",
      displayName: "Client Validation String",
      type: "string",
      role: "value",
      rules: {
        minLength: val => !val || val.length >= 0 || "Client Validation String must be at least 0 characters.",
        maxLength: val => !val || val.length <= 10 || "Client Validation String may not be more than 10 characters.",
      }
    },
    enumNullable: {
      name: "enumNullable",
      displayName: "Enum Nullable",
      type: "enum",
      get typeDef() { return Statuses },
      role: "value",
    },
    readOnlyPrimitiveCollection: {
      name: "readOnlyPrimitiveCollection",
      displayName: "Read Only Primitive Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
    mutablePrimitiveCollection: {
      name: "mutablePrimitiveCollection",
      displayName: "Mutable Primitive Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
    primitiveEnumerable: {
      name: "primitiveEnumerable",
      displayName: "Primitive Enumerable",
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
  methods: {
    methodWithManyParams: {
      name: "methodWithManyParams",
      displayName: "Method With Many Params",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        singleExternal: {
          name: "singleExternal",
          displayName: "Single External",
          type: "object",
          get typeDef() { return (domain.types.ExternalParent as ObjectType & { name: "ExternalParent" }) },
          role: "value",
        },
        collectionExternal: {
          name: "collectionExternal",
          displayName: "Collection External",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "object",
            get typeDef() { return (domain.types.ExternalParent as ObjectType & { name: "ExternalParent" }) },
          },
          role: "value",
        },
        file: {
          name: "file",
          displayName: "File",
          type: "file",
          role: "value",
        },
        strParam: {
          name: "strParam",
          displayName: "Str Param",
          type: "string",
          role: "value",
        },
        stringsParam: {
          name: "stringsParam",
          displayName: "Strings Param",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "string",
          },
          role: "value",
        },
        dateTime: {
          name: "dateTime",
          displayName: "Date Time",
          type: "date",
          dateKind: "datetime",
          noOffset: true,
          role: "value",
        },
        integer: {
          name: "integer",
          displayName: "Integer",
          type: "number",
          role: "value",
        },
        boolParam: {
          name: "boolParam",
          displayName: "Bool Param",
          type: "boolean",
          role: "value",
        },
        enumParam: {
          name: "enumParam",
          displayName: "Enum Param",
          type: "enum",
          get typeDef() { return Statuses },
          role: "value",
        },
        enumsParam: {
          name: "enumsParam",
          displayName: "Enums Param",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "enum",
            get typeDef() { return Statuses },
          },
          role: "value",
        },
        model: {
          name: "model",
          displayName: "Model",
          type: "model",
          get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
          role: "value",
        },
        modelCollection: {
          name: "modelCollection",
          displayName: "Model Collection",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "model",
            get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
          },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.ExternalParent as ObjectType & { name: "ExternalParent" }) },
        role: "value",
      },
    },
    methodWithOptionalParams: {
      name: "methodWithOptionalParams",
      displayName: "Method With Optional Params",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        requiredInt: {
          name: "requiredInt",
          displayName: "Required Int",
          type: "number",
          role: "value",
          rules: {
            required: val => val != null || "Required Int is required.",
          }
        },
        plainInt: {
          name: "plainInt",
          displayName: "Plain Int",
          type: "number",
          role: "value",
        },
        nullableInt: {
          name: "nullableInt",
          displayName: "Nullable Int",
          type: "number",
          role: "value",
        },
        intWithDefault: {
          name: "intWithDefault",
          displayName: "Int With Default",
          type: "number",
          role: "value",
        },
        enumWithDefault: {
          name: "enumWithDefault",
          displayName: "Enum With Default",
          type: "enum",
          get typeDef() { return Statuses },
          role: "value",
        },
        stringWithDefault: {
          name: "stringWithDefault",
          displayName: "String With Default",
          type: "string",
          role: "value",
        },
        optionalObject: {
          name: "optionalObject",
          displayName: "Optional Object",
          type: "model",
          get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
          role: "value",
        },
        optionalObjectCollection: {
          name: "optionalObjectCollection",
          displayName: "Optional Object Collection",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "model",
            get typeDef() { return (domain.types.Test as ModelType & { name: "Test" }) },
          },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
    methodWithRequiredAfterOptional: {
      name: "methodWithRequiredAfterOptional",
      displayName: "Method With Required After Optional",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        optionalInt: {
          name: "optionalInt",
          displayName: "Optional Int",
          type: "number",
          role: "value",
        },
        singleExternal: {
          name: "singleExternal",
          displayName: "Single External",
          type: "object",
          get typeDef() { return (domain.types.ExternalParent as ObjectType & { name: "ExternalParent" }) },
          role: "value",
          rules: {
            required: val => val != null || "Single External is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "number",
        role: "value",
      },
    },
    methodWithExternalTypesWithSinglePurpose: {
      name: "methodWithExternalTypesWithSinglePurpose",
      displayName: "Method With External Types With Single Purpose",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        single: {
          name: "single",
          displayName: "Single",
          type: "object",
          get typeDef() { return (domain.types.ExternalParentAsInputOnly as ObjectType & { name: "ExternalParentAsInputOnly" }) },
          role: "value",
        },
        collection: {
          name: "collection",
          displayName: "Collection",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "object",
            get typeDef() { return (domain.types.ExternalParentAsInputOnly as ObjectType & { name: "ExternalParentAsInputOnly" }) },
          },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.ExternalParentAsOutputOnly as ObjectType & { name: "ExternalParentAsOutputOnly" }) },
        role: "value",
      },
    },
    methodWithOutputOnlyExternalType: {
      name: "methodWithOutputOnlyExternalType",
      displayName: "Method With Output Only External Type",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.OutputOnlyExternalTypeWithoutDefaultCtor as ObjectType & { name: "OutputOnlyExternalTypeWithoutDefaultCtor" }) },
        role: "value",
      },
    },
    methodWithOutputOnlyExternalType2: {
      name: "methodWithOutputOnlyExternalType2",
      displayName: "Method With Output Only External Type2",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties as ObjectType & { name: "OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties" }) },
        role: "value",
      },
    },
    methodWithOutputOnlyExternalType3: {
      name: "methodWithOutputOnlyExternalType3",
      displayName: "Method With Output Only External Type3",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.OutputOnlyExternalTypeWithRequiredEntityProp as ObjectType & { name: "OutputOnlyExternalTypeWithRequiredEntityProp" }) },
        role: "value",
      },
    },
    methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp: {
      name: "methodWithInputOutputOnlyExternalTypeWithRequiredNonscalarProp",
      displayName: "Method With Input Output Only External Type With Required Nonscalar Prop",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        i: {
          name: "i",
          displayName: "I",
          type: "object",
          get typeDef() { return (domain.types.InputOutputOnlyExternalTypeWithRequiredNonscalarProp as ObjectType & { name: "InputOutputOnlyExternalTypeWithRequiredNonscalarProp" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.InputOutputOnlyExternalTypeWithRequiredNonscalarProp as ObjectType & { name: "InputOutputOnlyExternalTypeWithRequiredNonscalarProp" }) },
        role: "value",
      },
    },
    methodWithSingleFileParameter: {
      name: "methodWithSingleFileParameter",
      displayName: "Method With Single File Parameter",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        file: {
          name: "file",
          displayName: "File",
          type: "file",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    methodWithMultiFileParameter: {
      name: "methodWithMultiFileParameter",
      displayName: "Method With Multi File Parameter",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        files: {
          name: "files",
          displayName: "Files",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "file",
          },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    methodWithStringArrayParameterAndReturn: {
      name: "methodWithStringArrayParameterAndReturn",
      displayName: "Method With String Array Parameter And Return",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        strings: {
          name: "strings",
          displayName: "Strings",
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
      return: {
        name: "$return",
        displayName: "Result",
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
    downloadAttachment: {
      name: "downloadAttachment",
      displayName: "Download Attachment",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachment_VaryByteArray: {
      name: "downloadAttachment_VaryByteArray",
      displayName: "Download Attachment _ Vary Byte Array",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "binary",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.byteArrayProp },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachment_VaryDate: {
      name: "downloadAttachment_VaryDate",
      displayName: "Download Attachment _ Vary Date",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "date",
          dateKind: "datetime",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.dateTimeOffset },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachment_VaryString: {
      name: "downloadAttachment_VaryString",
      displayName: "Download Attachment _ Vary String",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "string",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.name },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachment_VaryInt: {
      name: "downloadAttachment_VaryInt",
      displayName: "Download Attachment _ Vary Int",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.int },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachment_VaryGuid: {
      name: "downloadAttachment_VaryGuid",
      displayName: "Download Attachment _ Vary Guid",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "string",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.guid },
          rules: {
            pattern: val => !val || /^\s*[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?\s*$/.test(val) || "Etag does not match expected format.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachmentItemResult: {
      name: "downloadAttachmentItemResult",
      displayName: "Download Attachment Item Result",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    downloadAttachmentStatic: {
      name: "downloadAttachmentStatic",
      displayName: "Download Attachment Static",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    methodWithOptionalCancellationToken: {
      name: "methodWithOptionalCancellationToken",
      displayName: "Method With Optional Cancellation Token",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        q: {
          name: "q",
          displayName: "Q",
          type: "string",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    methodWithOptionalEnumParam: {
      name: "methodWithOptionalEnumParam",
      displayName: "Method With Optional Enum Param",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        status: {
          name: "status",
          displayName: "Status",
          type: "enum",
          get typeDef() { return Statuses },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    externalTypeWithDtoProp: {
      name: "externalTypeWithDtoProp",
      displayName: "External Type With Dto Prop",
      transportType: "item",
      httpMethod: "POST",
      hidden: 3 as HiddenAreas,
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        input: {
          name: "input",
          displayName: "Input",
          type: "object",
          get typeDef() { return (domain.types.ExternalTypeWithDtoProp as ObjectType & { name: "ExternalTypeWithDtoProp" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.ExternalTypeWithDtoProp as ObjectType & { name: "ExternalTypeWithDtoProp" }) },
        role: "value",
      },
    },
    customDto: {
      name: "customDto",
      displayName: "Custom Dto",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        input: {
          name: "input",
          displayName: "Input",
          type: "model",
          get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
        role: "value",
      },
    },
    hasTopLevelParamWithSameNameAsObjectProp: {
      name: "hasTopLevelParamWithSameNameAsObjectProp",
      displayName: "Has Top Level Param With Same Name As Object Prop",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        complexModelId: {
          name: "complexModelId",
          displayName: "Complex Model Id",
          type: "number",
          role: "value",
        },
        model: {
          name: "model",
          displayName: "Model",
          type: "model",
          get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    methodWithPositionRecord: {
      name: "methodWithPositionRecord",
      displayName: "Method With Position Record",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        rec: {
          name: "rec",
          displayName: "Rec",
          type: "object",
          get typeDef() { return (domain.types.PositionalRecord as ObjectType & { name: "PositionalRecord" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.PositionalRecord as ObjectType & { name: "PositionalRecord" }) },
        role: "value",
      },
    },
    methodWithInitRecord: {
      name: "methodWithInitRecord",
      displayName: "Method With Init Record",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        rec: {
          name: "rec",
          displayName: "Rec",
          type: "object",
          get typeDef() { return (domain.types.InitRecordWithDefaultCtor as ObjectType & { name: "InitRecordWithDefaultCtor" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.InitRecordWithDefaultCtor as ObjectType & { name: "InitRecordWithDefaultCtor" }) },
        role: "value",
      },
    },
    methodWithValidationExplicitOff: {
      name: "methodWithValidationExplicitOff",
      displayName: "Method With Validation Explicit Off",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        target: {
          name: "target",
          displayName: "Target",
          type: "object",
          get typeDef() { return (domain.types.ValidationTarget as ObjectType & { name: "ValidationTarget" }) },
          role: "value",
          rules: {
            required: val => val != null || "Target is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    methodWithValidationExplicitOn: {
      name: "methodWithValidationExplicitOn",
      displayName: "Method With Validation Explicit On",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        target: {
          name: "target",
          displayName: "Target",
          type: "object",
          get typeDef() { return (domain.types.ValidationTarget as ObjectType & { name: "ValidationTarget" }) },
          role: "value",
          rules: {
            required: val => val != null || "Target is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
  },
  dataSources: {
  },
}
export const ComplexModelDependent = domain.types.ComplexModelDependent = {
  name: "ComplexModelDependent" as const,
  displayName: "Complex Model Dependent",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "ComplexModelDependent",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7 as BehaviorFlags,
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
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get principalType() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      rules: {
        required: val => val != null || "Parent Id is required.",
      }
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const EnumPk = domain.types.EnumPk = {
  name: "EnumPk" as const,
  displayName: "Enum Pk",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "EnumPk",
  get keyProp() { return this.props.enumPkId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    enumPkId: {
      name: "enumPkId",
      displayName: "Enum Pk Id",
      type: "enum",
      get typeDef() { return EnumPkId },
      role: "primaryKey",
      createOnly: true,
      rules: {
        required: val => val != null || "Enum Pk Id is required.",
      }
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Person = domain.types.Person = {
  name: "Person" as const,
  displayName: "Person",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Person",
  get keyProp() { return this.props.personId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    title: {
      name: "title",
      displayName: "Title",
      type: "enum",
      get typeDef() { return Titles },
      role: "value",
    },
    firstName: {
      name: "firstName",
      displayName: "First Name",
      type: "string",
      role: "value",
      rules: {
        minLength: val => !val || val.length >= 2 || "First Name must be at least 2 characters.",
        maxLength: val => !val || val.length <= 75 || "First Name may not be more than 75 characters.",
      }
    },
    lastName: {
      name: "lastName",
      displayName: "Last Name",
      type: "string",
      role: "value",
      rules: {
        minLength: val => !val || val.length >= 3 || "Last Name must be at least 3 characters.",
        maxLength: val => !val || val.length <= 100 || "Last Name may not be more than 100 characters.",
      }
    },
    personId: {
      name: "personId",
      displayName: "Person Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    email: {
      name: "email",
      displayName: "Email",
      type: "string",
      subtype: "email",
      role: "value",
    },
    secretPhrase: {
      name: "secretPhrase",
      displayName: "Secret Phrase",
      type: "string",
      subtype: "password",
      role: "value",
    },
    gender: {
      name: "gender",
      displayName: "Gender",
      type: "enum",
      get typeDef() { return Genders },
      role: "value",
    },
    casesAssigned: {
      name: "casesAssigned",
      displayName: "Cases Assigned",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Case as ModelType & { name: "Case" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.assignedToId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType & { name: "Case" }).props.assignedTo as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
    casesReported: {
      name: "casesReported",
      displayName: "Cases Reported",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Case as ModelType & { name: "Case" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType & { name: "Case" }).props.reportedById as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType & { name: "Case" }).props.reportedBy as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
    birthDate: {
      name: "birthDate",
      displayName: "Birth Date",
      type: "date",
      dateKind: "date",
      noOffset: true,
      serializeAs: "datetime",
      role: "value",
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
    companyId: {
      name: "companyId",
      displayName: "Company Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Company as ModelType & { name: "Company" }).props.companyId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Company as ModelType & { name: "Company" }) },
      get navigationProp() { return (domain.types.Person as ModelType & { name: "Person" }).props.company as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Company is required.",
      }
    },
    company: {
      name: "company",
      displayName: "Company",
      type: "model",
      get typeDef() { return (domain.types.Company as ModelType & { name: "Company" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.companyId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Company as ModelType & { name: "Company" }).props.companyId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Company as ModelType & { name: "Company" }).props.employees as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    siblingRelationships: {
      name: "siblingRelationships",
      displayName: "Sibling Relationships",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Sibling as ModelType & { name: "Sibling" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.person as ModelReferenceNavigationProperty },
      manyToMany: {
        name: "siblings",
        displayName: "Siblings",
        get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
        get farForeignKey() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personTwoId as ForeignKeyProperty },
        get farNavigationProp() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personTwo as ModelReferenceNavigationProperty },
        get nearForeignKey() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personId as ForeignKeyProperty },
        get nearNavigationProp() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.person as ModelReferenceNavigationProperty },
      },
      dontSerialize: true,
    },
  },
  methods: {
    rename: {
      name: "rename",
      displayName: "Rename",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
        name: {
          name: "name",
          displayName: "Name",
          type: "string",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
        role: "value",
      },
    },
    changeSpacesToDashesInName: {
      name: "changeSpacesToDashesInName",
      displayName: "Change Spaces To Dashes In Name",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    add: {
      name: "add",
      displayName: "Add",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        numberOne: {
          name: "numberOne",
          displayName: "Number One",
          type: "number",
          role: "value",
        },
        numberTwo: {
          name: "numberTwo",
          displayName: "Number Two",
          type: "number",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "number",
        role: "value",
      },
    },
    getUser: {
      name: "getUser",
      displayName: "Get User",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
    getUserPublic: {
      name: "getUserPublic",
      displayName: "Get User Public",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
    namesStartingWith: {
      name: "namesStartingWith",
      displayName: "Names Starting With",
      transportType: "item",
      httpMethod: "GET",
      isStatic: true,
      params: {
        characters: {
          name: "characters",
          displayName: "Characters",
          type: "string",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
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
  },
  dataSources: {
    borCPeople: {
      type: "dataSource",
      name: "BorCPeople" as const,
      displayName: "Bor C People",
      props: {
      },
    },
    namesStartingWithAWithCases: {
      type: "dataSource",
      name: "NamesStartingWithAWithCases" as const,
      displayName: "Names Starting With A With Cases",
      props: {
        allowedStatuses: {
          name: "allowedStatuses",
          displayName: "Allowed Statuses",
          type: "collection",
          itemType: {
            name: "$collectionItem",
            displayName: "",
            role: "value",
            type: "enum",
            get typeDef() { return Statuses },
          },
          role: "value",
        },
        hasEmail: {
          name: "hasEmail",
          displayName: "Has Email",
          type: "boolean",
          role: "value",
        },
      },
    },
    withoutCases: {
      type: "dataSource",
      name: "WithoutCases" as const,
      displayName: "Without Cases",
      isDefault: true,
      props: {
      },
    },
  },
}
export const Product = domain.types.Product = {
  name: "Product" as const,
  displayName: "Product",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Product",
  get keyProp() { return this.props.productId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const ReadOnlyEntityUsedAsMethodInput = domain.types.ReadOnlyEntityUsedAsMethodInput = {
  name: "ReadOnlyEntityUsedAsMethodInput" as const,
  displayName: "Read Only Entity Used As Method Input",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "ReadOnlyEntityUsedAsMethodInput",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 4 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
    staticCreate: {
      name: "staticCreate",
      displayName: "Static Create",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        foo: {
          name: "foo",
          displayName: "Foo",
          type: "model",
          get typeDef() { return (domain.types.ReadOnlyEntityUsedAsMethodInput as ModelType & { name: "ReadOnlyEntityUsedAsMethodInput" }) },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
  },
  dataSources: {
  },
}
export const RecursiveHierarchy = domain.types.RecursiveHierarchy = {
  name: "RecursiveHierarchy" as const,
  displayName: "Recursive Hierarchy",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "RecursiveHierarchy",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    parentId: {
      name: "parentId",
      displayName: "Parent Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }) },
      get navigationProp() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.parent as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    parent: {
      name: "parent",
      displayName: "Parent",
      type: "model",
      get typeDef() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.parentId as ForeignKeyProperty },
      get principalKey() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.id as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.children as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    children: {
      name: "children",
      displayName: "Children",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.parentId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.RecursiveHierarchy as ModelType & { name: "RecursiveHierarchy" }).props.parent as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const RequiredAndInitModel = domain.types.RequiredAndInitModel = {
  name: "RequiredAndInitModel" as const,
  displayName: "Required And Init Model",
  get displayProp() { return this.props.id }, 
  type: "model",
  controllerRoute: "RequiredAndInitModel",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    requiredRef: {
      name: "requiredRef",
      displayName: "Required Ref",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Required Ref is required.",
      }
    },
    requiredValue: {
      name: "requiredValue",
      displayName: "Required Value",
      type: "number",
      role: "value",
      rules: {
        required: val => val != null || "Required Value is required.",
      }
    },
    requiredInitRef: {
      name: "requiredInitRef",
      displayName: "Required Init Ref",
      type: "string",
      role: "value",
      createOnly: true,
      rules: {
        required: val => (val != null && val !== '') || "Required Init Ref is required.",
      }
    },
    requiredInitValue: {
      name: "requiredInitValue",
      displayName: "Required Init Value",
      type: "number",
      role: "value",
      createOnly: true,
      rules: {
        required: val => val != null || "Required Init Value is required.",
      }
    },
    initRef: {
      name: "initRef",
      displayName: "Init Ref",
      type: "string",
      role: "value",
      createOnly: true,
    },
    initValue: {
      name: "initValue",
      displayName: "Init Value",
      type: "number",
      role: "value",
      createOnly: true,
    },
    nullableInitValue: {
      name: "nullableInitValue",
      displayName: "Nullable Init Value",
      type: "number",
      role: "value",
      createOnly: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Sibling = domain.types.Sibling = {
  name: "Sibling" as const,
  displayName: "Sibling",
  get displayProp() { return this.props.siblingId }, 
  type: "model",
  controllerRoute: "Sibling",
  get keyProp() { return this.props.siblingId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    siblingId: {
      name: "siblingId",
      displayName: "Sibling Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    personId: {
      name: "personId",
      displayName: "Person Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType & { name: "Person" }) },
      get navigationProp() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.person as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Person is required.",
      }
    },
    person: {
      name: "person",
      displayName: "Person",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType & { name: "Person" }).props.siblingRelationships as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    personTwoId: {
      name: "personTwoId",
      displayName: "Person Two Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType & { name: "Person" }) },
      get navigationProp() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personTwo as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Person Two is required.",
      }
    },
    personTwo: {
      name: "personTwo",
      displayName: "Person Two",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType & { name: "Person" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Sibling as ModelType & { name: "Sibling" }).props.personTwoId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType & { name: "Person" }).props.personId as PrimaryKeyProperty },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const StandaloneReadonly = domain.types.StandaloneReadonly = {
  name: "StandaloneReadonly" as const,
  displayName: "Standalone Readonly",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "StandaloneReadonly",
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
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    description: {
      name: "description",
      displayName: "Description",
      type: "string",
      role: "value",
    },
  },
  methods: {
    instanceMethod: {
      name: "instanceMethod",
      displayName: "Instance Method",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.StandaloneReadonly as ObjectType & { name: "StandaloneReadonly" }).props.id },
          rules: {
            required: val => val != null || "Primary Key is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "number",
        role: "value",
      },
    },
    staticMethod: {
      name: "staticMethod",
      displayName: "Static Method",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "number",
        role: "value",
      },
    },
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource" as const,
      displayName: "Default Source",
      props: {
      },
    },
  },
}
export const StandaloneReadWrite = domain.types.StandaloneReadWrite = {
  name: "StandaloneReadWrite" as const,
  displayName: "Standalone Read Write",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "StandaloneReadWrite",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    date: {
      name: "date",
      displayName: "Date",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource" as const,
      displayName: "Default Source",
      props: {
      },
    },
  },
}
export const StringIdentity = domain.types.StringIdentity = {
  name: "StringIdentity" as const,
  displayName: "String Identity",
  get displayProp() { return this.props.stringIdentityId }, 
  type: "model",
  controllerRoute: "StringIdentity",
  get keyProp() { return this.props.stringIdentityId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    stringIdentityId: {
      name: "stringIdentityId",
      displayName: "String Identity Id",
      type: "string",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    parentId: {
      name: "parentId",
      displayName: "Parent Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.stringIdentityId as PrimaryKeyProperty },
      get principalType() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }) },
      get navigationProp() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parent as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
    },
    parent: {
      name: "parent",
      displayName: "Parent",
      type: "model",
      get typeDef() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parentId as ForeignKeyProperty },
      get principalKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.stringIdentityId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.children as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    parentReqId: {
      name: "parentReqId",
      displayName: "Parent Req Id",
      type: "string",
      role: "foreignKey",
      get principalKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.stringIdentityId as PrimaryKeyProperty },
      get principalType() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }) },
      get navigationProp() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parentReq as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => (val != null && val !== '') || "Parent Req is required.",
      }
    },
    parentReq: {
      name: "parentReq",
      displayName: "Parent Req",
      type: "model",
      get typeDef() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parentReqId as ForeignKeyProperty },
      get principalKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.stringIdentityId as PrimaryKeyProperty },
      dontSerialize: true,
    },
    children: {
      name: "children",
      displayName: "Children",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parentId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.StringIdentity as ModelType & { name: "StringIdentity" }).props.parent as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Test = domain.types.Test = {
  name: "Test" as const,
  displayName: "Test",
  get displayProp() { return this.props.testName }, 
  type: "model",
  controllerRoute: "Test",
  get keyProp() { return this.props.testId }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    testId: {
      name: "testId",
      displayName: "Test Id",
      type: "number",
      role: "primaryKey",
      hidden: 3 as HiddenAreas,
    },
    complexModelId: {
      name: "complexModelId",
      displayName: "Complex Model Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get principalType() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      get navigationProp() { return (domain.types.Test as ModelType & { name: "Test" }).props.complexModel as ModelReferenceNavigationProperty },
      hidden: 3 as HiddenAreas,
      rules: {
        required: val => val != null || "Complex Model is required.",
      }
    },
    complexModel: {
      name: "complexModel",
      displayName: "Complex Model",
      type: "model",
      get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Test as ModelType & { name: "Test" }).props.complexModelId as ForeignKeyProperty },
      get principalKey() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.complexModelId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }).props.tests as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    testName: {
      name: "testName",
      displayName: "Test Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const ZipCode = domain.types.ZipCode = {
  name: "ZipCode" as const,
  displayName: "Zip Code",
  get displayProp() { return this.props.zip }, 
  type: "model",
  controllerRoute: "ZipCode",
  get keyProp() { return this.props.zip }, 
  behaviorFlags: 7 as BehaviorFlags,
  props: {
    zip: {
      name: "zip",
      displayName: "Zip",
      type: "string",
      role: "primaryKey",
      createOnly: true,
      rules: {
        required: val => (val != null && val !== '') || "Zip is required.",
      }
    },
    state: {
      name: "state",
      displayName: "State",
      type: "string",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const ExternalChild = domain.types.ExternalChild = {
  name: "ExternalChild" as const,
  displayName: "External Child",
  type: "object",
  props: {
    value: {
      name: "value",
      displayName: "Value",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Value is required.",
      }
    },
  },
}
export const ExternalChildAsInputOnly = domain.types.ExternalChildAsInputOnly = {
  name: "ExternalChildAsInputOnly" as const,
  displayName: "External Child As Input Only",
  type: "object",
  props: {
    value: {
      name: "value",
      displayName: "Value",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Value is required.",
      }
    },
    recursive: {
      name: "recursive",
      displayName: "Recursive",
      type: "object",
      get typeDef() { return (domain.types.ExternalParentAsInputOnly as ObjectType & { name: "ExternalParentAsInputOnly" }) },
      role: "value",
    },
  },
}
export const ExternalChildAsOutputOnly = domain.types.ExternalChildAsOutputOnly = {
  name: "ExternalChildAsOutputOnly" as const,
  displayName: "External Child As Output Only",
  type: "object",
  props: {
    value: {
      name: "value",
      displayName: "Value",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Value is required.",
      }
    },
    recursive: {
      name: "recursive",
      displayName: "Recursive",
      type: "object",
      get typeDef() { return (domain.types.ExternalParentAsOutputOnly as ObjectType & { name: "ExternalParentAsOutputOnly" }) },
      role: "value",
    },
  },
}
export const ExternalParent = domain.types.ExternalParent = {
  name: "ExternalParent" as const,
  displayName: "External Parent",
  type: "object",
  props: {
    valueArray: {
      name: "valueArray",
      displayName: "Value Array",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueNullableArray: {
      name: "valueNullableArray",
      displayName: "Value Nullable Array",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueArrayNullable: {
      name: "valueArrayNullable",
      displayName: "Value Array Nullable",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueICollection: {
      name: "valueICollection",
      displayName: "Value I Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueNullableICollection: {
      name: "valueNullableICollection",
      displayName: "Value Nullable I Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueICollectionNullable: {
      name: "valueICollectionNullable",
      displayName: "Value I Collection Nullable",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    valueList: {
      name: "valueList",
      displayName: "Value List",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "number",
      },
      role: "value",
    },
    stringICollection: {
      name: "stringICollection",
      displayName: "String I Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
    stringList: {
      name: "stringList",
      displayName: "String List",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "string",
      },
      role: "value",
    },
    refArray: {
      name: "refArray",
      displayName: "Ref Array",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refNullableArray: {
      name: "refNullableArray",
      displayName: "Ref Nullable Array",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refArrayNullable: {
      name: "refArrayNullable",
      displayName: "Ref Array Nullable",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refICollection: {
      name: "refICollection",
      displayName: "Ref I Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refNullableICollection: {
      name: "refNullableICollection",
      displayName: "Ref Nullable I Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refICollectionNullable: {
      name: "refICollectionNullable",
      displayName: "Ref I Collection Nullable",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refList: {
      name: "refList",
      displayName: "Ref List",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refNullableList: {
      name: "refNullableList",
      displayName: "Ref Nullable List",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
    refListNullable: {
      name: "refListNullable",
      displayName: "Ref List Nullable",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      },
      role: "value",
    },
  },
}
export const ExternalParentAsInputOnly = domain.types.ExternalParentAsInputOnly = {
  name: "ExternalParentAsInputOnly" as const,
  displayName: "External Parent As Input Only",
  type: "object",
  props: {
    child: {
      name: "child",
      displayName: "Child",
      type: "object",
      get typeDef() { return (domain.types.ExternalChildAsInputOnly as ObjectType & { name: "ExternalChildAsInputOnly" }) },
      role: "value",
    },
  },
}
export const ExternalParentAsOutputOnly = domain.types.ExternalParentAsOutputOnly = {
  name: "ExternalParentAsOutputOnly" as const,
  displayName: "External Parent As Output Only",
  type: "object",
  props: {
    child: {
      name: "child",
      displayName: "Child",
      type: "object",
      get typeDef() { return (domain.types.ExternalChildAsOutputOnly as ObjectType & { name: "ExternalChildAsOutputOnly" }) },
      role: "value",
    },
  },
}
export const ExternalTypeWithDtoProp = domain.types.ExternalTypeWithDtoProp = {
  name: "ExternalTypeWithDtoProp" as const,
  displayName: "External Type With Dto Prop",
  type: "object",
  props: {
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
      role: "value",
      dontSerialize: true,
    },
    cases: {
      name: "cases",
      displayName: "Cases",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
      },
      role: "value",
      dontSerialize: true,
    },
    casesList: {
      name: "casesList",
      displayName: "Cases List",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
      },
      role: "value",
      dontSerialize: true,
    },
    casesArray: {
      name: "casesArray",
      displayName: "Cases Array",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.CaseDtoStandalone as ModelType & { name: "CaseDtoStandalone" }) },
      },
      role: "value",
      dontSerialize: true,
    },
  },
}
export const InitRecordWithDefaultCtor = domain.types.InitRecordWithDefaultCtor = {
  name: "InitRecordWithDefaultCtor" as const,
  displayName: "Init Record With Default Ctor",
  type: "object",
  props: {
    string: {
      name: "string",
      displayName: "String",
      type: "string",
      role: "value",
    },
    num: {
      name: "num",
      displayName: "Num",
      type: "number",
      role: "value",
    },
    nestedRecord: {
      name: "nestedRecord",
      displayName: "Nested Record",
      type: "object",
      get typeDef() { return (domain.types.PositionalRecord as ObjectType & { name: "PositionalRecord" }) },
      role: "value",
    },
  },
}
export const InputOutputOnlyExternalTypeWithRequiredNonscalarProp = domain.types.InputOutputOnlyExternalTypeWithRequiredNonscalarProp = {
  name: "InputOutputOnlyExternalTypeWithRequiredNonscalarProp" as const,
  displayName: "Input Output Only External Type With Required Nonscalar Prop",
  type: "object",
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "value",
    },
    externalChild: {
      name: "externalChild",
      displayName: "External Child",
      type: "object",
      get typeDef() { return (domain.types.ExternalChild as ObjectType & { name: "ExternalChild" }) },
      role: "value",
    },
  },
}
export const Location = domain.types.Location = {
  name: "Location" as const,
  displayName: "Location",
  type: "object",
  props: {
    city: {
      name: "city",
      displayName: "City",
      type: "string",
      role: "value",
    },
    state: {
      name: "state",
      displayName: "State",
      type: "string",
      role: "value",
    },
    zip: {
      name: "zip",
      displayName: "Zip",
      type: "string",
      role: "value",
    },
  },
}
export const OutputOnlyExternalTypeWithoutDefaultCtor = domain.types.OutputOnlyExternalTypeWithoutDefaultCtor = {
  name: "OutputOnlyExternalTypeWithoutDefaultCtor" as const,
  displayName: "Output Only External Type Without Default Ctor",
  type: "object",
  props: {
    bar: {
      name: "bar",
      displayName: "Bar",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
    baz: {
      name: "baz",
      displayName: "Baz",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
  },
}
export const OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties = domain.types.OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties = {
  name: "OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties" as const,
  displayName: "Output Only External Type Without Default Ctor With Input Mappable Properties",
  type: "object",
  props: {
    message: {
      name: "message",
      displayName: "Message",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Message is required.",
      }
    },
  },
}
export const OutputOnlyExternalTypeWithRequiredEntityProp = domain.types.OutputOnlyExternalTypeWithRequiredEntityProp = {
  name: "OutputOnlyExternalTypeWithRequiredEntityProp" as const,
  displayName: "Output Only External Type With Required Entity Prop",
  type: "object",
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "value",
    },
    entity: {
      name: "entity",
      displayName: "Entity",
      type: "model",
      get typeDef() { return (domain.types.ComplexModel as ModelType & { name: "ComplexModel" }) },
      role: "value",
      dontSerialize: true,
    },
  },
}
export const PositionalRecord = domain.types.PositionalRecord = {
  name: "PositionalRecord" as const,
  displayName: "Positional Record",
  type: "object",
  props: {
    string: {
      name: "string",
      displayName: "String",
      type: "string",
      role: "value",
    },
    num: {
      name: "num",
      displayName: "Num",
      type: "number",
      role: "value",
    },
    date: {
      name: "date",
      displayName: "Date",
      type: "date",
      dateKind: "datetime",
      noOffset: true,
      role: "value",
    },
  },
}
export const ValidationTarget = domain.types.ValidationTarget = {
  name: "ValidationTarget" as const,
  displayName: "Validation Target",
  type: "object",
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "value",
    },
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "string",
      role: "value",
    },
    email: {
      name: "email",
      displayName: "Email",
      type: "string",
      subtype: "email",
      role: "value",
      rules: {
        minLength: val => !val || val.length >= 3 || "Email must be at least 3 characters.",
        maxLength: val => !val || val.length <= 150 || "Email may not be more than 150 characters.",
        email: val => !val || /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<> ()\[\]\\.,;:\s@"]+)*)|(".+ "))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(val.trim()) || "Custom email error message",
      }
    },
    number: {
      name: "number",
      displayName: "Fancy Number",
      type: "number",
      role: "value",
      rules: {
        required: val => val != null || "Fancy Number is required.",
        min: val => val == null || val >= 5 || "Fancy Number must be at least 5.",
        max: val => val == null || val <= 10 || "Fancy Number may not be more than 10.",
      }
    },
    optionalChild: {
      name: "optionalChild",
      displayName: "Optional Child",
      type: "object",
      get typeDef() { return (domain.types.ValidationTargetChild as ObjectType & { name: "ValidationTargetChild" }) },
      role: "value",
    },
    requiredChild: {
      name: "requiredChild",
      displayName: "Required Child",
      type: "object",
      get typeDef() { return (domain.types.ValidationTargetChild as ObjectType & { name: "ValidationTargetChild" }) },
      role: "value",
    },
    nonInputtableNonNullableChild: {
      name: "nonInputtableNonNullableChild",
      displayName: "Non Inputtable Non Nullable Child",
      type: "object",
      get typeDef() { return (domain.types.ValidationTargetChild as ObjectType & { name: "ValidationTargetChild" }) },
      role: "value",
      dontSerialize: true,
    },
  },
}
export const ValidationTargetChild = domain.types.ValidationTargetChild = {
  name: "ValidationTargetChild" as const,
  displayName: "Validation Target Child",
  type: "object",
  props: {
    string: {
      name: "string",
      displayName: "String",
      type: "string",
      subtype: "url",
      role: "value",
      rules: {
        url: val => !val || /^((https?|ftp):\/\/.)/.test(val) || "String must be a valid URL.",
      }
    },
    requiredVal: {
      name: "requiredVal",
      displayName: "Required Val",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Required Val is required.",
      }
    },
  },
}
export const WeatherData = domain.types.WeatherData = {
  name: "WeatherData" as const,
  displayName: "Weather Data",
  type: "object",
  props: {
    tempFahrenheit: {
      name: "tempFahrenheit",
      displayName: "Temp Fahrenheit",
      type: "number",
      role: "value",
    },
    humidity: {
      name: "humidity",
      displayName: "Humidity",
      type: "number",
      role: "value",
    },
    location: {
      name: "location",
      displayName: "Location",
      type: "object",
      get typeDef() { return (domain.types.Location as ObjectType & { name: "Location" }) },
      role: "value",
    },
  },
}
export const WeatherService = domain.services.WeatherService = {
  name: "WeatherService",
  displayName: "Weather Service",
  type: "service",
  controllerRoute: "WeatherService",
  methods: {
    getWeather: {
      name: "getWeather",
      displayName: "Get Weather",
      transportType: "item",
      httpMethod: "POST",
      params: {
        location: {
          name: "location",
          displayName: "Location",
          type: "object",
          get typeDef() { return (domain.types.Location as ObjectType & { name: "Location" }) },
          role: "value",
          rules: {
            required: val => val != null || "Location is required.",
          }
        },
        dateTime: {
          name: "dateTime",
          displayName: "Date Time",
          type: "date",
          dateKind: "datetime",
          role: "value",
        },
        conditions: {
          name: "conditions",
          displayName: "Conditions",
          type: "enum",
          get typeDef() { return SkyConditions },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.WeatherData as ObjectType & { name: "WeatherData" }) },
        role: "value",
      },
    },
  },
}

interface AppDomain extends Domain {
  enums: {
    EnumPkId: typeof EnumPkId
    Genders: typeof Genders
    SkyConditions: typeof SkyConditions
    Statuses: typeof Statuses
    Titles: typeof Titles
  }
  types: {
    AbstractImpl: typeof AbstractImpl
    AbstractModel: typeof AbstractModel
    Case: typeof Case
    CaseDtoStandalone: typeof CaseDtoStandalone
    CaseProduct: typeof CaseProduct
    Company: typeof Company
    ComplexModel: typeof ComplexModel
    ComplexModelDependent: typeof ComplexModelDependent
    EnumPk: typeof EnumPk
    ExternalChild: typeof ExternalChild
    ExternalChildAsInputOnly: typeof ExternalChildAsInputOnly
    ExternalChildAsOutputOnly: typeof ExternalChildAsOutputOnly
    ExternalParent: typeof ExternalParent
    ExternalParentAsInputOnly: typeof ExternalParentAsInputOnly
    ExternalParentAsOutputOnly: typeof ExternalParentAsOutputOnly
    ExternalTypeWithDtoProp: typeof ExternalTypeWithDtoProp
    InitRecordWithDefaultCtor: typeof InitRecordWithDefaultCtor
    InputOutputOnlyExternalTypeWithRequiredNonscalarProp: typeof InputOutputOnlyExternalTypeWithRequiredNonscalarProp
    Location: typeof Location
    OutputOnlyExternalTypeWithoutDefaultCtor: typeof OutputOnlyExternalTypeWithoutDefaultCtor
    OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties: typeof OutputOnlyExternalTypeWithoutDefaultCtorWithInputMappableProperties
    OutputOnlyExternalTypeWithRequiredEntityProp: typeof OutputOnlyExternalTypeWithRequiredEntityProp
    Person: typeof Person
    PositionalRecord: typeof PositionalRecord
    Product: typeof Product
    ReadOnlyEntityUsedAsMethodInput: typeof ReadOnlyEntityUsedAsMethodInput
    RecursiveHierarchy: typeof RecursiveHierarchy
    RequiredAndInitModel: typeof RequiredAndInitModel
    Sibling: typeof Sibling
    StandaloneReadonly: typeof StandaloneReadonly
    StandaloneReadWrite: typeof StandaloneReadWrite
    StringIdentity: typeof StringIdentity
    Test: typeof Test
    ValidationTarget: typeof ValidationTarget
    ValidationTargetChild: typeof ValidationTargetChild
    WeatherData: typeof WeatherData
    ZipCode: typeof ZipCode
  }
  services: {
    WeatherService: typeof WeatherService
  }
}

solidify(domain)

export default domain as unknown as AppDomain
