
import { 
  Domain,
  ModelMetadata,
  ModelPropertyMetadata, 
  EnumPropertyMetadata ,
  NumberPropertyMetadata,
  EnumValues,
  getEnumMeta
} from './coalesce/metadata'

// Assigning each property as a member of domain ensures we don't break type contracts.
// Exporting each model individually ensures that we 
const domain: Domain = {};

enum Gender {
  Male,
  Female,
} 

export const Person = domain.Person = {
  name: "person",
  type: "model",
  displayName: "Person",
  get keyProp() { return this.props.personId },
  get displayProp() { return this.props.firstName },
  props: {
    personId: {
      name: "personId",
      type: "number", 
      role: "primaryKey",
      displayName: "Person ID",
      ctor: Number
    } as NumberPropertyMetadata,
    firstName: {
      name: "firstName",
      type: "string",
      role: "value",
      displayName: "First Name",
      ctor: String
    },
    lastName: {
      name: "lastName",
      type: "string",
      role: "value",
      displayName: "Last Name",
      ctor: String
    },
    gender: {
      name: "gender",
      type: "enum",
      role: "value",
      displayName: "Gender",
      enum: Gender,
      ...getEnumMeta<typeof Gender>([
        { value: Gender.Male, strValue: "Male", displayName: "Male" },
        { value: Gender.Female, strValue: "Female", displayName: "Female" }
      ])
    },
    companyId: {
      name: "companyId",
      type: "number",
      role: "foreignKey",
      displayName: "Company ID",
      ctor: Number
    },
    company: {
      name: "company",
      type: "model",
      role: "referenceNavigation",
      displayName: "Employer",
      get model() { return Company }
    },
    casesAssigned: {
      name: "casesAssigned",
      type: "collection",
      role: "collectionNavigation",
      displayName: "Cases Assigned",
      get model() { return Case }
    },
    casesReported: {
      name: "casesReported",
      type: "collection",
      role: "collectionNavigation",
      displayName: "Cases Reported",
      get model() { return Case }
    }
    // birthDate: {
    //   name: "birthDate",
    //   type: "date",
    //   displayName: "Birth Date",
    //   ctor: moment
    // },
  },
  methods: {},
};

export const Case = domain.Case = {
  name: "case",
  type: "model",
  displayName: "Case",
  get keyProp() { return this.props.caseId },
  get displayProp() { return this.props.title },
  props: {
    caseId: {
      name: "caseKey",
      type: "number",
      role: "primaryKey",
      displayName: "Case ID",
      ctor: Number,
    },
    assignedToId: {
      name: "assignedToId",
      type: "number",
      role: "foreignKey",
      displayName: "Assigned To ID",
      ctor: Number,
    },
    title: {
      name: "title",
      type: "string",
      role: "value",
      displayName: "Title",
      ctor: String,
    },

  },
  methods: {},
}

export const Company = domain.Company = {
  name: "company",
  type: "model",
  displayName: "Company",
  get keyProp() { return this.props.companyId },
  get displayProp() { return this.props.name },
  props: {
    companyId: <NumberPropertyMetadata> {
      name: "companyId",
      type: "number",
      role: "primaryKey",
      displayName: "Company ID",
      ctor: Number
    },
    name: {
      name: "name",
      type: "string",
      role: "value",
      displayName: "Name",
      ctor: String
    },
  },
  methods: {},
}

export default domain;