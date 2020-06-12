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
    { value: 0, strValue: 'NonSpecified', displayName: 'Non Specified' },
    { value: 1, strValue: 'Male', displayName: 'Male' },
    { value: 2, strValue: 'Female', displayName: 'Female' },
  ]),
}
export const SkyConditions = domain.enums.SkyConditions = {
  name: "SkyConditions",
  displayName: "Sky Conditions",
  type: "enum",
  ...getEnumMeta<"Cloudy"|"PartyCloudy"|"Sunny">([
    { value: 0, strValue: 'Cloudy', displayName: 'Cloudy' },
    { value: 1, strValue: 'PartyCloudy', displayName: 'Party Cloudy' },
    { value: 2, strValue: 'Sunny', displayName: 'Sunny' },
  ]),
}
export const Statuses = domain.enums.Statuses = {
  name: "Statuses",
  displayName: "Statuses",
  type: "enum",
  ...getEnumMeta<"Open"|"InProgress"|"Resolved"|"ClosedNoSolution"|"Cancelled">([
    { value: 0, strValue: 'Open', displayName: 'Open' },
    { value: 1, strValue: 'InProgress', displayName: 'In Progress' },
    { value: 2, strValue: 'Resolved', displayName: 'Resolved' },
    { value: 3, strValue: 'ClosedNoSolution', displayName: 'Closed No Solution' },
    { value: 4, strValue: 'Cancelled', displayName: 'Cancelled' },
  ]),
}
export const Titles = domain.enums.Titles = {
  name: "Titles",
  displayName: "Titles",
  type: "enum",
  ...getEnumMeta<"Mr"|"Ms"|"Mrs"|"Miss">([
    { value: 0, strValue: 'Mr', displayName: 'Mr' },
    { value: 1, strValue: 'Ms', displayName: 'Ms' },
    { value: 2, strValue: 'Mrs', displayName: 'Mrs' },
    { value: 4, strValue: 'Miss', displayName: 'Miss' },
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
      role: "value",
    },
    openedAt: {
      name: "openedAt",
      displayName: "Opened At",
      dateKind: "datetime",
      type: "date",
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
    },
    reportedBy: {
      name: "reportedBy",
      displayName: "Reported By",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById as ForeignKeyProperty },
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimaryKeyProperty },
      get inverseNavigation() { return (domain.types.Person as ModelType).props.casesReported as ModelCollectionNavigationProperty },
      dontSerialize: true,
    },
    imageName: {
      name: "imageName",
      displayName: "Image Name",
      type: "string",
      role: "value",
      dontSerialize: true,
    },
    imageSize: {
      name: "imageSize",
      displayName: "Image Size",
      type: "number",
      role: "value",
    },
    imageHash: {
      name: "imageHash",
      displayName: "Image Hash",
      type: "string",
      role: "value",
    },
    attachmentName: {
      name: "attachmentName",
      displayName: "Attachment Name",
      type: "string",
      role: "value",
    },
    severity: {
      name: "severity",
      displayName: "Severity",
      type: "string",
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
      // Type not supported natively by Coalesce - falling back to string.
      type: "string",
      role: "value",
    },
  },
  methods: {
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
    uploadAttachment: {
      name: "uploadAttachment",
      displayName: "Upload Attachment",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          role: "value",
          type: "number",
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
          dateKind: "datetime",
          type: "date",
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
          role: "value",
          type: "number",
        },
        input: {
          name: "input",
          displayName: "Input",
          type: "string",
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
    },
    caseId: {
      name: "caseId",
      displayName: "Case Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimaryKeyProperty },
      get principalType() { return (domain.types.Case as ModelType) },
      get navigationProp() { return (domain.types.CaseProduct as ModelType).props.case as ModelReferenceNavigationProperty },
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
  get keyProp() { return this.props.companyId }, 
  behaviorFlags: 6,
  props: {
    companyId: {
      name: "companyId",
      displayName: "Company Id",
      type: "number",
      role: "primaryKey",
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
      props: {
      },
    },
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
    personId: {
      name: "personId",
      displayName: "Person Id",
      type: "number",
      role: "primaryKey",
    },
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
    },
    lastName: {
      name: "lastName",
      displayName: "Last Name",
      type: "string",
      role: "value",
    },
    email: {
      name: "email",
      displayName: "Email",
      type: "string",
      role: "value",
    },
    gender: {
      name: "gender",
      displayName: "Gender",
      type: "enum",
      get typeDef() { return domain.enums.Genders },
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
      dateKind: "date",
      noOffset: true,
      type: "date",
      role: "value",
    },
    lastBath: {
      name: "lastBath",
      displayName: "Last Bath",
      dateKind: "datetime",
      noOffset: true,
      type: "date",
      role: "value",
      hidden: 3,
    },
    nextUpgrade: {
      name: "nextUpgrade",
      displayName: "Next Upgrade",
      dateKind: "datetime",
      type: "date",
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
      get principalKey() { return (domain.types.Company as ModelType).props.companyId as PrimaryKeyProperty },
      get principalType() { return (domain.types.Company as ModelType) },
      get navigationProp() { return (domain.types.Person as ModelType).props.company as ModelReferenceNavigationProperty },
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
      get principalKey() { return (domain.types.Company as ModelType).props.companyId as PrimaryKeyProperty },
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
          role: "value",
          type: "number",
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
          role: "value",
          type: "number",
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
    getBirthdate: {
      name: "getBirthdate",
      displayName: "Get Birthdate",
      transportType: "item",
      httpMethod: "POST",
      params: {
        id: {
          name: "id",
          displayName: "Primary Key",
          role: "value",
          type: "number",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        dateKind: "datetime",
        noOffset: true,
        type: "date",
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
          role: "value",
          type: "number",
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
          role: "value",
          type: "number",
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
          role: "value",
          type: "number",
        },
        firstName: {
          name: "firstName",
          displayName: "First Name",
          type: "string",
          role: "value",
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
      role: "value",
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
      dontSerialize: true,
    },
    companyHqAddress: {
      name: "companyHqAddress",
      displayName: "Company Hq Address",
      type: "object",
      get typeDef() { return (domain.types.StreetAddress as ObjectType) },
      role: "value",
      dontSerialize: true,
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
      dontSerialize: true,
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
        },
        dateTime: {
          name: "dateTime",
          displayName: "Date Time",
          dateKind: "datetime",
          type: "date",
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
    getWeatherAsync: {
      name: "getWeatherAsync",
      displayName: "Get Weather Async",
      transportType: "item",
      httpMethod: "POST",
      params: {
        location: {
          name: "location",
          displayName: "Location",
          type: "object",
          get typeDef() { return (domain.types.Location as ObjectType) },
          role: "value",
        },
        dateTime: {
          name: "dateTime",
          displayName: "Date Time",
          dateKind: "datetime",
          type: "date",
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
    CaseProduct: typeof CaseProduct
    CaseSummary: typeof CaseSummary
    Company: typeof Company
    DevTeam: typeof DevTeam
    Location: typeof Location
    Person: typeof Person
    PersonCriteria: typeof PersonCriteria
    PersonStats: typeof PersonStats
    Product: typeof Product
    ProductDetails: typeof ProductDetails
    StreetAddress: typeof StreetAddress
    WeatherData: typeof WeatherData
  }
  services: {
    WeatherService: typeof WeatherService
  }
}

solidify(domain)

export default domain as AppDomain
