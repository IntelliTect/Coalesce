import { Domain, getEnumMeta, ModelType, ExternalType } from './coalesce/core/metadata' 

const domain: Domain = { types: {}, enums: {} }
export default domain
export const Titles = domain.enums.Titles = {
  name: "titles",
  displayName: "Titles",
  type: "enum",
  ...getEnumMeta([
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
  ...getEnumMeta([
    { value: 0, strValue: 'NonSpecified', displayName: 'NonSpecified' },
    { value: 1, strValue: 'Male', displayName: 'Male' },
    { value: 2, strValue: 'Female', displayName: 'Female' },
  ]),
}
export const Statuses = domain.enums.Statuses = {
  name: "statuses",
  displayName: "Statuses",
  type: "enum",
  ...getEnumMeta([
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
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.assignedToId },
      get typeDef() { return (domain.types.Case as ModelType) },
    },
    casesReported: {
      name: "casesReported",
      displayName: "Cases Reported",
      type: "collection",
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.reportedById },
      get typeDef() { return (domain.types.Case as ModelType) },
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
      role: "value",
      get typeDef() { return domain.types.PersonStats as ExternalType },
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
    },
    company: {
      name: "company",
      displayName: "Company",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Company as ModelType).props.companyId },
      get typeDef() { return (domain.types.Company as ModelType) },
    },
  },
  methods: {},
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
    },
    assignedTo: {
      name: "assignedTo",
      displayName: "Assigned To",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.assignedToId },
      get typeDef() { return (domain.types.Person as ModelType) },
    },
    reportedById: {
      name: "reportedById",
      displayName: "Reported By Id",
      type: "number",
      role: "foreignKey",
    },
    reportedBy: {
      name: "reportedBy",
      displayName: "Reported By",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.reportedById },
      get typeDef() { return (domain.types.Person as ModelType) },
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
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.CaseProduct as ModelType).props.caseId },
      get typeDef() { return (domain.types.CaseProduct as ModelType) },
    },
    devTeamAssignedId: {
      name: "devTeamAssignedId",
      displayName: "Dev Team Assigned Id",
      type: "number",
      role: "foreignKey",
    },
    devTeamAssigned: {
      name: "devTeamAssigned",
      displayName: "Dev Team Assigned",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.DevTeam as ModelType).props.devTeamAssignedId },
      get typeDef() { return (domain.types.DevTeam as ModelType) },
    },
    duration: {
      name: "duration",
      displayName: "Duration",
      type: "string",
      role: "value",
    },
  },
  methods: {},
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
      role: "collectionNavigation",
      get foreignKey() { return (domain.types.Person as ModelType).props.companyId },
      get typeDef() { return (domain.types.Person as ModelType) },
    },
    altName: {
      name: "altName",
      displayName: "Alt Name",
      type: "string",
      role: "value",
    },
  },
  methods: {},
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
  },
  methods: {},
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
    },
    case: {
      name: "case",
      displayName: "Case",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Case as ModelType).props.caseId },
      get typeDef() { return (domain.types.Case as ModelType) },
    },
    productId: {
      name: "productId",
      displayName: "Product Id",
      type: "number",
      role: "foreignKey",
    },
    product: {
      name: "product",
      displayName: "Product",
      type: "model",
      role: "referenceNavigation",
      get foreignKey() { return (domain.types.Product as ModelType).props.productId },
      get typeDef() { return (domain.types.Product as ModelType) },
    },
  },
  methods: {},
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
  methods: {},
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
  get displayProp() { return this.props.caseSummaryId }, 
  type: "object",
  props: {
    caseSummaryId: {
      name: "caseSummaryId",
      displayName: "Case Summary Id",
      type: "number",
      role: "primaryKey",
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
      role: "primaryKey",
    },
    name: {
      name: "name",
      displayName: "Name",
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
      role: "value",
      get typeDef() { return domain.types.Location as ExternalType },
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
