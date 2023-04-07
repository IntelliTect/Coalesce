import {
  Domain, getEnumMeta, solidify, ModelType, ObjectType,
  PrimitiveProperty, ForeignKeyProperty, PrimaryKeyProperty,
  ModelCollectionNavigationProperty, ModelReferenceNavigationProperty
} from 'coalesce-vue/lib/metadata'


const domain: Domain = { enums: {}, types: {}, services: {} }
export const Genders = domain.enums.Genders = {
  name: "Genders",
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
  name: "SkyConditions",
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
  name: "Statuses",
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
  },
  {
    value: 3,
    strValue: "ClosedNoSolution",
    displayName: "Closed, No Solution",
    description: "The case was closed without being solved.",
  },
  {
    value: 4,
    strValue: "Cancelled",
    displayName: "Cancelled",
  },
  ]),
}
export const Titles = domain.enums.Titles = {
  name: "Titles",
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
export const Case = domain.types.Case = {
  name: "Case",
  displayName: "Case",
  get displayProp() { return this.props.title }, 
  type: "model",
  controllerRoute: "Case",
  get keyProp() { return this.props.caseKey }, 
  behaviorFlags: 7,
  props: {
    caseKey: {
      name: "caseKey",
      displayName: "Case Key",
      type: "number",
      role: "primaryKey",
      hidden: 3,
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
      description: "User-provided description of the issue",
      type: "string",
      subtype: "multiline",
      role: "value",
    },
    openedAt: {
      name: "openedAt",
      displayName: "Opened At",
      description: "Date and time when the case was opened",
      type: "date",
      dateKind: "datetime",
      role: "value",
    },
    assignedToId: {
      name: "assignedToId",
      displayName: "Assigned To Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType) },
      get navigationProp() { return (domain.types.Case as ModelType).props.assignedTo as ModelReferenceNavigationProperty },
      hidden: 3,
    },
    assignedTo: {
      name: "assignedTo",
      displayName: "Assigned To",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.assignedToId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType).props.casesAssigned as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    reportedById: {
      name: "reportedById",
      displayName: "Reported By Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Person as ModelType) },
      get navigationProp() { return (domain.types.Case as ModelType).props.reportedBy as ModelReferenceNavigationProperty },
      hidden: 3,
    },
    reportedBy: {
      name: "reportedBy",
      displayName: "Reported By",
      description: "Person who originally reported the case",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType).props.casesReported as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    attachmentSize: {
      name: "attachmentSize",
      displayName: "Attachment Size",
      type: "number",
      role: "value",
      dontSerialize: true,
    },
    attachmentName: {
      name: "attachmentName",
      displayName: "Attachment Name",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
    attachmentType: {
      name: "attachmentType",
      displayName: "Attachment Type",
      type: "string",
      role: "value",
    },
    attachmentHash: {
      name: "attachmentHash",
      displayName: "Attachment Hash",
      type: "binary",
      base64: true,
      role: "value",
      dontSerialize: true,
    },
    severity: {
      name: "severity",
      displayName: "Severity",
      type: "string",
      subtype: "color",
      role: "value",
    },
    status: {
      name: "status",
      displayName: "Status",
      type: "enum",
      get typeDef() { return domain.enums.Statuses },
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
        get typeDef() { return (domain.types.CaseProduct as ModelType) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.CaseProduct as ModelType).props.case as ModelReferenceNavigationProperty },
      manyToMany: {
        name: "products",
        displayName: "Products",
        get typeDef() { return (domain.types.Product as ModelType) },
        get farForeignKey() { return (domain.types.CaseProduct as ModelType).props.productId as ForeignKeyProperty },
        get farNavigationProp() { return (domain.types.CaseProduct as ModelType).props.product as ModelReferenceNavigationProperty },
        get nearForeignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as ForeignKeyProperty },
        get nearNavigationProp() { return (domain.types.CaseProduct as ModelType).props.case as ModelReferenceNavigationProperty },
      },
      dontSerialize: true,
    },
    devTeamAssignedId: {
      name: "devTeamAssignedId",
      displayName: "Dev Team Assigned Id",
      type: "number",
      role: "value",
    },
    devTeamAssigned: {
      name: "devTeamAssigned",
      displayName: "Dev Team Assigned",
      type: "object",
      get typeDef() { return (domain.types.DevTeam as ObjectType) },
      role: "value",
      dontSerialize: true,
    },
    duration: {
      name: "duration",
      displayName: "Duration",
      // Type not supported natively by Coalesce - falling back to unknown.
      type: "unknown",
      role: "value",
    },
  },
  methods: {
    getCaseTitles: {
      name: "getCaseTitles",
      displayName: "Get Case Titles",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        search: {
          name: "search",
          displayName: "Search",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Search is required.",
          }
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
    getSomeCases: {
      name: "getSomeCases",
      displayName: "Get Some Cases",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "collection",
        itemType: {
          name: "$collectionItem",
          displayName: "",
          role: "value",
          type: "model",
          get typeDef() { return (domain.types.Case as ModelType) },
        },
        role: "value",
      },
    },
    getAllOpenCasesCount: {
      name: "getAllOpenCasesCount",
      displayName: "Get All Open Cases Count",
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
    randomizeDatesAndStatus: {
      name: "randomizeDatesAndStatus",
      displayName: "Randomize Dates And Status",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "void",
        role: "value",
      },
    },
    uploadImage: {
      name: "uploadImage",
      displayName: "Upload Image",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.caseKey },
        },
        file: {
          name: "file",
          displayName: "File",
          type: "file",
          role: "value",
          rules: {
            required: val => val != null || "File is required.",
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
    downloadImage: {
      name: "downloadImage",
      displayName: "Download Image",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.caseKey },
        },
        etag: {
          name: "etag",
          displayName: "Etag",
          type: "binary",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.attachmentHash },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "file",
        role: "value",
      },
    },
    uploadAndDownload: {
      name: "uploadAndDownload",
      displayName: "Upload And Download",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.caseKey },
        },
        file: {
          name: "file",
          displayName: "File",
          type: "file",
          role: "value",
          rules: {
            required: val => val != null || "File is required.",
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
    uploadImages: {
      name: "uploadImages",
      displayName: "Upload Images",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.caseKey },
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
          rules: {
            required: val => val != null || "Files is required.",
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
    uploadByteArray: {
      name: "uploadByteArray",
      displayName: "Upload Byte Array",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Case as ModelType).props.caseKey },
        },
        file: {
          name: "file",
          displayName: "File",
          type: "binary",
          role: "value",
          rules: {
            required: val => val != null || "File is required.",
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
    getCaseSummary: {
      name: "getCaseSummary",
      displayName: "Get Case Summary",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.CaseSummary as ObjectType) },
        role: "value",
      },
    },
  },
  dataSources: {
    allOpenCases: {
      type: "dataSource",
      name: "AllOpenCases",
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
export const CaseDto = domain.types.CaseDto = {
  name: "CaseDto",
  displayName: "Case Dto",
  get displayProp() { return this.props.caseId }, 
  type: "model",
  controllerRoute: "CaseDto",
  get keyProp() { return this.props.caseId }, 
  behaviorFlags: 7,
  props: {
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    title: {
      name: "title",
      displayName: "Title",
      type: "string",
      role: "value",
    },
    assignedToName: {
      name: "assignedToName",
      displayName: "Assigned To Name",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
  },
  methods: {
    asyncMethodOnIClassDto: {
      name: "asyncMethodOnIClassDto",
      displayName: "Async Method On I Class Dto",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.CaseDto as ModelType).props.caseId },
        },
        input: {
          name: "input",
          displayName: "Input",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Input is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
  },
  dataSources: {
    caseDtoSource: {
      type: "dataSource",
      name: "CaseDtoSource",
      displayName: "Case Dto Source",
      props: {
      },
    },
  },
}
export const CaseDtoStandalone = domain.types.CaseDtoStandalone = {
  name: "CaseDtoStandalone",
  displayName: "Case Dto Standalone",
  get displayProp() { return this.props.caseId }, 
  type: "model",
  controllerRoute: "CaseDtoStandalone",
  get keyProp() { return this.props.caseId }, 
  behaviorFlags: 7,
  props: {
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
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
  name: "CaseProduct",
  displayName: "Case Product",
  get displayProp() { return this.props.caseProductId }, 
  type: "model",
  controllerRoute: "CaseProduct",
  get keyProp() { return this.props.caseProductId }, 
  behaviorFlags: 7,
  props: {
    caseProductId: {
      name: "caseProductId",
      displayName: "Case Product Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimaryKeyProperty },
      get principalType() { return (domain.types.Case as ModelType) },
      get navigationProp() { return (domain.types.CaseProduct as ModelType).props.case as ModelReferenceNavigationProperty },
      hidden: 3,
      rules: {
        required: val => val != null || "Case is required.",
      }
    },
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      get typeDef() { return (domain.types.Case as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType).props.caseProducts as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Product as ModelType).props.productId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Product as ModelType) },
      get navigationProp() { return (domain.types.CaseProduct as ModelType).props.product as ModelReferenceNavigationProperty },
      hidden: 3,
      rules: {
        required: val => val != null || "Product is required.",
      }
    },
    product: {
      name: "product",
      displayName: "Product",
      type: "model",
      get typeDef() { return (domain.types.Product as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.productId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Product as ModelType).props.productId as PrimaryKeyProperty },
      dontSerialize: true,
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const Company = domain.types.Company = {
  name: "Company",
  displayName: "Company",
  get displayProp() { return this.props.altName }, 
  type: "model",
  controllerRoute: "Company",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
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
      createOnly: true,
      rules: {
        required: val => (val != null && val !== '') || "Address1 is required.",
      }
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
      hidden: 1,
    },
    state: {
      name: "state",
      displayName: "State",
      type: "string",
      role: "value",
      hidden: 2,
    },
    zipCode: {
      name: "zipCode",
      displayName: "Zip Code",
      type: "string",
      role: "value",
      hidden: 3,
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
      rules: {
        url: val => !val || /^((http(s)|ftp):\/\/.)/.test(val) || "Website Url must be a valid URL.",
      }
    },
    logoUrl: {
      name: "logoUrl",
      displayName: "Logo Url",
      type: "string",
      subtype: "url-image",
      role: "value",
    },
    isDeleted: {
      name: "isDeleted",
      displayName: "Is Deleted",
      type: "boolean",
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
        get typeDef() { return (domain.types.Person as ModelType) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType).props.company as ModelReferenceNavigationProperty },
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
    conflictingParameterNames: {
      name: "conflictingParameterNames",
      displayName: "Conflicting Parameter Names",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Company as ModelType).props.id },
        },
        companyParam: {
          name: "companyParam",
          displayName: "Company Param",
          type: "model",
          get typeDef() { return (domain.types.Company as ModelType) },
          role: "value",
          rules: {
            required: val => val != null || "Company Param is required.",
          }
        },
        name: {
          name: "name",
          displayName: "Name",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Name is required.",
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
    getCertainItems: {
      name: "getCertainItems",
      displayName: "Get Certain Items",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        isDeleted: {
          name: "isDeleted",
          displayName: "Is Deleted",
          type: "boolean",
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
          type: "model",
          get typeDef() { return (domain.types.Company as ModelType) },
        },
        role: "value",
      },
    },
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource",
      displayName: "Default Source",
      isDefault: true,
      props: {
      },
    },
  },
}
export const Log = domain.types.Log = {
  name: "Log",
  displayName: "Log",
  get displayProp() { return this.props.logId }, 
  type: "model",
  controllerRoute: "Log",
  get keyProp() { return this.props.logId }, 
  behaviorFlags: 0,
  props: {
    logId: {
      name: "logId",
      displayName: "Log Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    level: {
      name: "level",
      displayName: "Level",
      type: "string",
      role: "value",
    },
    message: {
      name: "message",
      displayName: "Message",
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
  name: "Person",
  displayName: "Person",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Person",
  get keyProp() { return this.props.personId }, 
  behaviorFlags: 7,
  props: {
    title: {
      name: "title",
      displayName: "Title",
      type: "enum",
      get typeDef() { return domain.enums.Titles },
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
      hidden: 3,
    },
    email: {
      name: "email",
      displayName: "Email",
      type: "string",
      subtype: "email",
      role: "value",
      rules: {
        email: val => !val || /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<> ()\[\]\\.,;:\s@"]+)*)|(".+ "))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(val.trim()) || "Email must be a valid email address.",
      }
    },
    gender: {
      name: "gender",
      displayName: "Gender",
      type: "enum",
      get typeDef() { return domain.enums.Genders },
      role: "value",
    },
    height: {
      name: "height",
      displayName: "Height",
      type: "number",
      role: "value",
      rules: {
        min: val => val == null || val >= 5 || "Height must be at least 5.",
        max: val => val == null || val <= 10 || "Height may not be more than 10.",
      }
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
        get typeDef() { return (domain.types.Case as ModelType) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.assignedToId as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType).props.assignedTo as ModelReferenceNavigationProperty },
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
        get typeDef() { return (domain.types.Case as ModelType) },
      },
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById as ForeignKeyProperty },
      get inverseNavigation() { return (domain.types.Case as ModelType).props.reportedBy as ModelReferenceNavigationProperty },
      dontSerialize: true,
    },
    birthDate: {
      name: "birthDate",
      displayName: "Birth Date",
      type: "date",
      dateKind: "date",
      noOffset: true,
      role: "value",
    },
    lastBath: {
      name: "lastBath",
      displayName: "Last Bath",
      type: "date",
      dateKind: "datetime",
      noOffset: true,
      role: "value",
      hidden: 3,
    },
    nextUpgrade: {
      name: "nextUpgrade",
      displayName: "Next Upgrade",
      type: "date",
      dateKind: "datetime",
      role: "value",
      hidden: 3,
    },
    personStats: {
      name: "personStats",
      displayName: "Person Stats",
      type: "object",
      get typeDef() { return (domain.types.PersonStats as ObjectType) },
      role: "value",
      hidden: 3,
      dontSerialize: true,
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
      get principalKey() { return (domain.types.Company as ModelType).props.id as PrimaryKeyProperty },
      get principalType() { return (domain.types.Company as ModelType) },
      get navigationProp() { return (domain.types.Person as ModelType).props.company as ModelReferenceNavigationProperty },
      hidden: 3,
      rules: {
        required: val => val != null || "Company is required.",
      }
    },
    company: {
      name: "company",
      displayName: "Company",
      type: "model",
      get typeDef() { return (domain.types.Company as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Company as ModelType).props.id as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Company as ModelType).props.employees as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    arbitraryCollectionOfStrings: {
      name: "arbitraryCollectionOfStrings",
      displayName: "Arbitrary Collection Of Strings",
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
          get source() { return (domain.types.Person as ModelType).props.personId },
        },
        name: {
          name: "name",
          displayName: "Name",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Name is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType) },
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
          get source() { return (domain.types.Person as ModelType).props.personId },
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
          rules: {
            min: val => val == null || val >= 0 || "Number Two must be at least 0.",
            max: val => val == null || val <= 10000 || "Number Two may not be more than 10000.",
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
    getBirthdate: {
      name: "getBirthdate",
      displayName: "Get Birthdate",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType).props.personId },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "date",
        dateKind: "datetime",
        noOffset: true,
        role: "value",
      },
    },
    personCount: {
      name: "personCount",
      displayName: "Person Count",
      transportType: "item",
      httpMethod: "GET",
      isStatic: true,
      params: {
        lastNameStartsWith: {
          name: "lastNameStartsWith",
          displayName: "Last Name Starts With",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Last Name Starts With is required.",
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
    fullNameAndAge: {
      name: "fullNameAndAge",
      displayName: "Full Name And Age",
      transportType: "item",
      httpMethod: "GET",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType).props.personId },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
    removePersonById: {
      name: "removePersonById",
      displayName: "Remove Person By Id",
      transportType: "item",
      httpMethod: "DELETE",
      isStatic: true,
      params: {
        id: {
          name: "id",
          displayName: "Id",
          type: "number",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "boolean",
        role: "value",
      },
    },
    obfuscateEmail: {
      name: "obfuscateEmail",
      displayName: "Obfuscate Email",
      transportType: "item",
      httpMethod: "PUT",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType).props.personId },
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "string",
        role: "value",
      },
    },
    changeFirstName: {
      name: "changeFirstName",
      displayName: "Change First Name",
      transportType: "item",
      httpMethod: "PATCH",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          type: "number",
          role: "value",
          get source() { return (domain.types.Person as ModelType).props.personId },
        },
        firstName: {
          name: "firstName",
          displayName: "First Name",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "First Name is required.",
          }
        },
        title: {
          name: "title",
          displayName: "Title",
          type: "enum",
          get typeDef() { return domain.enums.Titles },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType) },
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
      httpMethod: "POST",
      isStatic: true,
      params: {
        characters: {
          name: "characters",
          displayName: "Characters",
          type: "string",
          role: "value",
          rules: {
            required: val => (val != null && val !== '') || "Characters is required.",
          }
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
    methodWithStringArrayParameter: {
      name: "methodWithStringArrayParameter",
      displayName: "Method With String Array Parameter",
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
          rules: {
            required: val => val != null || "Strings is required.",
          }
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
    methodWithEntityParameter: {
      name: "methodWithEntityParameter",
      displayName: "Method With Entity Parameter",
      transportType: "item",
      httpMethod: "POST",
      isStatic: true,
      params: {
        person: {
          name: "person",
          displayName: "Person",
          type: "model",
          get typeDef() { return (domain.types.Person as ModelType) },
          role: "value",
          rules: {
            required: val => val != null || "Person is required.",
          }
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType) },
        role: "value",
      },
    },
    searchPeople: {
      name: "searchPeople",
      displayName: "Search People",
      transportType: "list",
      httpMethod: "POST",
      isStatic: true,
      params: {
        criteria: {
          name: "criteria",
          displayName: "Criteria",
          type: "object",
          get typeDef() { return (domain.types.PersonCriteria as ObjectType) },
          role: "value",
          rules: {
            required: val => val != null || "Criteria is required.",
          }
        },
        page: {
          name: "page",
          displayName: "Page",
          type: "number",
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
          type: "model",
          get typeDef() { return (domain.types.Person as ModelType) },
        },
        role: "value",
      },
    },
  },
  dataSources: {
    bOrCPeople: {
      type: "dataSource",
      name: "BOrCPeople",
      displayName: "B Or C People",
      props: {
      },
    },
    namesStartingWithAWithCases: {
      type: "dataSource",
      name: "NamesStartingWithAWithCases",
      displayName: "Names Starting With A With Cases",
      props: {
      },
    },
    withoutCases: {
      type: "dataSource",
      name: "WithoutCases",
      displayName: "Without Cases",
      isDefault: true,
      props: {
      },
    },
  },
}
export const Product = domain.types.Product = {
  name: "Product",
  displayName: "Product",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Product",
  get keyProp() { return this.props.productId }, 
  behaviorFlags: 7,
  props: {
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    details: {
      name: "details",
      displayName: "Details",
      type: "object",
      get typeDef() { return (domain.types.ProductDetails as ObjectType) },
      role: "value",
      dontSerialize: true,
    },
    uniqueId: {
      name: "uniqueId",
      displayName: "Unique Id",
      type: "string",
      subtype: "password",
      role: "value",
      rules: {
        pattern: val => !val || /^\s*[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?\s*$/.test(val) || "Unique Id does not match expected format.",
      }
    },
    unknown: {
      name: "unknown",
      displayName: "Unknown",
      // Type not supported natively by Coalesce - falling back to unknown.
      type: "unknown",
      role: "value",
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const StandaloneReadCreate = domain.types.StandaloneReadCreate = {
  name: "StandaloneReadCreate",
  displayName: "Standalone Read Create",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "StandaloneReadCreate",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 5,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Name is required.",
      }
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
      name: "DefaultSource",
      displayName: "Default Source",
      props: {
      },
    },
  },
}
export const StandaloneReadonly = domain.types.StandaloneReadonly = {
  name: "StandaloneReadonly",
  displayName: "Standalone Readonly",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "StandaloneReadonly",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 0,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Name is required.",
      }
    },
    description: {
      name: "description",
      displayName: "Description",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Description is required.",
      }
    },
  },
  methods: {
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "DefaultSource",
      displayName: "Default Source",
      props: {
      },
    },
  },
}
export const StandaloneReadWrite = domain.types.StandaloneReadWrite = {
  name: "StandaloneReadWrite",
  displayName: "Standalone Read Write",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "StandaloneReadWrite",
  get keyProp() { return this.props.id }, 
  behaviorFlags: 7,
  props: {
    id: {
      name: "id",
      displayName: "Id",
      type: "number",
      role: "primaryKey",
      hidden: 3,
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
      rules: {
        required: val => (val != null && val !== '') || "Name is required.",
      }
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
      name: "DefaultSource",
      displayName: "Default Source",
      props: {
      },
    },
  },
}
export const ZipCode = domain.types.ZipCode = {
  name: "ZipCode",
  displayName: "Zip Code",
  get displayProp() { return this.props.zip }, 
  type: "model",
  controllerRoute: "ZipCode",
  get keyProp() { return this.props.zip }, 
  behaviorFlags: 7,
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
      rules: {
        required: val => (val != null && val !== '') || "State is required.",
      }
    },
  },
  methods: {
  },
  dataSources: {
  },
}
export const CaseSummary = domain.types.CaseSummary = {
  name: "CaseSummary",
  displayName: "Case Summary",
  get displayProp() { return this.props.description }, 
  type: "object",
  props: {
    caseSummaryId: {
      name: "caseSummaryId",
      displayName: "Case Summary Id",
      type: "number",
      role: "value",
    },
    openCases: {
      name: "openCases",
      displayName: "Open Cases",
      type: "number",
      role: "value",
    },
    caseCount: {
      name: "caseCount",
      displayName: "Case Count",
      type: "number",
      role: "value",
    },
    closeCases: {
      name: "closeCases",
      displayName: "Close Cases",
      type: "number",
      role: "value",
    },
    description: {
      name: "description",
      displayName: "Description",
      type: "string",
      role: "value",
    },
    testDict: {
      name: "testDict",
      displayName: "Test Dict",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        // Type not supported natively by Coalesce - falling back to unknown.
        type: "unknown",
      },
      role: "value",
    },
  },
}
export const DevTeam = domain.types.DevTeam = {
  name: "DevTeam",
  displayName: "Dev Team",
  get displayProp() { return this.props.name }, 
  type: "object",
  props: {
    devTeamId: {
      name: "devTeamId",
      displayName: "Dev Team Id",
      type: "number",
      role: "value",
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
  },
}
export const Location = domain.types.Location = {
  name: "Location",
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
export const PersonCriteria = domain.types.PersonCriteria = {
  name: "PersonCriteria",
  displayName: "Person Criteria",
  get displayProp() { return this.props.name }, 
  type: "object",
  props: {
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    birthdayMonth: {
      name: "birthdayMonth",
      displayName: "Birthday Month",
      type: "number",
      role: "value",
    },
    emailDomain: {
      name: "emailDomain",
      displayName: "Email Domain",
      type: "string",
      role: "value",
    },
  },
}
export const PersonLocation = domain.types.PersonLocation = {
  name: "PersonLocation",
  displayName: "Person Location",
  type: "object",
  props: {
    latitude: {
      name: "latitude",
      displayName: "Latitude",
      type: "number",
      role: "value",
    },
    longitude: {
      name: "longitude",
      displayName: "Longitude",
      type: "number",
      role: "value",
    },
  },
}
export const PersonStats = domain.types.PersonStats = {
  name: "PersonStats",
  displayName: "Person Stats",
  get displayProp() { return this.props.name }, 
  type: "object",
  props: {
    height: {
      name: "height",
      displayName: "Height",
      type: "number",
      role: "value",
    },
    weight: {
      name: "weight",
      displayName: "Weight",
      type: "number",
      role: "value",
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    nullableValueTypeCollection: {
      name: "nullableValueTypeCollection",
      displayName: "Nullable Value Type Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "date",
        dateKind: "datetime",
      },
      role: "value",
    },
    valueTypeCollection: {
      name: "valueTypeCollection",
      displayName: "Value Type Collection",
      type: "collection",
      itemType: {
        name: "$collectionItem",
        displayName: "",
        role: "value",
        type: "date",
        dateKind: "datetime",
      },
      role: "value",
    },
    personLocation: {
      name: "personLocation",
      displayName: "Person Location",
      type: "object",
      get typeDef() { return (domain.types.PersonLocation as ObjectType) },
      role: "value",
    },
  },
}
export const ProductDetails = domain.types.ProductDetails = {
  name: "ProductDetails",
  displayName: "Product Details",
  get displayProp() { return this.props.manufacturingAddress }, 
  type: "object",
  props: {
    manufacturingAddress: {
      name: "manufacturingAddress",
      displayName: "Manufacturing Address",
      type: "object",
      get typeDef() { return (domain.types.StreetAddress as ObjectType) },
      role: "value",
    },
    companyHqAddress: {
      name: "companyHqAddress",
      displayName: "Company Hq Address",
      type: "object",
      get typeDef() { return (domain.types.StreetAddress as ObjectType) },
      role: "value",
    },
  },
}
export const StreetAddress = domain.types.StreetAddress = {
  name: "StreetAddress",
  displayName: "Street Address",
  get displayProp() { return this.props.address }, 
  type: "object",
  props: {
    address: {
      name: "address",
      displayName: "Address",
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
    postalCode: {
      name: "postalCode",
      displayName: "Postal Code",
      type: "string",
      role: "value",
    },
  },
}
export const WeatherData = domain.types.WeatherData = {
  name: "WeatherData",
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
      get typeDef() { return (domain.types.Location as ObjectType) },
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
          get typeDef() { return (domain.types.Location as ObjectType) },
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
          get typeDef() { return domain.enums.SkyConditions },
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        type: "object",
        get typeDef() { return (domain.types.WeatherData as ObjectType) },
        role: "value",
      },
    },
  },
}

interface AppDomain extends Domain {
  enums: {
    Genders: typeof Genders
    SkyConditions: typeof SkyConditions
    Statuses: typeof Statuses
    Titles: typeof Titles
  }
  types: {
    Case: typeof Case
    CaseDto: typeof CaseDto
    CaseDtoStandalone: typeof CaseDtoStandalone
    CaseProduct: typeof CaseProduct
    CaseSummary: typeof CaseSummary
    Company: typeof Company
    DevTeam: typeof DevTeam
    Location: typeof Location
    Log: typeof Log
    Person: typeof Person
    PersonCriteria: typeof PersonCriteria
    PersonLocation: typeof PersonLocation
    PersonStats: typeof PersonStats
    Product: typeof Product
    ProductDetails: typeof ProductDetails
    StandaloneReadCreate: typeof StandaloneReadCreate
    StandaloneReadonly: typeof StandaloneReadonly
    StandaloneReadWrite: typeof StandaloneReadWrite
    StreetAddress: typeof StreetAddress
    WeatherData: typeof WeatherData
    ZipCode: typeof ZipCode
  }
  services: {
    WeatherService: typeof WeatherService
  }
}

solidify(domain)

export default domain as unknown as AppDomain
