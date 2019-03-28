import {
  Domain, getEnumMeta, ModelType, ObjectType,
  PrimitiveProperty, ModelReferenceNavigationProperty, ForeignKeyProperty, PrimaryKeyProperty
} from 'coalesce-vue/lib/metadata'


const domain: Domain = { enums: {}, types: {}, services: {} }
export const Genders = domain.enums.Genders = {
  name: "genders",
  displayName: "Genders",
  type: "enum",
  ...getEnumMeta<"NonSpecified"|"Male"|"Female">([
    { value: 0, strValue: 'NonSpecified', displayName: 'Non Specified' },
    { value: 1, strValue: 'Male', displayName: 'Male' },
    { value: 2, strValue: 'Female', displayName: 'Female' },
  ]),
}
export const Statuses = domain.enums.Statuses = {
  name: "statuses",
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
  name: "titles",
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
  name: "case",
  displayName: "Case",
  get displayProp() { return this.props.title }, 
  type: "model",
  controllerRoute: "Case",
  get keyProp() { return this.props.caseKey }, 
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
      dontSerialize: true,
    },
    attachment: {
      name: "attachment",
      displayName: "Attachment",
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
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "collection",
        itemType: {
          name: "$collectionItem",
          displayName: "",
          role: "value",
          type: "model",
          get typeDef() { return (domain.types.Case as ModelType) },
        },
      },
    },
    getAllOpenCasesCount: {
      name: "getAllOpenCasesCount",
      displayName: "Get All Open Cases Count",
      transportType: "item",
      httpMethod: "POST",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "number",
      },
    },
    randomizeDatesAndStatus: {
      name: "randomizeDatesAndStatus",
      displayName: "Randomize Dates And Status",
      transportType: "item",
      httpMethod: "POST",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "void",
      },
    },
    getCaseSummary: {
      name: "getCaseSummary",
      displayName: "Get Case Summary",
      transportType: "item",
      httpMethod: "POST",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.CaseSummary as ObjectType) },
      },
    },
  },
  dataSources: {
    allOpenCases: {
      type: "dataSource",
      name: "allOpenCases",
      displayName: "All Open Cases",
      params: {
        minDate: {
          name: "minDate",
          displayName: "Min Date",
          type: "date",
          role: "value",
        },
      },
    },
  },
}
export const CaseDto = domain.types.CaseDto = {
  name: "caseDto",
  displayName: "Case Dto",
  get displayProp() { return this.props.caseId }, 
  type: "model",
  controllerRoute: "CaseDto",
  get keyProp() { return this.props.caseId }, 
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
        role: "value",
        type: "string",
      },
    },
  },
  dataSources: {
    caseDtoSource: {
      type: "dataSource",
      name: "caseDtoSource",
      displayName: "Case Dto Source",
      params: {
      },
    },
  },
}
export const CaseProduct = domain.types.CaseProduct = {
  name: "caseProduct",
  displayName: "Case Product",
  get displayProp() { return this.props.caseProductId }, 
  type: "model",
  controllerRoute: "CaseProduct",
  get keyProp() { return this.props.caseProductId }, 
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
    },
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      get typeDef() { return (domain.types.Case as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimaryKeyProperty },
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
  name: "company",
  displayName: "Company",
  get displayProp() { return this.props.altName }, 
  type: "model",
  controllerRoute: "Company",
  get keyProp() { return this.props.companyId }, 
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
        role: "value",
        type: "collection",
        itemType: {
          name: "$collectionItem",
          displayName: "",
          role: "value",
          type: "model",
          get typeDef() { return (domain.types.Company as ModelType) },
        },
      },
    },
  },
  dataSources: {
    defaultSource: {
      type: "dataSource",
      name: "defaultSource",
      displayName: "Default Source",
      params: {
      },
    },
  },
}
export const Person = domain.types.Person = {
  name: "person",
  displayName: "Person",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Person",
  get keyProp() { return this.props.personId }, 
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
      get typeDef() { return domain.enums.Nullable },
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
      dontSerialize: true,
    },
    birthDate: {
      name: "birthDate",
      displayName: "Birth Date",
      type: "date",
      role: "value",
    },
    lastBath: {
      name: "lastBath",
      displayName: "Last Bath",
      type: "date",
      role: "value",
    },
    nextUpgrade: {
      name: "nextUpgrade",
      displayName: "Next Upgrade",
      type: "date",
      role: "value",
    },
    personStats: {
      name: "personStats",
      displayName: "Person Stats",
      type: "object",
      get typeDef() { return (domain.types.PersonStats as ObjectType) },
      role: "value",
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
    },
    company: {
      name: "company",
      displayName: "Company",
      type: "model",
      get typeDef() { return (domain.types.Company as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId as ForeignKeyProperty },
      get principalKey() { return (domain.types.Company as ModelType).props.companyId as PrimaryKeyProperty },
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
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType) },
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
        role: "value",
        type: "void",
      },
    },
    add: {
      name: "add",
      displayName: "Add",
      transportType: "item",
      httpMethod: "POST",
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
        role: "value",
        type: "number",
      },
    },
    getUser: {
      name: "getUser",
      displayName: "Get User",
      transportType: "item",
      httpMethod: "POST",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "string",
      },
    },
    personCount: {
      name: "personCount",
      displayName: "Person Count",
      transportType: "item",
      httpMethod: "GET",
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
        role: "value",
        type: "number",
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
        role: "value",
        type: "string",
      },
    },
    removePersonById: {
      name: "removePersonById",
      displayName: "Remove Person By Id",
      transportType: "item",
      httpMethod: "DELETE",
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
        role: "value",
        type: "boolean",
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
        role: "value",
        type: "string",
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
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "model",
        get typeDef() { return (domain.types.Person as ModelType) },
      },
    },
    getUserPublic: {
      name: "getUserPublic",
      displayName: "Get User Public",
      transportType: "item",
      httpMethod: "POST",
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "string",
      },
    },
    namesStartingWith: {
      name: "namesStartingWith",
      displayName: "Names Starting With",
      transportType: "item",
      httpMethod: "POST",
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
        role: "value",
        type: "collection",
        itemType: {
          name: "$collectionItem",
          displayName: "",
          role: "value",
          type: "string",
        },
      },
    },
    searchPeople: {
      name: "searchPeople",
      displayName: "Search People",
      transportType: "list",
      httpMethod: "POST",
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
        role: "value",
        type: "collection",
        itemType: {
          name: "$collectionItem",
          displayName: "",
          role: "value",
          type: "model",
          get typeDef() { return (domain.types.Person as ModelType) },
        },
      },
    },
  },
  dataSources: {
    bOrCPeople: {
      type: "dataSource",
      name: "bOrCPeople",
      displayName: "B Or C People",
      params: {
      },
    },
    namesStartingWithAWithCases: {
      type: "dataSource",
      name: "namesStartingWithAWithCases",
      displayName: "Names Starting With A With Cases",
      params: {
      },
    },
    withoutCases: {
      type: "dataSource",
      name: "withoutCases",
      displayName: "Without Cases",
      isDefault: true,
      params: {
      },
    },
  },
}
export const Product = domain.types.Product = {
  name: "product",
  displayName: "Product",
  get displayProp() { return this.props.name }, 
  type: "model",
  controllerRoute: "Product",
  get keyProp() { return this.props.productId }, 
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
  name: "caseSummary",
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
  name: "devTeam",
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
  name: "location",
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
  name: "personCriteria",
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
  name: "personStats",
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
  name: "productDetails",
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
  name: "streetAddress",
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
  name: "weatherData",
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
  name: "weatherService",
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
          type: "date",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.WeatherData as ObjectType) },
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
          type: "date",
          role: "value",
        },
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.WeatherData as ObjectType) },
      },
    },
  },
}

interface AppDomain extends Domain {
  enums: {
    Genders: typeof Genders
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

export default domain as AppDomain
