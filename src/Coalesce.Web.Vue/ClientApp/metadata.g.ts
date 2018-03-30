import { Domain, getEnumMeta, ModelType, ExternalType, PrimitiveProperty } from 'coalesce-vue/lib/metadata' 

const domain: Domain = { types: {}, enums: {} }
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
export const Genders = domain.enums.Genders = {
  name: "genders",
  displayName: "Genders",
  type: "enum",
  ...getEnumMeta<"NonSpecified"|"Male"|"Female">([
    { value: 0, strValue: 'NonSpecified', displayName: 'NonSpecified' },
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
    { value: 1, strValue: 'InProgress', displayName: 'InProgress' },
    { value: 2, strValue: 'Resolved', displayName: 'Resolved' },
    { value: 3, strValue: 'ClosedNoSolution', displayName: 'ClosedNoSolution' },
    { value: 4, strValue: 'Cancelled', displayName: 'Cancelled' },
  ]),
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
      get foreignKey() { return (domain.types.Case as ModelType).props.assignedToId as PrimitiveProperty },
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
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById as PrimitiveProperty },
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
      get typeDef() { return (domain.types.PersonStats as ExternalType) },
      role: "value",
    },
    name: {
      name: "name",
      displayName: "Name",
      type: "string",
      role: "value",
    },
    companyId: {
      name: "companyId",
      displayName: "Company Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Company as ModelType).props.companyId as PrimitiveProperty },
      get principalType() { return (domain.types.Company as ModelType) },
    },
    company: {
      name: "company",
      displayName: "Company",
      type: "model",
      get typeDef() { return (domain.types.Company as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId as PrimitiveProperty },
      get principalKey() { return (domain.types.Company as ModelType).props.companyId as PrimitiveProperty },
    },
  },
  methods: {
    rename: {
      name: "rename",
      displayName: "Rename",
      params: {
        name: {
          name: "name",
          displayName: "name",
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
      params: {
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
      params: {
        numberOne: {
          name: "numberOne",
          displayName: "number One",
          type: "number",
          role: "value",
        },
        numberTwo: {
          name: "numberTwo",
          displayName: "number Two",
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
      params: {
        lastNameStartsWith: {
          name: "lastNameStartsWith",
          displayName: "last Name Starts With",
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
      params: {
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
      params: {
        id: {
          name: "id",
          displayName: "id",
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
      params: {
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
      params: {
        firstName: {
          name: "firstName",
          displayName: "first Name",
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
      params: {
        characters: {
          name: "characters",
          displayName: "characters",
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
      params: {
        criteria: {
          name: "criteria",
          displayName: "criteria",
          type: "object",
          get typeDef() { return (domain.types.PersonCriteria as ExternalType) },
          role: "value",
        },
        page: {
          name: "page",
          displayName: "page",
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
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimitiveProperty },
      get principalType() { return (domain.types.Person as ModelType) },
    },
    assignedTo: {
      name: "assignedTo",
      displayName: "Assigned To",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.assignedToId as PrimitiveProperty },
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimitiveProperty },
    },
    reportedById: {
      name: "reportedById",
      displayName: "Reported By Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimitiveProperty },
      get principalType() { return (domain.types.Person as ModelType) },
    },
    reportedBy: {
      name: "reportedBy",
      displayName: "Reported By",
      type: "model",
      get typeDef() { return (domain.types.Person as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById as PrimitiveProperty },
      get principalKey() { return (domain.types.Person as ModelType).props.personId as PrimitiveProperty },
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
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as PrimitiveProperty },
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
      get typeDef() { return (domain.types.DevTeam as ExternalType) },
      role: "value",
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
      params: {
      },
      return: {
        name: "$return",
        displayName: "Result",
        role: "value",
        type: "object",
        get typeDef() { return (domain.types.CaseSummary as ExternalType) },
      },
    },
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
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId as PrimitiveProperty },
    },
    altName: {
      name: "altName",
      displayName: "Alt Name",
      type: "string",
      role: "value",
    },
  },
  methods: {
    getCertainItems: {
      name: "getCertainItems",
      displayName: "Get Certain Items",
      params: {
        isDeleted: {
          name: "isDeleted",
          displayName: "is Deleted",
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
      get typeDef() { return (domain.types.ProductDetails as ExternalType) },
      role: "value",
    },
  },
  methods: {
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
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimitiveProperty },
      get principalType() { return (domain.types.Case as ModelType) },
    },
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      get typeDef() { return (domain.types.Case as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId as PrimitiveProperty },
      get principalKey() { return (domain.types.Case as ModelType).props.caseKey as PrimitiveProperty },
    },
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "foreignKey",
      get principalKey() { return (domain.types.Product as ModelType).props.productId as PrimitiveProperty },
      get principalType() { return (domain.types.Product as ModelType) },
    },
    product: {
      name: "product",
      displayName: "Product",
      type: "model",
      get typeDef() { return (domain.types.Product as ModelType) },
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.productId as PrimitiveProperty },
      get principalKey() { return (domain.types.Product as ModelType).props.productId as PrimitiveProperty },
    },
  },
  methods: {
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
    },
  },
  methods: {
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
      get typeDef() { return (domain.types.StreetAddress as ExternalType) },
      role: "value",
    },
    companyHqAddress: {
      name: "companyHqAddress",
      displayName: "Company Hq Address",
      type: "object",
      get typeDef() { return (domain.types.StreetAddress as ExternalType) },
      role: "value",
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
      get typeDef() { return (domain.types.Location as ExternalType) },
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

interface AppDomain extends Domain {
  enums: {
    Titles: typeof Titles
    Genders: typeof Genders
    Statuses: typeof Statuses
  }
  types: {
    Person: typeof Person
    Case: typeof Case
    Company: typeof Company
    Product: typeof Product
    CaseProduct: typeof CaseProduct
    CaseDto: typeof CaseDto
    PersonCriteria: typeof PersonCriteria
    PersonStats: typeof PersonStats
    CaseSummary: typeof CaseSummary
    DevTeam: typeof DevTeam
    ProductDetails: typeof ProductDetails
    StreetAddress: typeof StreetAddress
    WeatherData: typeof WeatherData
    Location: typeof Location
  }
}

export default domain as AppDomain
